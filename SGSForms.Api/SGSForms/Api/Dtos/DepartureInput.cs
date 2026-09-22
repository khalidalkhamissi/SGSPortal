using System.Collections.Generic;

namespace SGSForms.Api.Dtos;

public class DepartureInput
{
	public bool Submit { get; set; }

	public int? StationId { get; set; }

	public string FlightNo { get; set; } = "";

	public string Route { get; set; } = "";

	public string? AircraftReg { get; set; }

	public string? FlightDate { get; set; }

	public string? Std { get; set; }

	public string? Atd { get; set; }

	public string? CounterNo { get; set; }

	public string? CountersStartedAt { get; set; }

	public int PaxExec { get; set; }

	public int PaxF { get; set; }

	public int PaxJ { get; set; }

	public int PaxW { get; set; }

	public int PaxY { get; set; }

	public int PaxInf { get; set; }

	public int PaxHajj { get; set; }

	public int PaxVip { get; set; }

	public int BagNormal { get; set; }

	public int BagWchr { get; set; }

	public int BagCbbg { get; set; }

	public int BagStcr { get; set; }

	public int BagAvih { get; set; }

	public int BagVip { get; set; }

	public int BagZamzam { get; set; }

	public int BagHajj { get; set; }

	public int ExcessTickets { get; set; }

	public decimal ExcessSales { get; set; }

	public string? BoardingGate { get; set; }

	public string? BoardingMode { get; set; }

	public string? BoardingStarted { get; set; }

	public string? BoardingCompleted { get; set; }

	public string? GateOpened { get; set; }

	public string? GateClosed { get; set; }

	public int TotalBuses { get; set; }

	public string? SpecialHandling { get; set; }

	public int ActualPax { get; set; }

	public int NoShow { get; set; }

	public int Offloaded { get; set; }

	public string? Remarks { get; set; }

	public List<DelayDto> Delays { get; set; } = new List<DelayDto>();

	public List<StaffDto> Staff { get; set; } = new List<StaffDto>();
}
