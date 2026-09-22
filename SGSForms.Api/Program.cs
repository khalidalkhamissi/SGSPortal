using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using PdfSharp.Fonts;
using SGSForms.Api.Auth;
using SGSForms.Api.Data;
using SGSForms.Api.Middleware;
using SGSForms.Api.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
GlobalFontSettings.UseWindowsFontsUnderWindows = true;

// Largest request body accepted (a report with a signature image is well under this).
builder.WebHost.ConfigureKestrel(k => k.Limits.MaxRequestBodySize = 5 * 1024 * 1024);

string conn = builder.Configuration.GetConnectionString("Default")
	?? throw new InvalidOperationException("ConnectionStrings:Default is not configured.");
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseMySql(conn, new MySqlServerVersion(new Version(8, 0, 36))));

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
JwtSettings jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
SymmetricSecurityKey signingKey = JwtTokenService.SigningKey(jwt.Key);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
{
	o.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		ValidIssuer = jwt.Issuer,
		ValidAudience = jwt.Audience,
		IssuerSigningKey = signingKey,
		ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
		ClockSkew = TimeSpan.FromMinutes(1)
	};
	o.Events = new JwtBearerEvents
	{
		// Re-check the account on every request: a deactivated user, a disabled role, a password change or
		// a permission change takes effect immediately instead of when the token expires.
		OnTokenValidated = async ctx =>
		{
			ClaimsPrincipal? principal = ctx.Principal;
			if (!int.TryParse(principal?.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
			{
				ctx.Fail("Invalid token.");
				return;
			}
			AppDbContext db = ctx.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
			var user = await db.Users.AsNoTracking()
				.Where(u => u.Id == userId && u.IsActive && u.Role.IsActive)
				.Select(u => new { u.Id, u.Name, u.PasswordHash, u.StationId, u.RoleId, RoleKey = u.Role.Key })
				.FirstOrDefaultAsync();
			if (user == null || principal!.FindFirstValue(JwtTokenService.StampClaim) != JwtTokenService.SecurityStamp(user.PasswordHash))
			{
				ctx.Fail("Account is no longer valid.");
				return;
			}
			List<string> perms = await db.RolePermissions.AsNoTracking()
				.Where(rp => rp.RoleId == user.RoleId)
				.Select(rp => rp.Permission)
				.ToListAsync();
			ClaimsIdentity identity = new ClaimsIdentity(
				JwtTokenService.ClaimsFor(user.Id, user.Name, user.RoleKey, user.StationId, perms),
				JwtBearerDefaults.AuthenticationScheme, JwtTokenService.NameClaim, ClaimTypes.Role);
			ctx.Principal = new ClaimsPrincipal(identity);
		}
	};
});
builder.Services.AddAuthorization();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();

// Brute-force protection: at most 10 login attempts per minute from one IP address.
builder.Services.AddRateLimiter(o =>
{
	o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
	o.AddPolicy("login", ctx => RateLimitPartition.GetFixedWindowLimiter(
		ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
		_ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
	o.OnRejected = async (ctx, token) =>
	{
		ctx.HttpContext.Response.ContentType = "application/json";
		await ctx.HttpContext.Response.WriteAsync("{\"detail\":\"محاولات دخول كثيرة — حاول بعد دقيقة\"}", token);
	};
});

// Behind a reverse proxy the client address comes from X-Forwarded-For — trusted only from the proxies listed in
// ReverseProxy:KnownProxies, otherwise anyone could fake an address and bypass the per-IP login limit.
string[] knownProxies = builder.Configuration.GetSection("ReverseProxy:KnownProxies").Get<string[]>() ?? Array.Empty<string>();
if (knownProxies.Length > 0)
{
	builder.Services.Configure<ForwardedHeadersOptions>(o =>
	{
		o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
		o.KnownNetworks.Clear();
		o.KnownProxies.Clear();
		foreach (string ip in knownProxies)
		{
			o.KnownProxies.Add(IPAddress.Parse(ip));
		}
	});
}

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CurrentUser>();
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IArrivalService, ArrivalService>();
builder.Services.AddScoped<IDepartureService, DepartureService>();
builder.Services.AddScoped<ICoordinationService, CoordinationService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IApprovalService, ApprovalService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddControllers().AddJsonOptions(o =>
{
	o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
	o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

WebApplication app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
	AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
	try
	{
		await db.Database.MigrateAsync();
		await DbSeeder.SeedAsync(db, app.Configuration.GetValue<bool>("Seed:DemoUsers"), app.Logger);
	}
	catch (Exception exception)
	{
		app.Logger.LogError(exception, "فشل تهيئة قاعدة البيانات — تأكد أن MySQL يعمل وأن سلسلة الاتصال صحيحة.");
	}
}

if (knownProxies.Length > 0)
{
	app.UseForwardedHeaders();
}
app.UseMiddleware<ErrorHandlingMiddleware>();

// Security headers on every response (pages and API).
app.Use(async (ctx, next) =>
{
	IHeaderDictionary h = ctx.Response.Headers;
	h[HeaderNames.XContentTypeOptions] = "nosniff";
	h[HeaderNames.XFrameOptions] = "DENY";
	h["Referrer-Policy"] = "no-referrer";
	h["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
	h[HeaderNames.ContentSecurityPolicy] =
		"default-src 'self'; script-src 'self' 'unsafe-inline'; " +
		"style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; font-src 'self' https://fonts.gstatic.com; " +
		"img-src 'self' data: blob:; connect-src 'self'; object-src 'none'; base-uri 'self'; " +
		"frame-ancestors 'none'; form-action 'self'";
	await next(ctx);
});

// The UI folder: an explicit Frontend:Path, else "Forms Source" next to the project (dotnet run) or the exe (published).
string? frontend = new[]
	{
		app.Configuration["Frontend:Path"],
		Path.Combine(builder.Environment.ContentRootPath, "Forms Source"),
		Path.Combine(AppContext.BaseDirectory, "Forms Source")
	}
	.Where(c => !string.IsNullOrWhiteSpace(c))
	.Select(c => Path.GetFullPath(c!))
	.FirstOrDefault(Directory.Exists);
if (frontend != null)
{
	PhysicalFileProvider fileProvider = new PhysicalFileProvider(frontend);
	app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = fileProvider });
	app.UseStaticFiles(new StaticFileOptions
	{
		FileProvider = fileProvider,
		OnPrepareResponse = ctx => ctx.Context.Response.Headers[HeaderNames.CacheControl] = "no-cache, must-revalidate"
	});
	// A page that static files did not find: show the friendly error page instead of a blank 404.
	app.Use(async (ctx, next) =>
	{
		if (HttpMethods.IsGet(ctx.Request.Method) && ctx.Request.Path.Value?.EndsWith(".html", StringComparison.OrdinalIgnoreCase) == true)
		{
			ctx.Response.Redirect("/error.html?code=404");
			return;
		}
		await next(ctx);
	});
	app.Logger.LogInformation("Serving frontend from {Frontend}", frontend);
}
else
{
	app.Logger.LogWarning("Frontend folder not found; API will run without serving the UI.");
}

app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

app.MapControllers();

app.Run();
