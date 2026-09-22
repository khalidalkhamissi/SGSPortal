using System.Collections.Generic;

namespace SGSForms.Api.Dtos;

public class PagedReports
{
	public List<ReportSummary> Items { get; set; } = new List<ReportSummary>();

	public int Total { get; set; }

	public int Page { get; set; }

	public int PageSize { get; set; }

	public List<string> Airlines { get; set; } = new List<string>();
}
