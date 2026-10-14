using System.Net;
using Assessment.Application.Interfaces;

namespace Assessment.Api.Middleware;

/// <summary>
/// TASK 11: Token Revocation Middleware (5 Points)
/// Checks if the incoming Bearer token has been revoked / blacklisted.
/// Candidates should wire this middleware into the HTTP pipeline in Program.cs
/// and ensure TokenRevocationService checks against the database or cache.
/// </summary>
public class TokenRevocationMiddleware
{
    private readonly RequestDelegate _next;

    public TokenRevocationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITokenRevocationService revocationService)
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var token = authHeader["Bearer ".Length..].Trim();

            if (await revocationService.IsTokenRevokedAsync(token, context.RequestAborted))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"error\": \"Token has been revoked. Please log in again.\"}");
                return;
            }
        }

        await _next(context);
    }
}
