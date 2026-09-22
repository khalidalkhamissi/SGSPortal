using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SGSForms.Api.Auth;
using SGSForms.Api.Data;
using SGSForms.Api.Dtos;
using SGSForms.Api.Entities;
using SGSForms.Api.Enums;

namespace SGSForms.Api.Services;

public partial class CoordinationService : ICoordinationService
{
	private const int MaxDelayMinutes = 100_000;

	[GeneratedRegex("^(\\d{1,3}):([0-5]?\\d)$")]
	private static partial Regex HoursMinutesRegex();

	private readonly AppDbContext _db;

	private readonly CurrentUser _me;

	private readonly IAuditService _audit;

	public CoordinationService(AppDbContext db, CurrentUser me, IAuditService audit)
	{
		_db = db;
		_me = me;
		_audit = audit;
	}

	private static bool Has(string? s) => !string.IsNullOrWhiteSpace(s);

	/// <summary>Minutes from "H:MM" or a plain number of minutes; empty means 0.</summary>
	private static int MinutesFrom(string? s)
	{
		if (!Has(s))
		{
			return 0;
		}
		s = s!.Trim();
		Match m = HoursMinutesRegex().Match(s);
		int minutes;
		if (m.Success)
		{
			minutes = int.Parse(m.Groups[1].Value) * 60 + int.Parse(m.Groups[2].Value);
		}
		else if (!int.TryParse(s, out minutes))
		{
			throw new AppException("مقدار التأخير غير صالح: " + s);
		}
		if (minutes < 0 || minutes > MaxDelayMinutes)
		{
			throw new AppException("مقدار التأخير غير صالح: " + s);
		}
		return minutes;
	}

	private static string HhMm(int minutes) => $"{minutes / 60:00}:{minutes % 60:00}";

	private static string? T5(TimeOnly? t) => t?.ToString("HH:mm");

	/// <summary>Minutes from one clock time to the next, wrapping past midnight.</summary>
	public static int? Elapsed(TimeOnly? from, TimeOnly? to)
	{
		if (!from.HasValue || !to.HasValue)
		{
			return null;
		}
		int mins = (int)(to.Value.ToTimeSpan() - from.Value.ToTimeSpan()).TotalMinutes;
		return mins < 0 ? mins + 1440 : mins;
	}

	/// <summary>GAIN = time the departure left ahead of schedule (STD − ATD), never negative.</summary>
	private static string? Gain(TimeOnly? std, TimeOnly? atd)
	{
		if (!std.HasValue || !atd.HasValue)
		{
			return null;
		}
		int late = (int)(atd.Value.ToTimeSpan() - std.Value.ToTimeSpan()).TotalMinutes;
		if (late > 720)
		{
			late -= 1440;
		}
		if (late < -720)
		{
			late += 1440;
		}
		return HhMm(Math.Max(0, -late));
	}

	private static string FlightOf(CoordinationSheet r) => Has(r.ArrFlightNo) ? r.ArrFlightNo! : (r.DepFlightNo ?? "");

	private void RequireView()
	{
		if (!_me.HasAny(Permissions.CoordinationView, Permissions.CoordinationCreate))
		{
			throw AppException.Forbidden();
		}
	}

