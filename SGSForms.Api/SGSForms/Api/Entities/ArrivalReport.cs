using System;
using System.Collections.Generic;
using SGSForms.Api.Enums;

namespace SGSForms.Api.Entities;

public class ArrivalReport
{
	public int Id { get; set; }

	public int StationId { get; set; }

	public Station Station { get; set; } = null!;

	public ReportStatus Status { get; set; }

	public string FlightNo { get; set; } = "";

	public string Route { get; set; } = "";

	public string? AircraftReg { get; set; }

	public DateOnly FlightDate { get; set; }

	public TimeOnly? Sta { get; set; }

	public TimeOnly? Ata { get; set; }

	public string? SupervisorName { get; set; }

	public DateTime? ReportTime { get; set; }

	public string? GateNo { get; set; }

	public TimeOnly? GateOpen { get; set; }

	public TimeOnly? GateClose { get; set; }

	public TimeOnly? FirstPaxTime { get; set; }

	public TimeOnly? LastPaxTime { get; set; }

	public int TtlPaxArr { get; set; }

	public int TtlWchr { get; set; }

	public int PaxVip { get; set; }

	public int BagTotal { get; set; }

	public int ActualPax { get; set; }

	public int NoShow { get; set; }

	public int Offloaded { get; set; }

	public string? Remarks { get; set; }

	public int CreatedByUserId { get; set; }

	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

	public DateTime? SubmittedAt { get; set; }

	public DateTime? DraftExpiresAt { get; set; }

	public ICollection<ArrivalService> Services { get; set; } = new List<ArrivalService>();

	public ICollection<ArrivalDelay> Delays { get; set; } = new List<ArrivalDelay>();
}
