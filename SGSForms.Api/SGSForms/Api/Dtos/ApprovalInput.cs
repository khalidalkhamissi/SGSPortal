namespace SGSForms.Api.Dtos;

public class ApprovalInput
{
	public string Kind { get; set; } = "";

	public int ReportId { get; set; }

	public int Satisfaction { get; set; }

	public string? AirlineRemarks { get; set; }

	public RepDto Rep { get; set; } = new RepDto();

	public string? Signature { get; set; }
}