	public async Task<PagedReports> ListAsync(string? status, string? from, string? to, int? stationId, string? airline, string? q, int page, int pageSize)
	{
		RequireView();
		ReportStatus? st = Parse.OptionalStatus(status);
		(DateOnly? fromDay, DateOnly? toDay) = Parse.Range(from, to);
		(page, pageSize) = Parse.Paging(page, pageSize);
		int? scope = _me.StationScope(stationId);
		string? af = FlightNo.Filter(airline);
		(string inc, string? exc) = af != null ? FlightNo.SqlPatterns(af) : ("", null);
		string? term = Parse.Search(q);

		IQueryable<CoordinationSheet> query = _db.CoordinationSheets.AsNoTracking()
			.Where(c => (st == null || c.Status == st) && (scope == null || c.StationId == scope)
				&& (fromDay == null || c.FlightDate >= fromDay) && (toDay == null || c.FlightDate <= toDay)
				&& (af == null || (Regex.IsMatch((c.ArrFlightNo == null || c.ArrFlightNo == "") ? (c.DepFlightNo ?? "") : c.ArrFlightNo, inc)
					&& (exc == null || !Regex.IsMatch((c.ArrFlightNo == null || c.ArrFlightNo == "") ? (c.DepFlightNo ?? "") : c.ArrFlightNo, exc))))
				&& (term == null || (c.ArrFlightNo != null && c.ArrFlightNo.Contains(term)) || (c.DepFlightNo != null && c.DepFlightNo.Contains(term))
					|| (c.ArrFrom != null && c.ArrFrom.Contains(term)) || (c.DepTo != null && c.DepTo.Contains(term)) || c.Station.Code.Contains(term)));
		int total = await query.CountAsync();
		var rows = await query
			.OrderByDescending(c => c.FlightDate).ThenByDescending(c => c.Id)
			.Skip((page - 1) * pageSize).Take(pageSize)
			.Select(c => new
			{
				c.Id,
				FlightNo = (c.ArrFlightNo == null || c.ArrFlightNo == "") ? (c.DepFlightNo ?? "") : c.ArrFlightNo,
				ArrFrom = c.ArrFrom ?? "",
				DepTo = c.DepTo ?? "",
				c.FlightDate,
				StationCode = c.Station.Code,
				c.Status,
				c.DraftExpiresAt
			})
			.ToListAsync();
		DateTime now = DateTime.UtcNow;
		List<ReportSummary> items = rows.Select(r => new ReportSummary
		{
			Kind = "coordination",
			Id = r.Id,
			FlightNo = r.FlightNo,
			Route = string.Join(" - ", new[] { r.ArrFrom, r.DepTo }.Where(x => x.Length > 0)),
			FlightDate = r.FlightDate.ToString("yyyy-MM-dd"),
			StationCode = r.StationCode,
			Status = r.Status.ToString().ToLowerInvariant(),
			HoursLeft = r.Status == ReportStatus.Draft && r.DraftExpiresAt.HasValue
				? Math.Max(0, (int)Math.Ceiling((r.DraftExpiresAt.Value - now).TotalHours))
				: null
		}).ToList();
		List<string> flights = await _db.CoordinationSheets.AsNoTracking()
			.Where(c => (st == null || c.Status == st) && (scope == null || c.StationId == scope))
			.Select(c => (c.ArrFlightNo == null || c.ArrFlightNo == "") ? (c.DepFlightNo ?? "") : c.ArrFlightNo)
			.Distinct().ToListAsync();
		List<string> airlines = flights.Select(FlightNo.Prefix).Where(x => x.Length > 0).Distinct().OrderBy(x => x).ToList();
		return new PagedReports
		{
			Items = items,
			Total = total,
			Page = page,
			PageSize = pageSize,
			Airlines = airlines
		};
	}

