using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SGSForms.Api.Auth;
using SGSForms.Api.Data;
using SGSForms.Api.Dtos;
using SGSForms.Api.Entities;
using SGSForms.Api.Enums;

namespace SGSForms.Api.Services;

/// <summary>
/// Dashboard figures for a period: one light row per report is read and counted in memory, and satisfaction
/// comes only from the approvals of approved reports inside the period (never the whole approvals history).
/// </summary>
public class DashboardService : IDashboardService
{
	/// <summary>Longest range the dashboard aggregates in one request (about five years).</summary>
	private const int MaxRangeDays = 1830;

	private const int LatestCount = 8;

	private static readonly HashSet<string> NamedAirlines = new HashSet<string> { "SV", "XY", "F3" };

	private sealed record Range(int? Scope, DateOnly From, DateOnly To, string? Include, string? Exclude);

	private sealed class Item
	{
		public int Id { get; init; }
		public ReportStatus Status { get; init; }
		public DateOnly FlightDate { get; init; }
		public string FlightNo { get; init; } = "";
		public string Code { get; init; } = "";
	}

	private static Dictionary<DateOnly, int> PerDay(List<Item> items) => items.GroupBy(x => x.FlightDate).ToDictionary(g => g.Key, g => g.Count());

	private sealed class Rated
	{
		public ReportKind Kind { get; init; }
		public int ReportId { get; init; }
		public int ApprovalId { get; init; }
		public int Satisfaction { get; init; }
		public DateTime ApprovedAt { get; init; }
	}

	private readonly AppDbContext _db;

	private readonly CurrentUser _me;

	public DashboardService(AppDbContext db, CurrentUser me)
	{
		_db = db;
		_me = me;
	}

	private static string AirlineBucket(string flightNo)
	{
		string prefix = FlightNo.Prefix(flightNo);
		return NamedAirlines.Contains(prefix) ? prefix : "Other";
	}

	private IQueryable<ArrivalReport> Arrivals(Range r)
	{
		IQueryable<ArrivalReport> q = _db.ArrivalReports.AsNoTracking()
			.Where(a => (r.Scope == null || a.StationId == r.Scope) && a.FlightDate >= r.From && a.FlightDate <= r.To);
		return r.Include == null ? q : q.Where(a => Regex.IsMatch(a.FlightNo, r.Include) && (r.Exclude == null || !Regex.IsMatch(a.FlightNo, r.Exclude)));
	}

	private IQueryable<DepartureReport> Departures(Range r)
	{
		IQueryable<DepartureReport> q = _db.DepartureReports.AsNoTracking()
			.Where(d => (r.Scope == null || d.StationId == r.Scope) && d.FlightDate >= r.From && d.FlightDate <= r.To);
		return r.Include == null ? q : q.Where(d => Regex.IsMatch(d.FlightNo, r.Include) && (r.Exclude == null || !Regex.IsMatch(d.FlightNo, r.Exclude)));
	}

	public async Task<DashboardSummary> SummaryAsync(string? from, string? to, int? stationId, string? airline)
	{
		DateOnly toDay = Parse.OptionalDate(to) ?? DateOnly.FromDateTime(DateTime.UtcNow);
		DateOnly fromDay = Parse.OptionalDate(from) ?? toDay.AddDays(-29);
		if (fromDay > toDay)
		{
			(fromDay, toDay) = (toDay, fromDay);
		}
		if (toDay.DayNumber - fromDay.DayNumber > MaxRangeDays)
		{
			fromDay = toDay.AddDays(-MaxRangeDays);
		}
		// The airline filter matches the flight-number designator exactly (the same rule as FlightNo.Prefix).
		string? af = FlightNo.Filter(airline);
		(string? inc, string? exc) = af != null ? FlightNo.SqlPatterns(af) : (null, null);
		Range r = new Range(_me.StationScope(stationId), fromDay, toDay, inc, exc);

		// One light row per report in the range (id, status, date, flight, station) — a single fast pass,
		// which beats many GROUP BY queries once the range spans years. Everything below is counted from it.
		List<Item> arr = await Arrivals(r).Select(x => new Item { Id = x.Id, Status = x.Status, FlightDate = x.FlightDate, FlightNo = x.FlightNo, Code = x.Station.Code }).ToListAsync();
		List<Item> dep = await Departures(r).Select(x => new Item { Id = x.Id, Status = x.Status, FlightDate = x.FlightDate, FlightNo = x.FlightNo, Code = x.Station.Code }).ToListAsync();
		List<Item> all = arr.Concat(dep).ToList();

		DashboardSummary s = new DashboardSummary
		{
			Total = all.Count,
			Approved = all.Count(x => x.Status == ReportStatus.Approved),
			Pending = all.Count(x => x.Status == ReportStatus.Submitted),
			Drafts = all.Count(x => x.Status == ReportStatus.Draft),
			ArrDep = new int[2] { arr.Count, dep.Count },
			Trend = Trend(PerDay(arr), PerDay(dep), fromDay, toDay),
			ByStation = all.GroupBy(x => x.Code)
				.Select(g => new NameCount { Name = g.Key, Count = g.Count() })
				.OrderByDescending(n => n.Count).ThenBy(n => n.Name).Take(8).ToList(),
			ByAirline = all.GroupBy(x => AirlineBucket(x.FlightNo))
				.Select(g => new NameCount { Name = g.Key, Count = g.Count() })
				.OrderByDescending(n => n.Count).ThenBy(n => n.Name).ToList()
		};

		// Satisfaction: the latest "approved" decision of every approved report in the range. The approved
		// report ids are already known; few of them → fetch just their approvals, many → one plain scan.
		HashSet<int> arrApproved = arr.Where(x => x.Status == ReportStatus.Approved).Select(x => x.Id).ToHashSet();
		HashSet<int> depApproved = dep.Where(x => x.Status == ReportStatus.Approved).Select(x => x.Id).ToHashSet();
		List<Rated> rated = (await ApprovalsFor(ReportKind.Arrival, arrApproved)).Concat(await ApprovalsFor(ReportKind.Departure, depApproved))
			.GroupBy(x => (x.Kind, x.ReportId))
			.Select(g => g.OrderByDescending(x => x.ApprovedAt).ThenByDescending(x => x.ApprovalId).First())
			.ToList();
		foreach (Rated x in rated)
		{
			s.SatDist[Math.Clamp(x.Satisfaction, 1, 5) - 1]++;
		}
		s.AvgSatisfaction = rated.Count > 0 ? Math.Round((double)rated.Sum(x => Math.Clamp(x.Satisfaction, 1, 5)) / rated.Count, 1) : 0.0;

		// The eight most recent approvals, with their report details.
		List<Rated> latest = rated.OrderByDescending(x => x.ApprovedAt).Take(LatestCount).ToList();
		Dictionary<(ReportKind, int), Item> details = arr.Select(x => (Key: (ReportKind.Arrival, x.Id), x))
			.Concat(dep.Select(x => (Key: (ReportKind.Departure, x.Id), x)))
			.ToDictionary(p => p.Key, p => p.x);
		s.Latest = latest.Select(x =>
		{
			Item d = details[(x.Kind, x.ReportId)];
			return new LatestReportDto
			{
				Kind = x.Kind == ReportKind.Arrival ? "arrival" : "departure",
				Id = d.Id,
				FlightNo = d.FlightNo,
				FlightDate = d.FlightDate.ToString("yyyy-MM-dd"),
				StationCode = d.Code,
				Satisfaction = x.Satisfaction
			};
		}).ToList();
		return s;
	}

