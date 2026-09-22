using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SGSForms.Api.Auth;
using SGSForms.Api.Data;
using SGSForms.Api.Dtos;
using SGSForms.Api.Enums;

namespace SGSForms.Api.Services;

public class ReportService : IReportService
{
	private sealed record Row(string Kind, int Id, string FlightNo, string Route, DateOnly FlightDate, string StationCode, DateTime? DraftExpiresAt);

	private readonly AppDbContext _db;

	private readonly CurrentUser _me;

	public ReportService(AppDbContext db, CurrentUser me)
	{
		_db = db;
		_me = me;
	}

	public async Task<PagedReports> ListAsync(string? status, string? from, string? to, int? stationId, string? airline, string? q, int page, int pageSize)
	{
		ReportStatus st = Parse.Status(status, ReportStatus.Submitted);
		if (!_me.Has(Permissions.ListPermissionFor(st)))
		{
			throw AppException.Forbidden();
		}
		(DateOnly? fromDay, DateOnly? toDay) = Parse.Range(from, to);
		(page, pageSize) = Parse.Paging(page, pageSize);
		int? scope = _me.StationScope(stationId);
		string? af = FlightNo.Filter(airline);
		(string inc, string? exc) = af != null ? FlightNo.SqlPatterns(af) : ("", null);
		string? term = Parse.Search(q);

		var arrivals = _db.ArrivalReports.AsNoTracking()
			.Where(a => a.Status == st && (scope == null || a.StationId == scope)
				&& (fromDay == null || a.FlightDate >= fromDay) && (toDay == null || a.FlightDate <= toDay)
				&& (af == null || (Regex.IsMatch(a.FlightNo, inc) && (exc == null || !Regex.IsMatch(a.FlightNo, exc))))
				&& (term == null || a.FlightNo.Contains(term) || a.Route.Contains(term) || a.Station.Code.Contains(term)));
		var departures = _db.DepartureReports.AsNoTracking()
			.Where(d => d.Status == st && (scope == null || d.StationId == scope)
				&& (fromDay == null || d.FlightDate >= fromDay) && (toDay == null || d.FlightDate <= toDay)
				&& (af == null || (Regex.IsMatch(d.FlightNo, inc) && (exc == null || !Regex.IsMatch(d.FlightNo, exc))))
				&& (term == null || d.FlightNo.Contains(term) || d.Route.Contains(term) || d.Station.Code.Contains(term)));

		int total = await arrivals.CountAsync() + await departures.CountAsync();

		// Both kinds share one ordering, so the first page*pageSize rows of each are enough to build the page.
		int take = page * pageSize;
		List<Row> arrRows = await arrivals
			.OrderByDescending(a => a.FlightDate).ThenByDescending(a => a.Id).Take(take)
			.Select(a => new Row("arrival", a.Id, a.FlightNo, a.Route, a.FlightDate, a.Station.Code, a.DraftExpiresAt))
			.ToListAsync();
		List<Row> depRows = await departures
			.OrderByDescending(d => d.FlightDate).ThenByDescending(d => d.Id).Take(take)
			.Select(d => new Row("departure", d.Id, d.FlightNo, d.Route, d.FlightDate, d.Station.Code, d.DraftExpiresAt))
			.ToListAsync();

		DateTime now = DateTime.UtcNow;
		string statusKey = st.ToString().ToLowerInvariant();
		List<ReportSummary> items = arrRows.Concat(depRows)
			.OrderByDescending(r => r.FlightDate).ThenByDescending(r => r.Id)
			.Skip((page - 1) * pageSize).Take(pageSize)
			.Select(r => new ReportSummary
			{
				Kind = r.Kind,
				Id = r.Id,
				FlightNo = r.FlightNo,
				Route = r.Route,
				FlightDate = r.FlightDate.ToString("yyyy-MM-dd"),
				StationCode = r.StationCode,
				Status = statusKey,
				HoursLeft = st == ReportStatus.Draft && r.DraftExpiresAt.HasValue
					? Math.Max(0, (int)Math.Ceiling((r.DraftExpiresAt.Value - now).TotalHours))
					: null
			})
			.ToList();

		List<string> arrFlights = await _db.ArrivalReports.AsNoTracking()
			.Where(a => a.Status == st && (scope == null || a.StationId == scope))
			.Select(a => a.FlightNo).Distinct().ToListAsync();
		List<string> depFlights = await _db.DepartureReports.AsNoTracking()
			.Where(d => d.Status == st && (scope == null || d.StationId == scope))
			.Select(d => d.FlightNo).Distinct().ToListAsync();
		List<string> airlines = arrFlights.Concat(depFlights)
			.Select(FlightNo.Prefix).Where(p => p.Length > 0)
			.Distinct().OrderBy(p => p).ToList();

		return new PagedReports
		{
			Items = items,
			Total = total,
			Page = page,
			PageSize = pageSize,
			Airlines = airlines
		};
	}
}
