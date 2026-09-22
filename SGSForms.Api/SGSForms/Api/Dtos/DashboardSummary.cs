using System.Collections.Generic;

namespace SGSForms.Api.Dtos;

public class DashboardSummary
{
	public int Total { get; set; }

	public int Approved { get; set; }

	public int Pending { get; set; }

	public int Drafts { get; set; }

	public double AvgSatisfaction { get; set; }

	public int[] ArrDep { get; set; } = new int[2];

	public List<TrendPoint> Trend { get; set; } = new List<TrendPoint>();

	public List<NameCount> ByStation { get; set; } = new List<NameCount>();

	public List<NameCount> ByAirline { get; set; } = new List<NameCount>();

	public int[] SatDist { get; set; } = new int[5];

	public List<LatestReportDto> Latest { get; set; } = new List<LatestReportDto>();
}
