using System;
using System.Collections.Generic;
using SGSForms.Api.Enums;

namespace SGSForms.Api.Entities;

public class CoordinationSheet
{
	public int Id { get; set; }

	public int StationId { get; set; }

	public Station Station { get; set; } = null!;

	public ReportStatus Status { get; set; }

	public DateOnly FlightDate { get; set; }

	public string? AcType { get; set; }

	public string? AcReg { get; set; }

	public string? ArrFlightNo { get; set; }

	public string? ArrFrom { get; set; }

	public int ArrPaxF { get; set; }

	public int ArrPaxJ { get; set; }

	public int ArrPaxY { get; set; }

	public string? DepFlightNo { get; set; }

	public string? DepTo { get; set; }

	public int DepPaxF { get; set; }

	public int DepPaxJ { get; set; }

	public int DepPaxY { get; set; }

	public bool Turnaround { get; set; }

	public bool Transit { get; set; }

	public bool Terminating { get; set; }

	public bool Originating { get; set; }

	public TimeOnly? Sta { get; set; }

	public TimeOnly? Ata { get; set; }

	public TimeOnly? Std { get; set; }

	public TimeOnly? Atd { get; set; }

	public string? GainTime { get; set; }

	public int DlyAmount { get; set; }

	public string? DelayCode { get; set; }

	public string? SupervisorName { get; set; }

	public DateTime? ReportTime { get; set; }

	public string? Remarks { get; set; }

	public int CreatedByUserId { get; set; }

	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

	public DateTime? SubmittedAt { get; set; }

	public DateTime? DraftExpiresAt { get; set; }

	public ICollection<CoordinationActivity> Activities { get; set; } = new List<CoordinationActivity>();

	public ICollection<CoordinationBus> Buses { get; set; } = new List<CoordinationBus>();
}
