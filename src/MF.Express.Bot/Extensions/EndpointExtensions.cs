using MF.Express.Bot.Api.Endpoints;

namespace MF.Express.Bot.Api.Extensions;

/// <summary>
/// Расширения для автоматической регистрации endpoints
/// </summary>
public static class EndpointExtensions
{
    /// <summary>
    /// Регистрация основных endpoints для упрощенного MF бота
    /// </summary>
    public static WebApplication MapSimpleEndpoints(this WebApplication app)
    {
        new BotCommandEndpoint().MapEndpoint(app);
        new BotStatusEndpoint().MapEndpoint(app);
        new NotificationCallbackEndpoint().MapEndpoint(app);
        
        new SendAuthRequestEndpoint().MapEndpoint(app);

        return app;
    }
}
