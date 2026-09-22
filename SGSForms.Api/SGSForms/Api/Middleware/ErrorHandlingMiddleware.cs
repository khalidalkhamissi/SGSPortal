using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SGSForms.Api.Services;

namespace SGSForms.Api.Middleware;

public class ErrorHandlingMiddleware
{
	private readonly RequestDelegate _next;

	private readonly ILogger<ErrorHandlingMiddleware> _log;

	public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> log)
	{
		_next = next;
		_log = log;
	}

	public async Task Invoke(HttpContext ctx)
	{
		try
		{
			await _next(ctx);
		}
		catch (AppException ex)
		{
			await WriteAsync(ctx, ex.StatusCode, ex.Message);
		}
		catch (Exception ex) when (!ctx.RequestAborted.IsCancellationRequested)
		{
			_log.LogError(ex, "Unhandled error on {Method} {Path}", ctx.Request.Method, ctx.Request.Path);
			await WriteAsync(ctx, StatusCodes.Status500InternalServerError, "خطأ داخلي في الخادم");
		}
	}

	private static async Task WriteAsync(HttpContext ctx, int status, string detail)
	{
		// Once the body has started streaming the status can no longer be changed; let the connection fail.
		if (ctx.Response.HasStarted)
		{
			return;
		}
		ctx.Response.Clear();
		ctx.Response.StatusCode = status;
		ctx.Response.ContentType = "application/json";
		await ctx.Response.WriteAsync(JsonSerializer.Serialize(new { detail }));
	}
}
