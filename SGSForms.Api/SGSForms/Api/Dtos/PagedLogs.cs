using System.Collections.Generic;

namespace SGSForms.Api.Dtos;

public class PagedLogs
{
	public List<LogDto> Items { get; set; } = new List<LogDto>();

	public int Total { get; set; }

	public int Page { get; set; }

	public int PageSize { get; set; }
}
