namespace MF.Express.Bot.Api.Endpoints;

/// <summary>
/// Интерфейс для регистрации endpoint'ов
/// </summary>
public interface IEndpoint
{
    /// <summary>
    /// Регистрирует endpoint
    /// </summary>
    /// <param name="app">WebApplication для регистрации</param>
    void MapEndpoint(IEndpointRouteBuilder app);
}
