using System;
using System.Globalization;
using SGSForms.Api.Enums;

namespace SGSForms.Api.Services;

/// <summary>Parsing and validation of client input. Invalid input is rejected with a 400, never silently replaced.</summary>
public static class Parse
{
	public const int MaxListItems = 100;

	/// <summary>Most rows any list page returns.</summary>
	public const int MaxPageSize = 50;

	public static DateOnly Date(string? s)
	{
		if (string.IsNullOrWhiteSpace(s))
		{
			return DateOnly.FromDateTime(DateTime.UtcNow);
		}
		if (!DateOnly.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly result))
		{
			throw new AppException("تاريخ غير صالح: " + s);
		}
		return result;
	}

	public static DateOnly? OptionalDate(string? s)
	{
		return DateOnly.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly result) ? result : null;
	}

	/// <summary>Reads a from/to filter pair, swapping them when given in reverse order.</summary>
	public static (DateOnly? From, DateOnly? To) Range(string? from, string? to)
	{
		DateOnly? f = OptionalDate(from);
		DateOnly? t = OptionalDate(to);
		return (f.HasValue && t.HasValue && f > t) ? (t, f) : (f, t);
	}

	public static TimeOnly? Time(string? s)
	{
		if (string.IsNullOrWhiteSpace(s))
		{
			return null;
		}
		if (!TimeOnly.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out TimeOnly result))
		{
			throw new AppException("وقت غير صالح: " + s);
		}
		return result;
	}

	/// <summary>Free-text search term: trimmed, at most 50 characters; empty means no search.</summary>
	public static string? Search(string? q)
	{
		if (string.IsNullOrWhiteSpace(q))
		{
			return null;
		}
		q = q.Trim();
		return q.Length > 50 ? q.Substring(0, 50) : q;
	}

	public static ReportKind Kind(string? kind)
	{
		return (kind ?? "").Trim().ToLowerInvariant() switch
		{
			"arrival" => ReportKind.Arrival,
			"departure" => ReportKind.Departure,
			_ => throw new AppException("نوع التقرير غير صالح"),
		};
	}

	public static ReportStatus Status(string? status, ReportStatus fallback)
	{
		return OptionalStatus(status) ?? fallback;
	}

	public static ReportStatus? OptionalStatus(string? status)
	{
		return status?.Trim().ToLowerInvariant() switch
		{
			"draft" => ReportStatus.Draft,
			"submitted" => ReportStatus.Submitted,
			"approved" => ReportStatus.Approved,
			"returned" => ReportStatus.Returned,
			_ => null,
		};
	}

	/// <summary>Trims the value and enforces a maximum length; empty becomes null.</summary>
	public static string? Text(string? s, int maxLength, string field)
	{
		if (string.IsNullOrWhiteSpace(s))
		{
			return null;
		}
		s = s.Trim();
		if (s.Length > maxLength)
		{
			throw new AppException($"{field}: الحد الأقصى {maxLength} حرف");
		}
		return s;
	}

	public static string Required(string? s, int maxLength, string field)
	{
		return Text(s, maxLength, field) ?? throw new AppException(field + " مطلوب");
	}

	public static int Count(int n, string field)
	{
		if (n < 0 || n > 100_000)
		{
			throw new AppException(field + ": قيمة غير صالحة");
		}
		return n;
	}

	public static void MaxItems<T>(System.Collections.Generic.ICollection<T>? items, string field)
	{
		if (items != null && items.Count > MaxListItems)
		{
			throw new AppException($"{field}: الحد الأقصى {MaxListItems} عنصر");
		}
	}

	public static (int Page, int PageSize) Paging(int page, int pageSize)
	{
		return (Math.Clamp(page, 1, 100_000), Math.Clamp(pageSize, 1, MaxPageSize));
	}
}
