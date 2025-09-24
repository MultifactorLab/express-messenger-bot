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
    /// Простая регистрация всех endpoints (исходящие API и входящие webhook'и)
    /// </summary>
    public static WebApplication MapSimpleEndpoints(this WebApplication app)
    {
        // Исходящие API endpoints
        new RegisterUserEndpoint().MapEndpoint(app);
        new SendAuthRequestEndpoint().MapEndpoint(app);
        new SendMessageEndpoint().MapEndpoint(app);
        new BotStatusEndpoint().MapEndpoint(app);

        // Webhook endpoints для проверок (в будущем будут напрямую отправляться в Multifactor)
        new IncomingMessageEndpoint().MapEndpoint(app);
        new AuthCallbackEndpoint().MapEndpoint(app);

        return app;
    }
}
