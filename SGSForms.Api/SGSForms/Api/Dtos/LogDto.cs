namespace SGSForms.Api.Dtos;

public class LogDto
{
	public int Id { get; set; }

	public string At { get; set; } = "";

	public string UserName { get; set; } = "";

	public string UserRole { get; set; } = "";

	public string? StationCode { get; set; }

	public string Action { get; set; } = "";

	public string ReportKind { get; set; } = "";

	public int? ReportId { get; set; }

	public string? FlightNo { get; set; }

	public string? Details { get; set; }
}
