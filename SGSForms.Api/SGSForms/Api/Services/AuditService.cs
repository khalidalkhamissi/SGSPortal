using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SGSForms.Api.Auth;
using SGSForms.Api.Data;
using SGSForms.Api.Entities;

namespace SGSForms.Api.Services;

public class AuditService : IAuditService
{
	private readonly AppDbContext _db;

	private readonly CurrentUser _me;

	private readonly ILogger<AuditService> _log;

	public AuditService(AppDbContext db, CurrentUser me, ILogger<AuditService> log)
	{
		_db = db;
		_me = me;
		_log = log;
	}

	/// <summary>Writes an audit entry. Called after the audited change is saved; a logging failure never undoes that change.</summary>
	public async Task LogAsync(string action, string kind, int? reportId = null, string? flightNo = null, string? details = null, int? stationId = null)
	{
		int? sid = stationId ?? _me.StationId;
		AuditLog entry = new AuditLog
		{
			At = DateTime.UtcNow,
			UserId = _me.Id == 0 ? null : _me.Id,
			UserName = _me.Name,
			UserRole = _me.Role,
			StationId = sid,
			Action = action,
			ReportKind = kind,
			ReportId = reportId,
			FlightNo = flightNo,
			Details = details
		};
		try
		{
			if (sid.HasValue)
			{
				entry.StationCode = await _db.Stations.AsNoTracking().Where(x => x.Id == sid.Value).Select(x => x.Code).FirstOrDefaultAsync();
			}
			_db.AuditLogs.Add(entry);
			await _db.SaveChangesAsync();
		}
		catch (Exception ex)
		{
			_db.Entry(entry).State = EntityState.Detached;
			_log.LogError(ex, "Failed to write audit log entry {Action} {Kind} {ReportId}", action, kind, reportId);
		}
	}
}
