using System;
using System.Collections.Generic;
using System.Linq;
using SGSForms.Api.Enums;

namespace SGSForms.Api.Auth;

public static class Permissions
{
	public const string ReportsView = "reports.view";

	public const string ReportsCreate = "reports.create";

	public const string ReportsDelete = "reports.delete";

	public const string ReportsApprove = "reports.approve";

	public const string ReportsPending = "reports.pending";

	public const string ReportsDrafts = "reports.drafts";

	public const string ReportsReturned = "reports.returned";

	public const string ReportsApproved = "reports.approved";

	public const string CoordinationView = "coordination.view";

	public const string CoordinationCreate = "coordination.create";

	public const string CoordinationDelete = "coordination.delete";

	public const string ExportPdf = "export.pdf";

	public const string ExportExcel = "export.excel";

	public const string DashboardView = "dashboard.view";

	public const string LogsView = "logs.view";

	public const string AdminUsers = "admin.users";

	public const string AdminRoles = "admin.roles";

	public const string AdminStations = "admin.stations";

	public const string StationsViewAll = "stations.viewAll";

	public const string PolicyPrefix = "perm:";

	public static readonly IReadOnlyList<PermissionDef> Catalog = new List<PermissionDef>
	{
		new PermissionDef("reports.view", "reports", "عرض التقارير", "View reports"),
		new PermissionDef("reports.create", "reports", "إنشاء وتعديل التقارير", "Create & edit reports"),
		new PermissionDef("reports.delete", "reports", "حذف التقارير", "Delete reports"),
		new PermissionDef("reports.approve", "reports", "اعتماد وإرجاع التقارير", "Approve & return reports"),
		new PermissionDef("reports.pending", "lists", "صفحة بانتظار الموافقة", "Pending approval page"),
		new PermissionDef("reports.drafts", "lists", "صفحة المسودات", "Drafts page"),
		new PermissionDef("reports.returned", "lists", "صفحة التقارير الم\u064fعادة", "Returned reports page"),
		new PermissionDef("reports.approved", "lists", "صفحة التقارير المعتمدة", "Approved reports page"),
		new PermissionDef("coordination.view", "coordination", "عرض نماذج التنسيق", "View coordination sheets"),
		new PermissionDef("coordination.create", "coordination", "إنشاء وتعديل التنسيق", "Create & edit coordination"),
		new PermissionDef("coordination.delete", "coordination", "حذف نماذج التنسيق", "Delete coordination sheets"),
		new PermissionDef("export.pdf", "export", "تصدير PDF", "Export PDF"),
		new PermissionDef("export.excel", "export", "تصدير Excel", "Export Excel"),
		new PermissionDef("dashboard.view", "monitoring", "لوحة التحكم", "Dashboard"),
		new PermissionDef("logs.view", "monitoring", "سجل التحديثات", "Activity logs"),
		new PermissionDef("admin.users", "admin", "إدارة المستخدمين", "Manage users"),
		new PermissionDef("admin.roles", "admin", "إدارة الأدوار", "Manage roles"),
		new PermissionDef("admin.stations", "admin", "إدارة المطارات", "Manage stations"),
		new PermissionDef("stations.viewAll", "scope", "الاط\u0651لاع على كل المحطات", "View all stations")
	};

	private static readonly HashSet<string> Known = Catalog.Select((PermissionDef p) => p.Key).ToHashSet<string>(StringComparer.Ordinal);

	public static readonly IReadOnlyDictionary<string, (string Ar, string En)> Groups = new Dictionary<string, (string, string)>
	{
		["reports"] = ("التقارير", "Reports"),
		["lists"] = ("القوائم", "Lists"),
		["coordination"] = ("التنسيق", "Coordination"),
		["export"] = ("التصدير", "Export"),
		["monitoring"] = ("المتابعة", "Monitoring"),
		["admin"] = ("الإدارة", "Administration"),
		["scope"] = ("النطاق", "Scope")
	};

	/// <summary>Any one of these lets a user open a single arrival/departure report.</summary>
	public static readonly string[] ReportReaders = new string[8] { ReportsView, ReportsCreate, ReportsApprove, ReportsPending, ReportsDrafts, ReportsReturned, ReportsApproved, DashboardView };

	/// <summary>Permission required to list reports in a given status (the page that shows that list).</summary>
	public static string ListPermissionFor(ReportStatus status) => status switch
	{
		ReportStatus.Draft => ReportsDrafts,
		ReportStatus.Approved => ReportsApproved,
		ReportStatus.Returned => ReportsReturned,
		_ => ReportsPending,
	};

	public static bool IsKnown(string permission)
	{
		return Known.Contains(permission);
	}
}