	public async Task<object> SaveAsync(int? id, CoordinationInput dto)
	{
		Parse.MaxItems(dto.Activities, "الأنشطة");
		Parse.MaxItems(dto.BusesArrival, "باصات الوصول");
		Parse.MaxItems(dto.BusesDeparture, "باصات المغادرة");
		int stationId = await ReportRules.ResolveStationAsync(_db, _me, dto.StationId);

		CoordinationSheet r;
		if (!id.HasValue)
		{
			r = new CoordinationSheet
			{
				CreatedByUserId = _me.Id,
				CreatedAt = DateTime.UtcNow
			};
			_db.CoordinationSheets.Add(r);
		}
		else
		{
			r = await _db.CoordinationSheets.Include(x => x.Activities).Include(x => x.Buses)
				.FirstOrDefaultAsync(x => x.Id == id.Value) ?? throw AppException.NotFound("النموذج غير موجود");
			if (!_me.CanAccessStation(r.StationId))
			{
				throw AppException.Forbidden();
			}
			if (r.Status == ReportStatus.Approved)
			{
				throw new AppException("لا يمكن تعديل نموذج معتمد");
			}
			_db.CoordinationActivities.RemoveRange(r.Activities);
			_db.CoordinationBuses.RemoveRange(r.Buses);
		}
		r.StationId = stationId;
		r.FlightDate = Parse.Date(dto.FlightDate);
		r.AcType = Parse.Text(dto.AcType, 20, "نوع الطائرة");
		r.AcReg = Parse.Text(dto.AcReg, 20, "تسجيل الطائرة");
		r.ArrFlightNo = Parse.Text(dto.ArrFlightNo, 20, "رقم رحلة الوصول");
		r.ArrFrom = Parse.Text(dto.ArrFrom, 100, "قادمة من");
		r.ArrPaxF = Parse.Count(dto.ArrPaxF, "ركاب F");
		r.ArrPaxJ = Parse.Count(dto.ArrPaxJ, "ركاب J");
		r.ArrPaxY = Parse.Count(dto.ArrPaxY, "ركاب Y");
		r.DepFlightNo = Parse.Text(dto.DepFlightNo, 20, "رقم رحلة المغادرة");
		r.DepTo = Parse.Text(dto.DepTo, 100, "متجهة إلى");
		r.DepPaxF = Parse.Count(dto.DepPaxF, "ركاب F");
		r.DepPaxJ = Parse.Count(dto.DepPaxJ, "ركاب J");
		r.DepPaxY = Parse.Count(dto.DepPaxY, "ركاب Y");
		r.Turnaround = dto.Turnaround;
		r.Transit = dto.Transit;
		r.Terminating = dto.Terminating;
		r.Originating = dto.Originating;
		r.Sta = Parse.Time(dto.Sta);
		r.Ata = Parse.Time(dto.Ata);
		r.Std = Parse.Time(dto.Std);
		r.Atd = Parse.Time(dto.Atd);
		r.GainTime = r.Terminating ? null : Gain(r.Std, r.Atd);
		r.DlyAmount = MinutesFrom(dto.DlyAmount);
		r.DelayCode = Parse.Text(dto.DelayCode, 20, "كود التأخير");
		r.SupervisorName = _me.Name;
		r.ReportTime = DateTime.UtcNow;
		r.Remarks = Parse.Text(dto.Remarks, 2000, "الملاحظات");
		r.UpdatedAt = DateTime.UtcNow;
		if (dto.Submit && r.ArrFlightNo == null && r.DepFlightNo == null)
		{
			throw new AppException("يرجى تعبئة رقم رحلة واحد على الأقل (وصول أو مغادرة)");
		}
		// One row per known activity; later duplicates of the same key are ignored.
		r.Activities = (dto.Activities ?? new())
			.Where(a => Has(a.Key) && CoordinationActivities.IsKnown(a.Key.Trim())
				&& (Has(a.ActualStart) || Has(a.ActualFinish) || Has(a.Remarks)))
			.DistinctBy(a => a.Key.Trim())
			.Select(a => new CoordinationActivity
			{
				ActivityKey = a.Key.Trim(),
				ActualStart = Parse.Time(a.ActualStart),
				ActualFinish = Parse.Time(a.ActualFinish),
				Remarks = Parse.Text(a.Remarks, 500, "ملاحظات النشاط")
			}).ToList();
		r.Buses = Buses(dto.BusesArrival, "arrival").Concat(Buses(dto.BusesDeparture, "departure")).ToList();
		if (dto.Submit)
		{
			r.Status = ReportStatus.Submitted;
			r.SubmittedAt = DateTime.UtcNow;
			r.DraftExpiresAt = null;
		}
		else
		{
			r.Status = ReportStatus.Draft;
			r.DraftExpiresAt = r.CreatedAt.AddHours(ReportRules.DraftLifetimeHours);
		}
		await _db.SaveChangesAsync();
		await _audit.LogAsync(ReportRules.SaveAction(dto.Submit, !id.HasValue), "coordination", r.Id, FlightOf(r), null, r.StationId);
		return new
		{
			r.Id,
			status = r.Status.ToString().ToLowerInvariant(),
			activities = r.Activities.Count,
			buses = r.Buses.Count
		};
	}

