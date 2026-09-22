using System;
using System.Collections.Generic;
using SGSForms.Api.Enums;

namespace SGSForms.Api.Entities;

public class DepartureReport
{
	public int Id { get; set; }

	public int StationId { get; set; }

	public Station Station { get; set; } = null!;

	public ReportStatus Status { get; set; }

	public string FlightNo { get; set; } = "";

	public string Route { get; set; } = "";

	public string? AircraftReg { get; set; }

	public DateOnly FlightDate { get; set; }

	public TimeOnly? Std { get; set; }

	public TimeOnly? Atd { get; set; }

	public string? CounterNo { get; set; }

	public TimeOnly? CountersStartedAt { get; set; }

	public string? SupervisorName { get; set; }

	public DateTime? ReportTime { get; set; }

	public int PaxExec { get; set; }

	public int PaxF { get; set; }

	public int PaxJ { get; set; }

	public int PaxW { get; set; }

	public int PaxY { get; set; }

	public int PaxInf { get; set; }

	public int PaxHajj { get; set; }

	public int PaxVip { get; set; }

	public int PaxTotal { get; set; }

	public int BagNormal { get; set; }

	public int BagWchr { get; set; }

	public int BagCbbg { get; set; }

	public int BagStcr { get; set; }

	public int BagAvih { get; set; }

	public int BagVip { get; set; }

	public int BagZamzam { get; set; }

	public int BagHajj { get; set; }

	public int BagTotal { get; set; }

	public int ExcessTickets { get; set; }

	public decimal ExcessSales { get; set; }

	public string? BoardingGate { get; set; }

	public BoardingMode? BoardingMode { get; set; }

	public TimeOnly? BoardingStarted { get; set; }

	public TimeOnly? BoardingCompleted { get; set; }

	public TimeOnly? GateOpened { get; set; }

	public TimeOnly? GateClosed { get; set; }

	public int TotalBuses { get; set; }

	public string? SpecialHandling { get; set; }

	public int ActualPax { get; set; }

	public int NoShow { get; set; }

	public int Offloaded { get; set; }

	public string? Remarks { get; set; }

	public int CreatedByUserId { get; set; }

	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

	public DateTime? SubmittedAt { get; set; }

	public DateTime? DraftExpiresAt { get; set; }

	public ICollection<DepartureDelay> Delays { get; set; } = new List<DepartureDelay>();

	public ICollection<StaffProductivity> StaffProductivity { get; set; } = new List<StaffProductivity>();
}
