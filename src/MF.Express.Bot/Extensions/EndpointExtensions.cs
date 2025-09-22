using MF.Express.Bot.Api.Endpoints;
using MF.Express.Bot.Api.Endpoints.Groups;
using System.Reflection;

namespace MF.Express.Bot.Api.Extensions;

/// <summary>
/// Расширения для автоматической регистрации endpoints
/// </summary>
public static class EndpointExtensions
{
    /// <summary>
    /// Регистрирует все endpoints, реализующие IEndpoint
    /// </summary>
    public static WebApplication MapEndpoints(this WebApplication app)
    {
        var endpoints = GetEndpoints();

        foreach (var endpoint in endpoints)
        {
            endpoint.MapEndpoint(app);
        }

        return app;
    }

    /// <summary>
    /// Альтернативный способ - простая регистрация без группировки
    /// </summary>
    public static WebApplication MapSimpleEndpoints(this WebApplication app)
    {
        new RegisterUserEndpoint().MapEndpoint(app);
        new SendAuthRequestEndpoint().MapEndpoint(app);
        new SendMessageEndpoint().MapEndpoint(app);
        new BotStatusEndpoint().MapEndpoint(app);

        return app;
    }

    private static IEnumerable<IEndpoint> GetEndpoints()
    {
        var endpointType = typeof(IEndpoint);
        var assembly = Assembly.GetExecutingAssembly();

        return assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && 
                          type.IsAssignableTo(endpointType))
            .Select(Activator.CreateInstance)
            .Cast<IEndpoint>();
    }
}
