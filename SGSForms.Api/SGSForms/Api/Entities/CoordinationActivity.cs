using System;

namespace SGSForms.Api.Entities;

public class CoordinationActivity
{
	public int Id { get; set; }

	public int CoordinationSheetId { get; set; }

	public CoordinationSheet CoordinationSheet { get; set; } = null!;

	public string ActivityKey { get; set; } = "";

	public TimeOnly? ActualStart { get; set; }

	public TimeOnly? ActualFinish { get; set; }

	public string? Remarks { get; set; }
}
