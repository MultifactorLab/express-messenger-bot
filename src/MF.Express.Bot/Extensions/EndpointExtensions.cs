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
        var apiGroup = app.MapGroup("/api")
            .WithTags("Express Bot API");

        new BotCommandEndpoint().MapEndpoint(apiGroup);
        new BotStatusEndpoint().MapEndpoint(apiGroup);
        new NotificationCallbackEndpoint().MapEndpoint(apiGroup);
        new SendAuthRequestEndpoint().MapEndpoint(apiGroup);

        return app;
    }
}
