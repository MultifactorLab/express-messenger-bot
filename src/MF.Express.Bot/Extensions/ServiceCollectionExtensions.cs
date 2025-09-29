using System.Reflection;
using MF.Express.Bot.Api.Endpoints;
using MF.Express.Bot.Application.Commands;
using MF.Express.Bot.Application.Interfaces;
using MF.Express.Bot.Application.Queries;
using MF.Express.Bot.Infrastructure.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MF.Express.Bot.Api.Extensions;

/// <summary>
/// Расширения для IServiceCollection в API слое
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Добавляет сервисы Application слоя - команды и запросы
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var applicationAssembly = typeof(ICommand<,>).Assembly;
        var apiAssembly = Assembly.GetExecutingAssembly(); // Текущая сборка (API)
        
        services.AddCommands(applicationAssembly);
        services.AddCommands(apiAssembly); // Для команд из API слоя
        services.AddQueries(applicationAssembly);
        
        return services;
    }

    /// <summary>
    /// Добавляет health checks для Express Bot
    /// </summary>
    public static IServiceCollection AddMfHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<ExpressBotHealthCheck>("express_bot");

        return services;
    }
    
    /// <summary>
    /// Автоматическая регистрация всех обработчиков команд из указанной сборки
    /// </summary>
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

    /// <summary>
    /// Автоматическая регистрация всех обработчиков запросов из указанной сборки
    /// </summary>
    private static IServiceCollection AddQueries(this IServiceCollection services, Assembly assembly)
    {
        var queryHandlerType = typeof(IQuery<,>);
        var queryHandlers = assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false })
            .Where(type => type.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == queryHandlerType))
            .ToList();

        foreach (var handlerType in queryHandlers)
        {
            var interfaceType = handlerType.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == queryHandlerType);

            services.AddScoped(interfaceType, handlerType);
        }

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
