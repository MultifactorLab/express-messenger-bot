using MF.Express.Bot.Application.Interfaces;
using MF.Express.Bot.Infrastructure.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MF.Express.Bot.Api.Extensions;

/// <summary>
/// Расширения для IServiceCollection в API слое
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Добавляет health checks для Express Bot
    /// </summary>
    public static IServiceCollection AddMfHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<ExpressBotHealthCheck>("express_bot");

        return services;
    }
}

/// <summary>
/// Health check для Express Bot API
/// </summary>
public class ExpressBotHealthCheck : IHealthCheck
{
    private readonly IExpressBotService _botService;
    private readonly ILogger<ExpressBotHealthCheck> _logger;

    public ExpressBotHealthCheck(
        IExpressBotService botService,
        ILogger<ExpressBotHealthCheck> logger)
    {
        _botService = botService;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var botInfo = await _botService.GetBotInfoAsync(cancellationToken);
            
            if (botInfo.IsActive)
            {
                return HealthCheckResult.Healthy($"Bot {botInfo.Name} is active");
            }
            
            return HealthCheckResult.Degraded($"Bot {botInfo.Name} is not active");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed for Express Bot");
            return HealthCheckResult.Unhealthy("Express Bot API is not accessible", ex);
        }
    }
}
