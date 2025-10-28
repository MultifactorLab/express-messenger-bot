using MF.Express.Bot.Api.DTOs.Common;
using MF.Express.Bot.Api.DTOs.VerifyBot;
using MF.Express.Bot.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace MF.Express.Bot.Api.Endpoints;

public class VerifyBotEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/verify", HandleAsync)
            .WithName("VerifyBot")
            .Produces<VerifyBotResponseDto>(200)
            .Produces<BotApiErrorResponseDto>(400)
            .Produces<BotApiErrorResponseDto>(401)
            .AllowAnonymous();
    }

    private static async Task<IResult> HandleAsync(
        VerifyBotRequestDto request,
        IOptions<ExpressBotConfiguration> config,
        ILogger<VerifyBotEndpoint> logger,
        CancellationToken ct)
    {
        try
        {
            logger.LogInformation("Bot verification requested. ProvidedBotId: {ProvidedBotId:l}", request.BotId);

            if (string.IsNullOrWhiteSpace(request.BotId) || string.IsNullOrWhiteSpace(request.Signature))
            {
                logger.LogWarning("Invalid verification request. Missing BotId or Signature");
                
                var badRequestResponse = new BotApiErrorResponseDto(
                    Reason: "validation_error",
                    ErrorData: new Dictionary<string, object> 
                    { 
                        { "message", "BotId and Signature are required" }
                    },
                    Errors: new List<object> { "Invalid request parameters" }
                );
                
                return Results.Json(badRequestResponse, statusCode: 400);
            }

            var expectedBotId = config.Value.BotId;
            var botSecret = config.Value.BotSecretKey;

            if (request.BotId != expectedBotId)
            {
                logger.LogWarning("Bot verification failed. BotId mismatch. ProvidedBotId: {ProvidedBotId:l}, ExpectedBotId: {ExpectedBotId:l}", 
                    request.BotId, expectedBotId);
                
                var response = new VerifyBotResponseDto(
                    Status: "failed",
                    BotId: request.BotId,
                    Verified: false
                );
                
                return Results.Json(response, statusCode: 401);
            }

            var expectedSignature = ComputeSignature(request.BotId, botSecret);

            if (request.Signature != expectedSignature)
            {
                logger.LogWarning("Bot verification failed. Signature mismatch. BotId: {BotId:l}", request.BotId);
                
                var response = new VerifyBotResponseDto(
                    Status: "failed",
                    BotId: request.BotId,
                    Verified: false
                );
                
                return Results.Json(response, statusCode: 401);
            }

            logger.LogInformation("Bot verification successful. BotId: {BotId:l}", request.BotId);

            var successResponse = new VerifyBotResponseDto(
                Status: "ok",
                BotId: request.BotId,
                Verified: true
            );

            return Results.Ok(successResponse);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to verify bot. ProvidedBotId: {ProvidedBotId:l}", request.BotId);

            var errorResponse = new BotApiErrorResponseDto(
                Reason: "internal_error",
                ErrorData: new Dictionary<string, object> 
                { 
                    { "message", "Internal server error during verification" },
                    { "timestamp", DateTime.UtcNow.ToString("O") }
                },
                Errors: new List<object> { ex.Message }
            );

            return Results.Json(errorResponse, statusCode: 500);
        }
    }

    private static string ComputeSignature(string botId, string botSecret)
    {
        var data = $"{botId}:{botSecret}";
        var bytes = Encoding.UTF8.GetBytes(data);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}



