using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SGSForms.Api.Auth;
using SGSForms.Api.Data;

namespace SGSForms.Api.Services;

/// <summary>Rules shared by arrival, departure and coordination forms.</summary>
public static class ReportRules
{
	public const int DraftLifetimeHours = 24;

	/// <summary>Station the form is filed under: the requested one (checked) or the user's own, which must exist and be active.</summary>
	public static async Task<int> ResolveStationAsync(AppDbContext db, CurrentUser me, int? requested)
	{
		int stationId = requested ?? me.StationId ?? throw new AppException("لا توجد محطة محددة للمستخدم");
		if (!me.CanAccessStation(stationId))
		{
			throw AppException.Forbidden();
		}
		if (!await db.Stations.AnyAsync(s => s.Id == stationId && s.IsActive))
		{
			throw new AppException("المحطة غير موجودة أو معطّلة");
		}
		return stationId;
	}

	public static string SaveAction(bool submit, bool isNew) => submit ? "submit" : (isNew ? "create" : "update");
}
