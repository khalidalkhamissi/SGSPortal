using System;

namespace SGSForms.Api.Services;

public class AppException : Exception
{
	public int StatusCode { get; }

	public AppException(string message, int statusCode = 400)
		: base(message)
	{
		StatusCode = statusCode;
	}

	public static AppException NotFound(string msg = "غير موجود")
	{
		return new AppException(msg, 404);
	}

	public static AppException Forbidden(string msg = "غير مصر\u0651ح بالوصول")
	{
		return new AppException(msg, 403);
	}
}
