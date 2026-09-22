namespace SGSForms.Api.Dtos;

public class ReportSummary
{
	public string Kind { get; set; } = "";

	public int Id { get; set; }

	public string FlightNo { get; set; } = "";

	public string FlightDate { get; set; } = "";

	public string Route { get; set; } = "";

	public string StationCode { get; set; } = "";

	public string Status { get; set; } = "";

	public int? HoursLeft { get; set; }
}
