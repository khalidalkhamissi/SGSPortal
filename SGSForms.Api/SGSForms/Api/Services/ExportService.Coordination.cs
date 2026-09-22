using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using SGSForms.Api.Auth;
using SGSForms.Api.Entities;
using SGSForms.Api.Enums;

namespace SGSForms.Api.Services;

/// <summary>Excel export of coordination sheets (the export dialog on coordination.html).</summary>
public partial class ExportService
{
	private sealed record CoordFilter(List<ReportStatus> Statuses, string? Handling, DateOnly? From, DateOnly? To, int? Scope, string? Airline, string? Term);

	private static readonly string[] HandlingTypes = new string[4] { "turnaround", "transit", "terminating", "originating" };

	/// <summary>
	/// status: comma separated draft / submitted, or "all"; handling: all | turnaround | transit | terminating | originating.
	/// </summary>
	private CoordFilter ReadCoordFilter(string? status, string? handling, string? from, string? to, int? stationId, string? airline, string? q)
	{
		if (!_me.HasAny(Permissions.CoordinationView, Permissions.CoordinationCreate))
		{
			throw AppException.Forbidden();
		}
		List<ReportStatus> statuses = string.Equals(status?.Trim(), "all", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(status)
			? new List<ReportStatus> { ReportStatus.Draft, ReportStatus.Submitted }
			: status.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
				.Select(x => Parse.OptionalStatus(x) is ReportStatus st && (st == ReportStatus.Draft || st == ReportStatus.Submitted)
					? st
					: throw new AppException("حالة غير صالحة: " + x))
				.Distinct().ToList();
		string h = (handling ?? "all").Trim().ToLowerInvariant();
		if (h != "all" && !HandlingTypes.Contains(h))
		{
			throw new AppException("نوع المناولة غير صالح");
		}
		(DateOnly? fromDay, DateOnly? toDay) = Parse.Range(from, to);
		return new CoordFilter(statuses, h == "all" ? null : h, fromDay, toDay,
			_me.StationScope(stationId), FlightNo.Filter(airline), Parse.Search(q));
	}

	private IQueryable<CoordinationSheet> CoordSheets(CoordFilter f)
	{
		return _db.CoordinationSheets.AsNoTracking()
			.Where(c => f.Statuses.Contains(c.Status) && (f.Scope == null || c.StationId == f.Scope)
				&& (f.From == null || c.FlightDate >= f.From) && (f.To == null || c.FlightDate <= f.To)
				&& (f.Handling == null
					|| (f.Handling == "turnaround" && c.Turnaround) || (f.Handling == "transit" && c.Transit)
					|| (f.Handling == "terminating" && c.Terminating) || (f.Handling == "originating" && c.Originating))
				&& (f.Term == null || (c.ArrFlightNo != null && c.ArrFlightNo.Contains(f.Term)) || (c.DepFlightNo != null && c.DepFlightNo.Contains(f.Term))
					|| (c.ArrFrom != null && c.ArrFrom.Contains(f.Term)) || (c.DepTo != null && c.DepTo.Contains(f.Term)) || c.Station.Code.Contains(f.Term)));
	}

	/// <summary>A sheet matches an airline when its arrival or departure flight carries that designator.</summary>
	private static bool CoordAirlineMatches(CoordFilter f, string? arr, string? dep)
	{
		return f.Airline == null || FlightNo.Prefix(arr) == f.Airline || FlightNo.Prefix(dep) == f.Airline;
	}

	public async Task<int> CoordinationExcelCountAsync(string? status, string? handling, string? from, string? to, int? stationId, string? airline, string? q)
	{
		CoordFilter f = ReadCoordFilter(status, handling, from, to, stationId, airline, q);
		var flights = await CoordSheets(f).Select(c => new { c.ArrFlightNo, c.DepFlightNo }).ToListAsync();
		return flights.Count(x => CoordAirlineMatches(f, x.ArrFlightNo, x.DepFlightNo));
	}

	public async Task<(byte[], string)> CoordinationExcelAsync(string? status, string? handling, string? from, string? to, int? stationId, string? airline, string? q)
	{
		CoordFilter f = ReadCoordFilter(status, handling, from, to, stationId, airline, q);
		EnsureExportSize(await CoordSheets(f).CountAsync());
		List<CoordinationSheet> sheets = (await CoordSheets(f)
				.Include(c => c.Station).Include(c => c.Activities).Include(c => c.Buses)
				.OrderByDescending(c => c.FlightDate).ThenByDescending(c => c.Id)
				.ToListAsync())
			.Where(c => CoordAirlineMatches(f, c.ArrFlightNo, c.DepFlightNo))
			.ToList();

		string? stationCode = f.Scope.HasValue ? await _db.Stations.Where(s => s.Id == f.Scope.Value).Select(s => s.Code).FirstOrDefaultAsync() : null;
		string title = string.Join(" + ", f.Statuses.Select(CoordStatusLabel))
			+ ((f.From.HasValue || f.To.HasValue) ? "   ·   " + (f.From?.ToString("yyyy-MM-dd") ?? "…") + " → " + (f.To?.ToString("yyyy-MM-dd") ?? "…") : "")
			+ (f.Handling != null ? "   ·   " + HandlingLabel(f.Handling) : "")
			+ (stationCode != null ? "   ·   Station: " + stationCode : "")
			+ (f.Airline != null ? "   ·   Airline: " + f.Airline : "")
			+ $"   ·   Generated: {DateTime.Now:yyyy-MM-dd HH:mm}";

		using XLWorkbook wb = new XLWorkbook();

		// Sheet 1: one row per coordination sheet.
		(string, int)[] groups = new (string, int)[6]
		{
			("General Information", 6),
			("Arrival", 8),
			("Departure", 8),
			("Times & Delay", 3),
			("Additional", 4),
			("", 1)
		};
		string[] heads = new string[30]
		{
			"Date", "Station", "AC Type", "AC Reg", "Handling Type", "Supervisor",
			"Flight No", "From", "Pax F", "Pax J", "Pax Y", "Total Pax", "STA", "ATA",
			"Flight No", "To", "Pax F", "Pax J", "Pax Y", "Total Pax", "STD", "ATD",
			"GAIN Time", "DLY Amount", "Delay Code",
			"Arrival Buses", "Departure Buses", "Report Time", "Remarks",
			"Status"
		};
		List<object[]> rows = sheets.Select(r => new object[30]
		{
			r.FlightDate.ToString("yyyy-MM-dd"),
			r.Station.Code,
			V(r.AcType),
			V(r.AcReg),
			HandlingOf(r),
			V(r.SupervisorName),
			V(r.ArrFlightNo),
			V(r.ArrFrom),
			r.ArrPaxF,
			r.ArrPaxJ,
			r.ArrPaxY,
			r.ArrPaxF + r.ArrPaxJ + r.ArrPaxY,
			T(r.Sta),
			T(r.Ata),
			V(r.DepFlightNo),
			V(r.DepTo),
			r.DepPaxF,
			r.DepPaxJ,
			r.DepPaxY,
			r.DepPaxF + r.DepPaxJ + r.DepPaxY,
			T(r.Std),
			T(r.Atd),
			V(r.GainTime),
			r.DlyAmount > 0 ? $"{r.DlyAmount / 60:00}:{r.DlyAmount % 60:00}" : "-",
			V(r.DelayCode),
			BusesOf(r, "arrival"),
			BusesOf(r, "departure"),
			DT(r.ReportTime),
			V(r.Remarks),
			CoordStatusLabel(r.Status)
		}).ToList();
		BuildSheet(wb.Worksheets.Add("Coordination"), "SGS Coordination Sheets — " + title, groups, heads, rows,
			new int[8] { 9, 10, 11, 12, 17, 18, 19, 20 }, Array.Empty<int>());

		// Sheet 2: the activities time chart, one row per recorded activity.
		string[] actHeads = new string[9] { "Date", "Station", "Arr Flight", "Dep Flight", "Activity", "Actual Start", "Actual Finish", "Duration", "Remarks" };
		List<object[]> actRows = sheets.SelectMany(r => CoordinationActivities.All
			.Select(def => (def.Label, Act: r.Activities.FirstOrDefault(a => a.ActivityKey == def.Key)))
			.Where(x => x.Act != null)
			.Select(x => new object[9]
			{
				r.FlightDate.ToString("yyyy-MM-dd"),
				r.Station.Code,
				V(r.ArrFlightNo),
				V(r.DepFlightNo),
				x.Label,
				T(x.Act!.ActualStart),
				T(x.Act.ActualFinish),
				V(Dur(x.Act.ActualStart, x.Act.ActualFinish)),
				V(x.Act.Remarks)
			})).ToList();
		BuildSheet(wb.Worksheets.Add("Activities"), "SGS Coordination — Activities Time Chart — " + title,
			new (string, int)[2] { ("Flight", 4), ("Activity", 5) }, actHeads, actRows, Array.Empty<int>(), Array.Empty<int>());

		using MemoryStream ms = new MemoryStream();
		wb.SaveAs(ms);
		string range = f.From.HasValue || f.To.HasValue ? $"_{f.From:yyyyMMdd}-{f.To:yyyyMMdd}" : "";
		return (ms.ToArray(), $"Coordination{(f.Airline != null ? "_" + Safe(f.Airline) : "")}{range}_{DateTime.Now:yyyyMMdd}.xlsx");
	}

	private static string CoordStatusLabel(ReportStatus s) => s == ReportStatus.Draft ? "Draft" : "Completed";

	private static string HandlingLabel(string h) => char.ToUpperInvariant(h[0]) + h.Substring(1);

	private static string HandlingOf(CoordinationSheet r)
	{
		return r.Turnaround ? "Turnaround" : r.Transit ? "Transit" : r.Terminating ? "Terminating" : r.Originating ? "Originating" : "-";
	}

	private static string BusesOf(CoordinationSheet r, string phase)
	{
		return J(", ", r.Buses.Where(b => b.Phase == phase).Select(b => V(b.BusNo) + " (" + T(b.Time) + ")"));
	}
}
