using MF.Express.Bot.Application.Commands;
using MF.Express.Bot.Application.DTOs;

namespace MF.Express.Bot.Api.Endpoints;

/// <summary>
/// Endpoint для отправки простого сообщения через бота (для тестирования)
/// </summary>
public class SendMessageEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/send", HandleAsync)
            .WithName("SendMessage")
            .WithOpenApi()
            .Produces<SendMessageResultDto>()
            .ProducesValidationProblem()
            .WithTags("Testing");
    }

    private static async Task<IResult> HandleAsync(
        SendMessageRequestDto request,
        ICommand<SendMessageCommand, SendMessageResultDto> handler,
        CancellationToken ct)
    {
        var command = new SendMessageCommand(
            request.ChatId, 
            request.Text, 
            request.Type, 
            request.ReplyToMessageId, 
            request.Metadata);
            
        var result = await handler.Handle(command, ct);
        return Results.Ok(result);
    }
}