	private static IEnumerable<CoordinationBus> Buses(List<CoordBusDto>? buses, string phase)
	{
		return (buses ?? new())
			.Where(x => Has(x.BusNo) || Has(x.Time))
			.Select(x => new CoordinationBus
			{
				Phase = phase,
				BusNo = Parse.Text(x.BusNo, 20, "رقم الباص"),
				Time = Parse.Time(x.Time)
			});
	}

	public async Task<object> GetAsync(int id)
	{
		RequireView();
		CoordinationSheet r = await _db.CoordinationSheets.AsNoTracking()
			.Include(x => x.Station).Include(x => x.Activities).Include(x => x.Buses)
			.FirstOrDefaultAsync(x => x.Id == id) ?? throw AppException.NotFound("النموذج غير موجود");
		if (!_me.CanAccessStation(r.StationId))
		{
			throw AppException.Forbidden();
		}
		return ToOutput(r);
	}

	private static CoordinationOutput ToOutput(CoordinationSheet r)
	{
		return new CoordinationOutput
		{
			Id = r.Id,
			Status = r.Status.ToString().ToLowerInvariant(),
			StationCode = r.Station?.Code ?? "",
			SupervisorName = r.SupervisorName,
			ReportTime = r.ReportTime?.ToString("yyyy-MM-dd HH:mm"),
			FlightDate = r.FlightDate.ToString("yyyy-MM-dd"),
			AcType = r.AcType,
			AcReg = r.AcReg,
			ArrFlightNo = r.ArrFlightNo,
			ArrFrom = r.ArrFrom,
			ArrPaxF = r.ArrPaxF,
			ArrPaxJ = r.ArrPaxJ,
			ArrPaxY = r.ArrPaxY,
			DepFlightNo = r.DepFlightNo,
			DepTo = r.DepTo,
			DepPaxF = r.DepPaxF,
			DepPaxJ = r.DepPaxJ,
			DepPaxY = r.DepPaxY,
			Turnaround = r.Turnaround,
			Transit = r.Transit,
			Terminating = r.Terminating,
			Originating = r.Originating,
			Sta = T5(r.Sta),
			Ata = T5(r.Ata),
			Std = T5(r.Std),
			Atd = T5(r.Atd),
			GainTime = r.GainTime,
			DlyAmount = r.DlyAmount > 0 ? HhMm(r.DlyAmount) : null,
			DelayCode = r.DelayCode,
			Remarks = r.Remarks,
			Activities = r.Activities.Select(a => new CoordActivityDto
			{
				Key = a.ActivityKey,
				ActualStart = T5(a.ActualStart),
				ActualFinish = T5(a.ActualFinish),
				Duration = Elapsed(a.ActualStart, a.ActualFinish) is int m ? HhMm(m) : null,
				Remarks = a.Remarks
			}).ToList(),
			BusesArrival = r.Buses.Where(b => b.Phase == "arrival").Select(b => new CoordBusDto { BusNo = b.BusNo, Time = T5(b.Time) }).ToList(),
			BusesDeparture = r.Buses.Where(b => b.Phase == "departure").Select(b => new CoordBusDto { BusNo = b.BusNo, Time = T5(b.Time) }).ToList()
		};
	}

	public async Task DeleteAsync(int id)
	{
		CoordinationSheet r = await _db.CoordinationSheets.FirstOrDefaultAsync(x => x.Id == id) ?? throw AppException.NotFound("النموذج غير موجود");
		if (!_me.CanAccessStation(r.StationId))
		{
			throw AppException.Forbidden();
		}
		if (r.Status == ReportStatus.Approved)
		{
			throw new AppException("لا يمكن حذف نموذج معتمد");
		}
		_db.CoordinationSheets.Remove(r);
		await _db.SaveChangesAsync();
		await _audit.LogAsync("delete", "coordination", id, FlightOf(r), null, r.StationId);
	}
}
