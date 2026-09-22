using System;
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

public partial class ApprovalService : IApprovalService
{
	/// <summary>Largest accepted signature image (base64 text), ~1.5 MB of PNG.</summary>
	private const int MaxSignatureChars = 2_000_000;

	[GeneratedRegex("^data:image/(png|jpeg);base64,[A-Za-z0-9+/]+={0,2}$")]
	private static partial Regex SignatureRegex();

	private readonly AppDbContext _db;

	private readonly CurrentUser _me;

	private readonly IAuditService _audit;

	public ApprovalService(AppDbContext db, CurrentUser me, IAuditService audit)
	{
		_db = db;
		_me = me;
		_audit = audit;
	}

	public async Task<object> ApproveAsync(ApprovalInput dto)
	{
		ReportKind kind = Parse.Kind(dto.Kind);
		if (dto.Satisfaction < 1 || dto.Satisfaction > 5)
		{
			throw new AppException("يرجى اختيار تقييم الرضا");
		}
		string signature = dto.Signature?.Trim() ?? "";
		if (signature.Length == 0)
		{
			throw new AppException("التوقيع مطلوب");
		}
		if (signature.Length > MaxSignatureChars || !SignatureRegex().IsMatch(signature))
		{
			throw new AppException("صورة التوقيع غير صالحة");
		}
		string repName = Parse.Required(dto.Rep?.Name, 150, "اسم الموظف");
		string repAirline = Parse.Required(dto.Rep?.Airline, 100, "شركة الطيران");
		string? repPosition = Parse.Text(dto.Rep?.Position, 150, "الوظيفة");
		string? remarks = Parse.Text(dto.AirlineRemarks, 1000, "ملاحظات شركة الطيران");

		int stationId = await DecideAsync(kind, dto.ReportId, ReportStatus.Approved);
		Approval approval = new Approval
		{
			ReportKind = kind,
			StationId = stationId,
			ArrivalReportId = kind == ReportKind.Arrival ? dto.ReportId : null,
			DepartureReportId = kind == ReportKind.Departure ? dto.ReportId : null,
			Satisfaction = dto.Satisfaction,
			AirlineRemarks = remarks,
			AirlineRepName = repName,
			AirlineCompany = repAirline,
			AirlineRepPosition = repPosition,
			SignatureImage = signature,
			Decision = ApprovalDecision.Approved,
			ApprovedByUserId = _me.Id,
			ApprovedAt = DateTime.UtcNow
		};
		_db.Approvals.Add(approval);
		await _db.SaveChangesAsync();
		await _audit.LogAsync("approve", KindKey(kind), dto.ReportId, null, $"Rating {dto.Satisfaction}/5", stationId);
		return new { approval.Id };
	}

	public async Task ReturnAsync(string kind, int reportId, string? reason)
	{
		ReportKind k = Parse.Kind(kind);
		string text = Parse.Required(reason, 1000, "سبب الإرجاع");
		int stationId = await DecideAsync(k, reportId, ReportStatus.Returned);
		_db.Approvals.Add(new Approval
		{
			ReportKind = k,
			StationId = stationId,
			ArrivalReportId = k == ReportKind.Arrival ? reportId : null,
			DepartureReportId = k == ReportKind.Departure ? reportId : null,
			Satisfaction = 0,
			Decision = ApprovalDecision.Returned,
			ReturnReason = text,
			AirlineRepName = _me.Name,
			AirlineCompany = "-",
			ApprovedByUserId = _me.Id,
			ApprovedAt = DateTime.UtcNow
		});
		await _db.SaveChangesAsync();
		await _audit.LogAsync("return", KindKey(k), reportId, null, text, stationId);
	}

	public async Task<object?> GetForReportAsync(string kind, int reportId)
	{
		ReportKind k = Parse.Kind(kind);
		if (!_me.HasAny(Permissions.ReportReaders))
		{
			throw AppException.Forbidden();
		}
		Approval? approval = await _db.Approvals.AsNoTracking()
			.Where(x => x.ReportKind == k && (k == ReportKind.Arrival ? x.ArrivalReportId == reportId : x.DepartureReportId == reportId))
			.OrderByDescending(x => x.ApprovedAt)
			.FirstOrDefaultAsync();
		if (approval == null)
		{
			return null;
		}
		if (!_me.CanAccessStation(approval.StationId))
		{
			throw AppException.Forbidden();
		}
		return new
		{
			decision = approval.Decision == ApprovalDecision.Returned ? "returned" : "approved",
			returnReason = approval.ReturnReason,
			satisfaction = approval.Satisfaction,
			airlineRemarks = approval.AirlineRemarks,
			repName = approval.AirlineRepName,
			repAirline = approval.AirlineCompany,
			repPosition = approval.AirlineRepPosition,
			signature = approval.SignatureImage,
			approvedAt = approval.ApprovedAt.ToString("yyyy-MM-dd HH:mm")
		};
	}

	private static string KindKey(ReportKind kind) => kind == ReportKind.Arrival ? "arrival" : "departure";

	/// <summary>
	/// Moves a submitted report to its decided status. Only reports awaiting approval can be approved or
	/// returned, so a report is never approved twice nor approved while still a draft.
	/// </summary>
	private async Task<int> DecideAsync(ReportKind kind, int reportId, ReportStatus decided)
	{
		(ReportStatus status, int stationId, Action<ReportStatus> set) = kind == ReportKind.Arrival
			? await LoadAsync(_db.ArrivalReports, reportId)
			: await LoadAsync(_db.DepartureReports, reportId);
		if (!_me.CanAccessStation(stationId))
		{
			throw AppException.Forbidden();
		}
		if (status != ReportStatus.Submitted)
		{
			throw new AppException("لا يمكن اعتماد أو إرجاع إلا التقارير المرسلة بانتظار الموافقة", 409);
		}
		set(decided);
		return stationId;
	}

	private static async Task<(ReportStatus, int, Action<ReportStatus>)> LoadAsync(DbSet<ArrivalReport> set, int id)
	{
		ArrivalReport r = await set.FirstOrDefaultAsync(x => x.Id == id) ?? throw AppException.NotFound("التقرير غير موجود");
		return (r.Status, r.StationId, s => { r.Status = s; r.UpdatedAt = DateTime.UtcNow; });
	}

	private static async Task<(ReportStatus, int, Action<ReportStatus>)> LoadAsync(DbSet<DepartureReport> set, int id)
	{
		DepartureReport r = await set.FirstOrDefaultAsync(x => x.Id == id) ?? throw AppException.NotFound("التقرير غير موجود");
		return (r.Status, r.StationId, s => { r.Status = s; r.UpdatedAt = DateTime.UtcNow; });
	}
}
