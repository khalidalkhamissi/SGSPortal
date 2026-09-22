namespace SGSForms.Api.Entities;

public class ArrivalDelay
{
	public int Id { get; set; }

	public int ArrivalReportId { get; set; }

	public ArrivalReport ArrivalReport { get; set; } = null!;

	public string? Code { get; set; }

	public string? Reason { get; set; }

	public int DurationMinutes { get; set; }
}
