using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace FileStorage.Extensions;

public static class HttpContextExtensions
{
	public static string? GetUserId(this HttpContext httpContext, string userIdClaimType = "sub")
	{
		return httpContext.User.FindFirstValue(userIdClaimType);
	}

	public static bool IsAuthenticated(this HttpContext httpContext)
	{
		return httpContext.User.Identity?.IsAuthenticated ?? false;
	}
}
