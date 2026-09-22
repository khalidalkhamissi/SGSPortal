using System.Collections.Generic;
using System.Linq;

namespace SGSForms.Api.Services;

/// <summary>The activities of the coordination time chart, in sheet order (same keys as coordination_form.html).</summary>
public static class CoordinationActivities
{
	public static readonly IReadOnlyList<(string Key, string Label)> All = new (string, string)[24]
	{
		("blocks_in", "Blocks In"),
		("position_plb", "Position PLB/Step"),
		("open_door", "Door Open"),
		("pax_deplane", "Passenger Deplane"),
		("customs_clearance", "Customs Clearance"),
		("cabin_cleaning", "Cabin Cleaning"),
		("galley_services", "Galley Services"),
		("cabin_security_check", "Cabin Security Check"),
		("boarding_clearance", "Boarding Clearance"),
		("pax_enplane", "Passengers Enplane"),
		("top_finalization", "TOP Finalization"),
		("fwd_unloading", "FWD Unloading"),
		("fwd_loading", "FWD Loading"),
		("aft_unloading", "AFT Unloading"),
		("aft_loading", "AFT Loading"),
		("bulk_unloading", "Bulk Unloading"),
		("bulk_loading", "Bulk Loading"),
		("gpu_support", "GPU Support"),
		("acu_support", "ACU Support"),
		("asu_support", "ASU Support"),
		("refueling", "Refueling"),
		("close_door", "Door Close"),
		("remove_plb", "Remove PLB/Step"),
		("pushback", "Pushback/Block-out")
	};

	private static readonly HashSet<string> Keys = All.Select(a => a.Key).ToHashSet();

	public static bool IsKnown(string key) => Keys.Contains(key);
}
