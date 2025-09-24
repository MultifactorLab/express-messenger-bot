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
/// Health check для Bot API v4
/// Проверяет доступность BotX API и статус бота
/// </summary>
public class ExpressBotHealthCheck : IHealthCheck
{
    private readonly IBotXApiService _botXApiService;
    private readonly ILogger<ExpressBotHealthCheck> _logger;

    public ExpressBotHealthCheck(
        IBotXApiService botXApiService,
        ILogger<ExpressBotHealthCheck> logger)
    {
        _botXApiService = botXApiService;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Простая проверка - пытаемся отправить тестовое сообщение самому себе
            // В реальности можно добавить специальный health check endpoint в BotX API
            var testResult = await _botXApiService.SendTextMessageAsync("health-check", "ping", cancellationToken);
            
            if (testResult)
            {
                return HealthCheckResult.Healthy("BotX API connection is healthy");
            }
            
            return HealthCheckResult.Degraded("BotX API connection has issues");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed for BotX API");
            return HealthCheckResult.Unhealthy("BotX API is not accessible", ex);
        }
    }
}
