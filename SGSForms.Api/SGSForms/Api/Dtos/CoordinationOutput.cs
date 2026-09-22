using System.Collections.Generic;

namespace SGSForms.Api.Dtos;

public class CoordinationOutput
{
	public int Id { get; set; }

	public string Status { get; set; } = "";

	public string StationCode { get; set; } = "";

	public string? SupervisorName { get; set; }

	public string? ReportTime { get; set; }

	public string? FlightDate { get; set; }

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

	public string? Sta { get; set; }

	public string? Ata { get; set; }

	public string? Std { get; set; }

	public string? Atd { get; set; }

	public string? GainTime { get; set; }

	public string? DlyAmount { get; set; }

	public string? DelayCode { get; set; }

	public string? Remarks { get; set; }

	public List<CoordActivityDto> Activities { get; set; } = new List<CoordActivityDto>();

	public List<CoordBusDto> BusesArrival { get; set; } = new List<CoordBusDto>();

	public List<CoordBusDto> BusesDeparture { get; set; } = new List<CoordBusDto>();
}
