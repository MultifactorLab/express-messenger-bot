using System.Reflection;
using MF.Express.Bot.Application.Commands;
using MF.Express.Bot.Infrastructure.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace MF.Express.Bot.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var applicationAssembly = typeof(ICommand<,>).Assembly;
        var apiAssembly = Assembly.GetExecutingAssembly();
        
        services.AddCommands(applicationAssembly);
        services.AddCommands(apiAssembly);
        
        return services;
    }

    /// <summary>
    /// Добавляет health checks для Express Bot
    /// Liveness - проверяет базовое состояние приложения
    /// Readiness - проверяет готовность к обработке запросов (внешние зависимости)
    /// </summary>
    public static IServiceCollection AddHealthChecks(this IServiceCollection services)
    {
        HealthCheckServiceCollectionExtensions.AddHealthChecks(services)
            .AddCheck("self", () => HealthCheckResult.Healthy("Application is running"))
            .AddCheck<BotXApiHealthCheck>("botx_api", tags: new[] { "ready" })
            .AddCheck<MultifactorApiHealthCheck>("multifactor_api", tags: new[] { "ready" });

        return services;
    }
    private static IServiceCollection AddCommands(this IServiceCollection services, Assembly assembly)
    {
        var commandHandlerType = typeof(ICommand<,>);
        var commandHandlers = assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false })
            .Where(type => type.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == commandHandlerType))
            .ToList();

        foreach (var handlerType in commandHandlers)
        {
            var interfaceType = handlerType.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == commandHandlerType);

            services.AddScoped(interfaceType, handlerType);
        }

        return services;
    }
}

public class BotXApiHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptions<ExpressBotConfiguration> _config;
    private readonly ILogger<BotXApiHealthCheck> _logger;

    public BotXApiHealthCheck(
        IHttpClientFactory httpClientFactory,
        IOptions<ExpressBotConfiguration> config,
        ILogger<BotXApiHealthCheck> logger)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("BotX");
            
            // Используем timeout 5 секунд для health check
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(5));

            var response = await client.SendAsync(
                new HttpRequestMessage(HttpMethod.Head, "/"), 
                cts.Token);
            
            return (int)response.StatusCode < 500 
                ? HealthCheckResult.Healthy($"BotX API is reachable (Status: {response.StatusCode})") 
                : HealthCheckResult.Degraded($"BotX API returned server error: {response.StatusCode}");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Unhealthy("Health check was cancelled");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("BotX API health check timeout");
            return HealthCheckResult.Degraded("BotX API timeout");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "BotX API health check failed - connection error");
            return HealthCheckResult.Unhealthy("BotX API is not accessible", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BotX API health check failed unexpectedly");
            return HealthCheckResult.Unhealthy("BotX API health check failed", ex);
        }
    }
}

public class MultifactorApiHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptions<MultifactorApiConfiguration> _config;
    private readonly ILogger<MultifactorApiHealthCheck> _logger;

    public MultifactorApiHealthCheck(
        IHttpClientFactory httpClientFactory,
        IOptions<MultifactorApiConfiguration> config,
        ILogger<MultifactorApiHealthCheck> logger)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("MultifactorApi");
            
            // Используем timeout 5 секунд для health check
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(5));

            var response = await client.SendAsync(
                new HttpRequestMessage(HttpMethod.Head, "/"), 
                cts.Token);

            return (int)response.StatusCode < 500 
                ? HealthCheckResult.Healthy($"Multifactor API is reachable (Status: {response.StatusCode})") 
                : HealthCheckResult.Degraded($"Multifactor API returned server error: {response.StatusCode}");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Unhealthy("Health check was cancelled");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Multifactor API health check timeout");
            return HealthCheckResult.Degraded("Multifactor API timeout");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Multifactor API health check failed - connection error");
            return HealthCheckResult.Unhealthy("Multifactor API is not accessible", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Multifactor API health check failed unexpectedly");
            return HealthCheckResult.Unhealthy("Multifactor API health check failed", ex);
        }
    }
}
