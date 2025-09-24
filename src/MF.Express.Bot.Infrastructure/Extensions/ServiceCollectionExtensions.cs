using System.Reflection;
using MF.Express.Bot.Application.Commands;
using MF.Express.Bot.Application.Interfaces;
using MF.Express.Bot.Application.Queries;
using MF.Express.Bot.Infrastructure.Configuration;
using MF.Express.Bot.Infrastructure.ExternalServices;
using MF.Express.Bot.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MF.Express.Bot.Infrastructure.Extensions;

/// <summary>
/// Расширения для регистрации сервисов Infrastructure слоя
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Добавляет все сервисы Infrastructure слоя
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddExpressBotService();
        services.AddMultifactorApiService();
        services.AddCommandsAndQueries();
        
        return services;
    }

    /// <summary>
    /// Добавляет Express Bot API сервис
    /// </summary>
    public static IServiceCollection AddExpressBotService(this IServiceCollection services)
    {
        services.AddHttpClient("ExpressBot", (serviceProvider, client) =>
        {
            var config = serviceProvider.GetRequiredService<IOptions<ExpressBotConfiguration>>().Value;
            client.BaseAddress = new Uri(config.ApiBaseUrl);
            client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", config.BotToken);
            client.Timeout = TimeSpan.FromSeconds(config.RequestTimeoutSeconds);
        });

        services.AddScoped<IExpressBotService, ExpressBotService>();

        return services;
    }


    /// <summary>
    /// Автоматическая регистрация всех обработчиков команд и запросов
    /// </summary>
    public static IServiceCollection AddCommandsAndQueries(this IServiceCollection services)
    {
        var applicationAssembly = typeof(SendMessageCommand).Assembly;
        
        services.AddCommands(applicationAssembly);
        services.AddQueries(applicationAssembly);
        
        return services;
    }

    /// <summary>
    /// Автоматическая регистрация всех обработчиков команд из указанной сборки
    /// </summary>
    public static IServiceCollection AddCommands(this IServiceCollection services, Assembly assembly)
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
    public static IServiceCollection AddQueries(this IServiceCollection services, Assembly assembly)
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

    /// <summary>
    /// Добавляет Multifactor API сервис
    /// </summary>
    public static IServiceCollection AddMultifactorApiService(this IServiceCollection services)
    {
        services.AddHttpClient("MultifactorApi", (serviceProvider, client) =>
        {
            var config = serviceProvider.GetRequiredService<IOptions<MultifactorApiConfiguration>>().Value;
            client.BaseAddress = new Uri(config.BaseUrl);
            client.DefaultRequestHeaders.Add("X-API-Key", config.ApiKey);
            client.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds);
        });

        services.AddScoped<IMultifactorApiService, MultifactorApiService>();

        return services;
    }
}
