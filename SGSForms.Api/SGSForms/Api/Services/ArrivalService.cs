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

public class ArrivalService : IArrivalService
{
	private readonly AppDbContext _db;

	private readonly CurrentUser _me;

	private readonly IAuditService _audit;

	public ArrivalService(AppDbContext db, CurrentUser me, IAuditService audit)
	{
		_db = db;
		_me = me;
		_audit = audit;
	}

	public async Task<object> SaveAsync(int? id, ArrivalInput dto)
	{
		string flightNo = Parse.Required(dto.FlightNo, 20, "رقم الرحلة");
		string route = Parse.Required(dto.Route, 100, "الوجهة");
		Parse.MaxItems(dto.Services, "الخدمات الإضافية");
		Parse.MaxItems(dto.Delays, "التأخيرات");
		int stationId = await ReportRules.ResolveStationAsync(_db, _me, dto.StationId);

		ArrivalReport r;
		if (!id.HasValue)
		{
			r = new ArrivalReport
			{
				CreatedByUserId = _me.Id,
				CreatedAt = DateTime.UtcNow
			};
			_db.ArrivalReports.Add(r);
		}
		else
		{
			r = await _db.ArrivalReports.Include(x => x.Services).Include(x => x.Delays)
				.FirstOrDefaultAsync(x => x.Id == id.Value) ?? throw AppException.NotFound("التقرير غير موجود");
			if (!_me.CanAccessStation(r.StationId))
			{
				throw AppException.Forbidden();
			}
			if (r.Status == ReportStatus.Approved)
			{
				throw new AppException("لا يمكن تعديل تقرير معتمد");
			}
			_db.ArrivalServices.RemoveRange(r.Services);
			_db.ArrivalDelays.RemoveRange(r.Delays);
		}
		r.StationId = stationId;
		r.FlightNo = flightNo;
		r.Route = route;
		r.AircraftReg = Parse.Text(dto.AircraftReg, 20, "تسجيل الطائرة");
		r.FlightDate = Parse.Date(dto.FlightDate);
		r.Sta = Parse.Time(dto.Sta);
		r.Ata = Parse.Time(dto.Ata);
		r.SupervisorName = _me.Name;
		r.ReportTime = DateTime.UtcNow;
		r.GateNo = Parse.Text(dto.GateNo, 20, "رقم البوابة");
		r.GateOpen = Parse.Time(dto.GateOpen);
		r.GateClose = Parse.Time(dto.GateClose);
		r.FirstPaxTime = Parse.Time(dto.FirstPaxTime);
		r.LastPaxTime = Parse.Time(dto.LastPaxTime);
		r.TtlPaxArr = Parse.Count(dto.TtlPaxArr, "إجمالي الركاب");
		r.TtlWchr = Parse.Count(dto.TtlWchr, "الكراسي المتحركة");
		r.PaxVip = Parse.Count(dto.PaxVip, "VIP");
		r.BagTotal = Parse.Count(dto.BagTotal, "الحقائب");
		r.ActualPax = Parse.Count(dto.ActualPax, "العدد الفعلي");
		r.NoShow = Parse.Count(dto.NoShow, "عدم الحضور");
		r.Offloaded = Parse.Count(dto.Offloaded, "المُنزَلون");
		r.Remarks = Parse.Text(dto.Remarks, 2000, "الملاحظات");
		r.UpdatedAt = DateTime.UtcNow;
		r.Services = (dto.Services ?? new())
			.Where(s => !string.IsNullOrWhiteSpace(s.Name))
			.Select(s => new SGSForms.Api.Entities.ArrivalService
			{
				Name = Parse.Required(s.Name, 100, "اسم الخدمة"),
				Count = Parse.Count(s.Count, "عدد الخدمة")
			}).ToList();
		r.Delays = (dto.Delays ?? new())
			.Where(d => !string.IsNullOrWhiteSpace(d.Code) || !string.IsNullOrWhiteSpace(d.Reason) || d.DurationMinutes != 0)
			.Select(d => new ArrivalDelay
			{
				Code = Parse.Text(d.Code, 20, "كود التأخير"),
				Reason = Parse.Text(d.Reason, 500, "سبب التأخير"),
				DurationMinutes = Parse.Count(d.DurationMinutes, "مدة التأخير")
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
		await _audit.LogAsync(ReportRules.SaveAction(dto.Submit, !id.HasValue), "arrival", r.Id, r.FlightNo, null, r.StationId);
		return new
		{
			r.Id,
			status = r.Status.ToString().ToLowerInvariant()
		};
	}

	public async Task<object> GetAsync(int id)
	{
		if (!_me.HasAny(Permissions.ReportReaders))
		{
			throw AppException.Forbidden();
		}
		ArrivalReport r = await _db.ArrivalReports.AsNoTracking()
			.Include(x => x.Station).Include(x => x.Services).Include(x => x.Delays)
			.FirstOrDefaultAsync(x => x.Id == id) ?? throw AppException.NotFound("التقرير غير موجود");
		if (!_me.CanAccessStation(r.StationId))
		{
			throw AppException.Forbidden();
		}
		return r;
	}

	public async Task DeleteAsync(int id)
	{
		ArrivalReport r = await _db.ArrivalReports.FirstOrDefaultAsync(x => x.Id == id) ?? throw AppException.NotFound("التقرير غير موجود");
		if (!_me.CanAccessStation(r.StationId))
		{
			throw AppException.Forbidden();
		}
		if (r.Status == ReportStatus.Approved)
		{
			throw new AppException("لا يمكن حذف تقرير معتمد وموقّع");
		}
		_db.ArrivalReports.Remove(r);
		await _db.SaveChangesAsync();
		await _audit.LogAsync("delete", "arrival", id, r.FlightNo, null, r.StationId);
	}
}