	/// <summary>Largest id list sent as an IN (...) filter; beyond it one scan of the approvals is cheaper.</summary>
	private const int MaxIdFilter = 2000;

	/// <summary>Every "approved" decision (usually one) for the given reports of one kind.</summary>
	private async Task<List<Rated>> ApprovalsFor(ReportKind kind, HashSet<int> reportIds)
	{
		if (reportIds.Count == 0)
		{
			return new List<Rated>();
		}
		IQueryable<Approval> q = _db.Approvals.AsNoTracking().Where(a => a.Decision == ApprovalDecision.Approved && a.ReportKind == kind);
		if (reportIds.Count <= MaxIdFilter)
		{
			List<int> ids = reportIds.ToList();
			q = kind == ReportKind.Arrival
				? q.Where(a => a.ArrivalReportId != null && ids.Contains(a.ArrivalReportId.Value))
				: q.Where(a => a.DepartureReportId != null && ids.Contains(a.DepartureReportId.Value));
		}
		List<Rated> rows = await q.Select(a => new Rated
		{
			Kind = kind,
			ReportId = (kind == ReportKind.Arrival ? a.ArrivalReportId : a.DepartureReportId) ?? 0,
			ApprovalId = a.Id,
			Satisfaction = a.Satisfaction,
			ApprovedAt = a.ApprovedAt
		}).ToListAsync();
		return rows.Where(x => reportIds.Contains(x.ReportId)).ToList();
	}

	/// <summary>Daily buckets up to a month, weekly up to ~4 months, monthly beyond.</summary>
	private static List<TrendPoint> Trend(Dictionary<DateOnly, int> arr, Dictionary<DateOnly, int> dep, DateOnly fromDay, DateOnly toDay)
	{
		List<TrendPoint> trend = new List<TrendPoint>();
		void Add(string label, DateOnly a, DateOnly b) => trend.Add(new TrendPoint
		{
			Label = label,
			Arr = arr.Where(x => x.Key >= a && x.Key <= b).Sum(x => x.Value),
			Dep = dep.Where(x => x.Key >= a && x.Key <= b).Sum(x => x.Value)
		});

		int days = toDay.DayNumber - fromDay.DayNumber + 1;
		if (days <= 31)
		{
			for (DateOnly d = fromDay; d <= toDay; d = d.AddDays(1))
			{
				Add(d.ToString("MM-dd"), d, d);
			}
		}
		else if (days <= 120)
		{
			for (DateOnly d = fromDay; d <= toDay; d = d.AddDays(7))
			{
				DateOnly end = d.AddDays(6);
				Add(d.ToString("MM-dd"), d, end > toDay ? toDay : end);
			}
		}
		else
		{
			for (DateOnly m = new DateOnly(fromDay.Year, fromDay.Month, 1); m <= toDay; m = m.AddMonths(1))
			{
				DateOnly end = m.AddMonths(1).AddDays(-1);
				Add(m.ToString("yyyy-MM"), m < fromDay ? fromDay : m, end > toDay ? toDay : end);
			}
		}
		return trend;
	}
}
