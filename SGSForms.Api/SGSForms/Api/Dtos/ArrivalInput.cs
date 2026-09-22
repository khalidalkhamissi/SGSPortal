using System.Collections.Generic;

namespace SGSForms.Api.Dtos;

public class ArrivalInput
{
	public bool Submit { get; set; }

	public int? StationId { get; set; }

	public string FlightNo { get; set; } = "";

	public string Route { get; set; } = "";

	public string? AircraftReg { get; set; }

	public string? FlightDate { get; set; }

	public string? Sta { get; set; }

	public string? Ata { get; set; }

	public string? GateNo { get; set; }

	public string? GateOpen { get; set; }

	public string? GateClose { get; set; }

	public string? FirstPaxTime { get; set; }

	public string? LastPaxTime { get; set; }

	public int TtlPaxArr { get; set; }

	public int TtlWchr { get; set; }

	public int PaxVip { get; set; }

	public int BagTotal { get; set; }

	public int ActualPax { get; set; }

	public int NoShow { get; set; }

	public int Offloaded { get; set; }

	public string? Remarks { get; set; }

	public List<ServiceDto> Services { get; set; } = new List<ServiceDto>();

	public List<DelayDto> Delays { get; set; } = new List<DelayDto>();
}
