using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MF.Express.Bot.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace MF.Express.Bot.Api.Middleware;

public class BotXJwtValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<BotXJwtValidationMiddleware> _logger;
    private readonly ExpressBotConfiguration _config;

    public BotXJwtValidationMiddleware(
        RequestDelegate next, 
        ILogger<BotXJwtValidationMiddleware> logger,
        IOptions<ExpressBotConfiguration> config)
    {
        _next = next;
        _logger = logger;
        _config = config.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!ShouldValidateRequest(context))
        {
            await _next(context);
            return;
        }

        try
        {
            var token = ExtractToken(context);
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Missing Authorization header. Path: {Path:l}", context.Request.Path);
                await WriteUnauthorizedResponse(context, "Missing authorization header");
                return;
            }

            var claimsPrincipal = ValidateJwtToken(token);
            if (claimsPrincipal == null)
            {
                _logger.LogWarning("Invalid JWT token. Path: {Path:l}", context.Request.Path);
                await WriteUnauthorizedResponse(context, "Invalid JWT token");
                return;
            }

            context.User = claimsPrincipal;

            _logger.LogDebug("JWT token validated successfully. Path: {Path:l}", context.Request.Path);
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "JWT validation error. Path: {Path:l}", context.Request.Path);
            await WriteUnauthorizedResponse(context, "JWT validation failed");
        }
    }

    private static bool ShouldValidateRequest(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant();
        
        return path switch
        {
            "/api/command" => true,
            "/api/notification/callback" => true,
            _ => false
        };
    }

    private static string? ExtractToken(HttpContext context)
    {
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return authHeader["Bearer ".Length..].Trim();
    }

    private ClaimsPrincipal? ValidateJwtToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            
            var key = Encoding.UTF8.GetBytes(_config.BotSecretKey);
            
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _config.ExpectedIssuer,
                ValidateAudience = true,
                ValidAudience = _config.BotId,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1)
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            
            if (validatedToken is JwtSecurityToken jwtToken && 
                jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return principal;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "JWT token validation error");
            return null;
        }
    }

    private static async Task WriteUnauthorizedResponse(HttpContext context, string message)
    {
        context.Response.StatusCode = 401;
        context.Response.ContentType = "application/json";
        
        var response = new 
        {
            error = "Unauthorized",
            message = message,
            timestamp = DateTime.UtcNow
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}

public static class BotXJwtValidationMiddlewareExtensions
{
    public static IApplicationBuilder UseBotXJwtValidation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<BotXJwtValidationMiddleware>();
    }
}
