using System;

namespace SGSForms.Api.Entities;

public class AuditLog
{
	public int Id { get; set; }

	public DateTime At { get; set; } = DateTime.UtcNow;

	public int? UserId { get; set; }

	public string UserName { get; set; } = "";

	public string UserRole { get; set; } = "";

	public int? StationId { get; set; }

	public string? StationCode { get; set; }

	public string Action { get; set; } = "";

	public string ReportKind { get; set; } = "";

	public int? ReportId { get; set; }

	public string? FlightNo { get; set; }

	public string? Details { get; set; }
}
