namespace SGSForms.Api.Entities;

public class DepartureDelay
{
	public int Id { get; set; }

	public int DepartureReportId { get; set; }

	public DepartureReport DepartureReport { get; set; } = null!;

	public string? Code { get; set; }

	public string? Reason { get; set; }

	public int DurationMinutes { get; set; }
}
