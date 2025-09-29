using MF.Express.Bot.Application.DTOs;
using MF.Express.Bot.Application.Interfaces;
using Microsoft.Extensions.Options;
using MF.Express.Bot.Infrastructure.Configuration;

namespace MF.Express.Bot.Api.Endpoints;

/// <summary>
/// Bot API v3/v4 endpoint для передачи статуса и списка команд бота
/// Согласно документации https://docs.express.ms/chatbots/developer-guide/api/bot-api/status/
/// </summary>
public class BotStatusEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/status", HandleAsync)
            .WithName("GetBotStatus")
            .Produces<BotStatusResponse>(200)
            .Produces<BotApiErrorResponse>(503);
    }

    private static async Task<IResult> HandleAsync(
        IOptions<ExpressBotConfiguration> config,
        ILogger<BotStatusEndpoint> logger,
        CancellationToken ct)
    {
        try
        {
            logger.LogDebug("Запрос статуса бота {BotId}", config.Value.BotId);

            var commands = new List<BotCommand>
            {
                new BotCommand(
                    Name: "/start",
                    Body: "/start",
                    Description: "Начать работу с ботом"
                )
            };

            var response = new BotStatusResponse(
                Status: "ok",
                Result: new BotStatusResult(
                    Enabled: true,
                    StatusMessage: string.Empty,
                    Commands: commands
                )
            );
            logger.LogInformation("Статус бота успешно возвращен: {CommandCount} команд", commands.Count);

            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при получении статуса бота");

            var errorResponse = new BotApiErrorResponse(
                Reason: "internal_error",
                ErrorData: new Dictionary<string, object> 
                { 
                    { "message", "Внутренняя ошибка сервера" },
                    { "timestamp", DateTime.UtcNow.ToString("O") }
                },
                Errors: new List<object> { ex.Message }
            );

            return Results.Json(errorResponse, statusCode: 503);
        }
    }
}