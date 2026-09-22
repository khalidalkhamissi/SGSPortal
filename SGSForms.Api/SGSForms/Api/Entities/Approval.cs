using System;
using SGSForms.Api.Enums;

namespace SGSForms.Api.Entities;

public class Approval
{
	public int Id { get; set; }

	public ReportKind ReportKind { get; set; }

	public int StationId { get; set; }

	public int? ArrivalReportId { get; set; }

	public ArrivalReport? ArrivalReport { get; set; }

	public int? DepartureReportId { get; set; }

	public DepartureReport? DepartureReport { get; set; }

	public int Satisfaction { get; set; }

	public string? AirlineRemarks { get; set; }

	public string AirlineRepName { get; set; } = "";

	public string AirlineCompany { get; set; } = "";

	public string? AirlineRepPosition { get; set; }

	public string? SignatureImage { get; set; }

	public ApprovalDecision Decision { get; set; }

	public string? ReturnReason { get; set; }

	public int ApprovedByUserId { get; set; }

	public DateTime ApprovedAt { get; set; } = DateTime.UtcNow;
}
