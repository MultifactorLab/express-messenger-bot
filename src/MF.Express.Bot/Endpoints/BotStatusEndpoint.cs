using MF.Express.Bot.Api.DTOs.BotStatus;
using MF.Express.Bot.Api.DTOs.Common;
using MF.Express.Bot.Application.Models.BotX;
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
            .Produces<BotStatusResponseDto>(200)
            .Produces<BotApiErrorResponseDto>(503);
    }

    private static async Task<IResult> HandleAsync(
        IOptions<ExpressBotConfiguration> config,
        ILogger<BotStatusEndpoint> logger,
        CancellationToken ct)
    {
        try
        {
            logger.LogDebug("Запрос статуса бота {BotId}", config.Value.BotId);

            var commands = new List<BotCommandInfoModel>
            {
                new BotCommandInfoModel(
                    Name: "/start",
                    Body: "/start",
                    Description: "Начать работу с ботом"
                )
            };

            var statusModel = new BotStatusModel(
                Status: "ok",
                Result: new BotStatusResultModel(
                    Enabled: true,
                    StatusMessage: string.Empty,
                    Commands: commands
                )
            );
            
            logger.LogInformation("Статус бота успешно возвращен: {CommandCount} команд", commands.Count);
            var responseDto = BotStatusResponseDto.FromAppModel(statusModel);
            return Results.Ok(responseDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при получении статуса бота");

            var errorResponse = new BotApiErrorResponseDto(
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