using MF.Express.Bot.Application.Interfaces;
using MF.Express.Bot.Infrastructure.Configuration;
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
        services.AddMultifactorApiService();
        services.AddBotXApiService();
        return services;
    }

    /// <summary>
    /// Добавляет BotX API сервис
    /// </summary>
    public static IServiceCollection AddBotXApiService(this IServiceCollection services)
    {
        services.AddHttpClient("BotX", (serviceProvider, client) =>
        {
            var config = serviceProvider.GetRequiredService<IOptions<ExpressBotConfiguration>>().Value;
            client.BaseAddress = new Uri(config.BotXApiBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(config.RequestTimeoutSeconds);
        });

        services.AddScoped<IBotXApiService, BotXApiService>();
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
