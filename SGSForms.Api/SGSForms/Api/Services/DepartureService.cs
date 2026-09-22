using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SGSForms.Api.Auth;
using SGSForms.Api.Data;
using SGSForms.Api.Dtos;
using SGSForms.Api.Entities;
using SGSForms.Api.Enums;

namespace SGSForms.Api.Services;

public class DepartureService : IDepartureService
{
	private readonly AppDbContext _db;

	private readonly CurrentUser _me;

	private readonly IAuditService _audit;

	public DepartureService(AppDbContext db, CurrentUser me, IAuditService audit)
	{
		_db = db;
		_me = me;
		_audit = audit;
	}

	public async Task<object> SaveAsync(int? id, DepartureInput dto)
	{
		string flightNo = Parse.Required(dto.FlightNo, 20, "رقم الرحلة");
		string route = Parse.Required(dto.Route, 100, "الوجهة");
		Parse.MaxItems(dto.Delays, "التأخيرات");
		Parse.MaxItems(dto.Staff, "إنتاجية الموظفين");
		if (dto.ExcessSales < 0 || dto.ExcessSales > 9_999_999_999m)
		{
			throw new AppException("مبيعات الوزن الزائد: قيمة غير صالحة");
		}
		int stationId = await ReportRules.ResolveStationAsync(_db, _me, dto.StationId);

		DepartureReport r;
		if (!id.HasValue)
		{
			r = new DepartureReport
			{
				CreatedByUserId = _me.Id,
				CreatedAt = DateTime.UtcNow
			};
			_db.DepartureReports.Add(r);
		}
		else
		{
			r = await _db.DepartureReports.Include(x => x.Delays).Include(x => x.StaffProductivity)
				.FirstOrDefaultAsync(x => x.Id == id.Value) ?? throw AppException.NotFound("التقرير غير موجود");
			if (!_me.CanAccessStation(r.StationId))
			{
				throw AppException.Forbidden();
			}
			if (r.Status == ReportStatus.Approved)
			{
				throw new AppException("لا يمكن تعديل تقرير معتمد");
			}
			_db.DepartureDelays.RemoveRange(r.Delays);
			_db.StaffProductivity.RemoveRange(r.StaffProductivity);
		}
		r.StationId = stationId;
		r.FlightNo = flightNo;
		r.Route = route;
		r.AircraftReg = Parse.Text(dto.AircraftReg, 20, "تسجيل الطائرة");
		r.FlightDate = Parse.Date(dto.FlightDate);
		r.Std = Parse.Time(dto.Std);
		r.Atd = Parse.Time(dto.Atd);
		r.CounterNo = Parse.Text(dto.CounterNo, 50, "رقم الكاونتر");
		r.CountersStartedAt = Parse.Time(dto.CountersStartedAt);
		r.SupervisorName = _me.Name;
		r.ReportTime = DateTime.UtcNow;
		r.PaxExec = Parse.Count(dto.PaxExec, "Altanfeethi");
		r.PaxF = Parse.Count(dto.PaxF, "ركاب F");
		r.PaxJ = Parse.Count(dto.PaxJ, "ركاب J");
		r.PaxW = Parse.Count(dto.PaxW, "ركاب W");
		r.PaxY = Parse.Count(dto.PaxY, "ركاب Y");
		r.PaxInf = Parse.Count(dto.PaxInf, "INF");
		r.PaxHajj = Parse.Count(dto.PaxHajj, "ركاب الحج");
		r.PaxVip = Parse.Count(dto.PaxVip, "VIP");
		// Infants travel on an adult's seat and are not counted in the passenger total.
		r.PaxTotal = r.PaxF + r.PaxJ + r.PaxW + r.PaxY + r.PaxHajj + r.PaxExec + r.PaxVip;
		r.BagNormal = Parse.Count(dto.BagNormal, "الحقائب المسجلة");
		r.BagWchr = Parse.Count(dto.BagWchr, "WCHR");
		r.BagCbbg = Parse.Count(dto.BagCbbg, "CBBG");
		r.BagStcr = Parse.Count(dto.BagStcr, "STCR");
		r.BagAvih = Parse.Count(dto.BagAvih, "AVIH");
		r.BagVip = Parse.Count(dto.BagVip, "VIP/CIP");
		r.BagZamzam = Parse.Count(dto.BagZamzam, "زمزم");
		r.BagHajj = Parse.Count(dto.BagHajj, "حقائب الحج");
		r.BagTotal = r.BagNormal + r.BagWchr + r.BagCbbg + r.BagStcr + r.BagAvih + r.BagVip + r.BagZamzam + r.BagHajj;
		r.ExcessTickets = Parse.Count(dto.ExcessTickets, "تذاكر الوزن الزائد");
		r.ExcessSales = decimal.Round(dto.ExcessSales, 2);
		r.BoardingGate = Parse.Text(dto.BoardingGate, 20, "البوابة");
		r.BoardingMode = dto.BoardingMode?.Trim().ToLowerInvariant() switch
		{
			"bus" => BoardingMode.Bus,
			"jetway" => BoardingMode.Jetway,
			_ => null,
		};
		r.BoardingStarted = Parse.Time(dto.BoardingStarted);
		r.BoardingCompleted = Parse.Time(dto.BoardingCompleted);
		r.GateOpened = Parse.Time(dto.GateOpened);
		r.GateClosed = Parse.Time(dto.GateClosed);
		r.TotalBuses = Parse.Count(dto.TotalBuses, "عدد الباصات");
		r.SpecialHandling = Parse.Text(dto.SpecialHandling, 500, "المناولة الخاصة");
		r.ActualPax = Parse.Count(dto.ActualPax, "العدد الفعلي");
		r.NoShow = Parse.Count(dto.NoShow, "عدم الحضور");
		r.Offloaded = Parse.Count(dto.Offloaded, "المُنزَلون");
		r.Remarks = Parse.Text(dto.Remarks, 2000, "الملاحظات");
		r.UpdatedAt = DateTime.UtcNow;
		r.Delays = (dto.Delays ?? new())
			.Where(d => !string.IsNullOrWhiteSpace(d.Code) || !string.IsNullOrWhiteSpace(d.Reason) || d.DurationMinutes != 0)
			.Select(d => new DepartureDelay
			{
				Code = Parse.Text(d.Code, 20, "كود التأخير"),
				Reason = Parse.Text(d.Reason, 500, "سبب التأخير"),
				DurationMinutes = Parse.Count(d.DurationMinutes, "مدة التأخير")
			}).ToList();
		r.StaffProductivity = (dto.Staff ?? new())
			.Where(s => !string.IsNullOrWhiteSpace(s.StaffName))
			.Select(s => new StaffProductivity
			{
				StaffName = Parse.Required(s.StaffName, 100, "اسم الموظف"),
				PassengersServed = Parse.Count(s.PassengersServed, "عدد المسافرين"),
				Comments = Parse.Text(s.Comments, 500, "ملاحظات الموظف")
			}).ToList();
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
		await _audit.LogAsync(ReportRules.SaveAction(dto.Submit, !id.HasValue), "departure", r.Id, r.FlightNo, null, r.StationId);
		return new
		{
			r.Id,
			status = r.Status.ToString().ToLowerInvariant(),
			paxTotal = r.PaxTotal,
			bagTotal = r.BagTotal
		};
	}

	public async Task<object> GetAsync(int id)
	{
		if (!_me.HasAny(Permissions.ReportReaders))
		{
			throw AppException.Forbidden();
		}
		DepartureReport r = await _db.DepartureReports.AsNoTracking()
			.Include(x => x.Station).Include(x => x.Delays).Include(x => x.StaffProductivity)
			.FirstOrDefaultAsync(x => x.Id == id) ?? throw AppException.NotFound("التقرير غير موجود");
		if (!_me.CanAccessStation(r.StationId))
		{
			throw AppException.Forbidden();
		}
		return r;
	}

	public async Task DeleteAsync(int id)
	{
		DepartureReport r = await _db.DepartureReports.FirstOrDefaultAsync(x => x.Id == id) ?? throw AppException.NotFound("التقرير غير موجود");
		if (!_me.CanAccessStation(r.StationId))
		{
			throw AppException.Forbidden();
		}
		if (r.Status == ReportStatus.Approved)
		{
			throw new AppException("لا يمكن حذف تقرير معتمد وموقّع");
		}
		_db.DepartureReports.Remove(r);
		await _db.SaveChangesAsync();
		await _audit.LogAsync("delete", "departure", id, r.FlightNo, null, r.StationId);
	}
}
