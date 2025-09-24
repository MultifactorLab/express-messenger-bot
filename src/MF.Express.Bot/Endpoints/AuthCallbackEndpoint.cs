using MF.Express.Bot.Application.Commands;
using MF.Express.Bot.Application.DTOs;

namespace MF.Express.Bot.Api.Endpoints;

/// <summary>
/// Endpoint для обработки callback от кнопок авторизации
/// </summary>
public class AuthCallbackEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/webhook/auth-callback", HandleAsync)
            .WithName("ProcessAuthCallback")
            .WithOpenApi()
            .Produces<WebhookProcessedResponse>()
            .WithTags("Webhook");
    }

    private static async Task<IResult> HandleAsync(
        AuthCallbackDto callback,
        ICommand<ProcessAuthCallbackCommand, WebhookProcessedResponse> handler,
        CancellationToken ct)
    {
        var command = new ProcessAuthCallbackCommand(
            callback.CallbackId,
            callback.UserId,
            callback.ChatId,
            callback.AuthRequestId,
            callback.Action,
            callback.Timestamp,
            callback.MessageId,
            callback.Metadata);
            
        await handler.Handle(command, ct);
        
        // Возвращаем 202 Accepted
        return Results.Accepted();
    }
}
