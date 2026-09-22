namespace SGSForms.Api.Entities;

public class ArrivalService
{
	public int Id { get; set; }

	public int ArrivalReportId { get; set; }

	public ArrivalReport ArrivalReport { get; set; } = null!;

	public string Name { get; set; } = "";

	public int Count { get; set; }
}
