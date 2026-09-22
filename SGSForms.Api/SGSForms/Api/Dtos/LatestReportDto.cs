namespace SGSForms.Api.Dtos;

public class LatestReportDto
{
	public string Kind { get; set; } = "";

	public int Id { get; set; }

	public string FlightNo { get; set; } = "";

	public string FlightDate { get; set; } = "";

	public string StationCode { get; set; } = "";

	public int Satisfaction { get; set; }
}
