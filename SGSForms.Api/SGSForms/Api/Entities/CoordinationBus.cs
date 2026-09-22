using System;

namespace SGSForms.Api.Entities;

public class CoordinationBus
{
	public int Id { get; set; }

	public int CoordinationSheetId { get; set; }

	public CoordinationSheet CoordinationSheet { get; set; } = null!;

	public string Phase { get; set; } = "arrival";

	public string? BusNo { get; set; }

	public TimeOnly? Time { get; set; }
}
