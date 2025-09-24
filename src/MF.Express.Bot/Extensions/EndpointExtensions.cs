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
        // Bot API v4 endpoints - основной функционал
        new BotCommandEndpoint().MapEndpoint(app);   // Функции 1 и 3: прием команд от BotX
        new BotStatusEndpoint().MapEndpoint(app);    // Статус бота для BotX
        
        // MF API endpoints - для взаимодействия с Multifactor API
        new SendAuthRequestEndpoint().MapEndpoint(app);  // Функция 2: отправка запроса авторизации

        return app;
    }
}
