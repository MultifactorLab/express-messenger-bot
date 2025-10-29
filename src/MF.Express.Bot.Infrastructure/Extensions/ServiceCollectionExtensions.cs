using MF.Express.Bot.Application.Interfaces;
using MF.Express.Bot.Application.Services;
using MF.Express.Bot.Infrastructure.Configuration;
using MF.Express.Bot.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MF.Express.Bot.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddMfExpressApiService();
        services.AddBotXApiService();
        services.AddApplicationServices();
        return services;
    }
    
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthProcessingService, AuthProcessingService>();
        return services;
    }

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

    public static IServiceCollection AddMfExpressApiService(this IServiceCollection services)
    {
        services.AddHttpClient("MfExpressApi", (serviceProvider, client) =>
        {
            var config = serviceProvider.GetRequiredService<IOptions<MfExpressApiConfiguration>>().Value;
            client.BaseAddress = new Uri(config.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(config.RequestTimeoutSeconds);
        });

        services.AddScoped<IMfExpressApiService, MfExpressApiService>();

        return services;
    }
}
