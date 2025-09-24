using MF.Express.Bot.Application.Commands;
using MF.Express.Bot.Application.DTOs;

namespace MF.Express.Bot.Api.Endpoints;

/// <summary>
/// Endpoint для обработки входящих сообщений от пользователей Express
/// </summary>
public class IncomingMessageEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/webhook/message", HandleAsync)
            .WithName("ProcessIncomingMessage")
            .WithOpenApi()
            .Produces<WebhookProcessedResponse>()
            .WithTags("Webhook");
    }

    private static async Task<IResult> HandleAsync(
        IncomingMessageDto message,
        ICommand<ProcessIncomingMessageCommand, WebhookProcessedResponse> handler,
        CancellationToken ct)
    {
        var command = new ProcessIncomingMessageCommand(
            message.ChatId,
            message.UserId,
            message.Text,
            message.Timestamp,
            message.MessageId,
            message.Username,
            message.FirstName,
            message.LastName,
            message.Metadata);
            
        await handler.Handle(command, ct);
        
        // Возвращаем 202 Accepted как в pybotx
        return Results.Accepted();
    }
}
