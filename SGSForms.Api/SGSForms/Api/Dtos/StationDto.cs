namespace SGSForms.Api.Dtos;

public class StationDto
{
	public int Id { get; set; }

	public string Code { get; set; } = "";

	public string NameAr { get; set; } = "";

	public string NameEn { get; set; } = "";

	public bool IsActive { get; set; } = true;
}
