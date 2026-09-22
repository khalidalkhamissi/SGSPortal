namespace SGSForms.Api.Entities;

public class StaffProductivity
{
	public int Id { get; set; }

	public int DepartureReportId { get; set; }

	public DepartureReport DepartureReport { get; set; } = null!;

	public string StaffName { get; set; } = "";

	public int PassengersServed { get; set; }

	public string? Comments { get; set; }
}
