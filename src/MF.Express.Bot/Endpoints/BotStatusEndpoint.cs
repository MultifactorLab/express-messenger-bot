using MF.Express.Bot.Application.Interfaces;

namespace MF.Express.Bot.Api.Endpoints;

/// <summary>
/// Endpoint для получения статуса бота
/// </summary>
public class BotStatusEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/status", HandleAsync)
            .WithName("GetBotStatus")
            .WithOpenApi()
            .WithSummary("Получить статус бота")
            .WithDescription("Возвращает информацию о состоянии бота и подключении к Express API")
            .Produces<BotStatusResponse>()
            .WithTags("Monitoring");
    }

    private static async Task<IResult> HandleAsync(
        IExpressBotService botService,
        CancellationToken ct)
    {
        var botInfo = await botService.GetBotInfoAsync(ct);
        
        var response = new BotStatusResponse
        {
            BotInfo = botInfo,
            Status = botInfo.IsActive ? "Running" : "Inactive",
            Timestamp = DateTime.UtcNow,
            Version = typeof(BotStatusEndpoint).Assembly.GetName().Version?.ToString() ?? "Unknown"
        };

        return Results.Ok(response);
    }
}

/// <summary>
/// Ответ со статусом бота
/// </summary>
public record BotStatusResponse
{
    public required object BotInfo { get; init; }
    public required string Status { get; init; }
    public required DateTime Timestamp { get; init; }
    public required string Version { get; init; }
}
