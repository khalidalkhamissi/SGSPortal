using System.Text.RegularExpressions;

namespace SGSForms.Api.Services;

/// <summary>
/// Airline designator of a flight number. IATA designators are always two characters and may contain a digit
/// (SV, XY, F3, 6E); three-letter ICAO designators are recognised when a flight number follows them (SVA123).
/// The same rule is used for lists, filters, exports, the dashboard and the pages (nav.js SGS.airlineCode).
/// </summary>
public static partial class FlightNo
{
	[GeneratedRegex("^(?:[A-Z]{3}(?=[ -]?[0-9])|[A-Z]{2}|[A-Z][0-9]|[0-9][A-Z])", RegexOptions.IgnoreCase)]
	private static partial Regex PrefixRegex();

	[GeneratedRegex("^(?:[A-Z]{2,3}|[A-Z][0-9]|[0-9][A-Z])$")]
	private static partial Regex CodeRegex();

	/// <summary>"SV123" → "SV", "F3123" → "F3", "6E201" → "6E", "SVA123" → "SVA", "123" → "".</summary>
	public static string Prefix(string? flightNo) => PrefixRegex().Match(flightNo ?? "").Value.ToUpperInvariant();

	/// <summary>Normalised airline filter from a query string, or null for "all airlines".</summary>
	public static string? Filter(string? airline) => string.IsNullOrWhiteSpace(airline) ? null : airline.Trim().ToUpperInvariant();

	/// <summary>
	/// MySQL REGEXP patterns that select exactly the flight numbers whose <see cref="Prefix"/> equals the filter,
	/// so paging and counting can happen in the database: match <c>Include</c> and not <c>Exclude</c>.
	/// A filter that is not a valid designator matches nothing.
	/// </summary>
	public static (string Include, string? Exclude) SqlPatterns(string af)
	{
		if (!CodeRegex().IsMatch(af))
		{
			return ("^$", null);
		}
		if (af.Length == 3)
		{
			return ($"^{af}[ -]?[0-9]", null);
		}
		// "SV" must not also pick up SVA123, whose designator is SVA.
		return char.IsLetter(af[0]) && char.IsLetter(af[1])
			? ($"^{af}", $"^{af}[A-Z][ -]?[0-9]")
			: ($"^{af}", null);
	}
}
