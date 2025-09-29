using MF.Express.Bot.Application.Commands;
using MF.Express.Bot.Application.DTOs;

namespace MF.Express.Bot.Api.Endpoints;

/// <summary>
/// Endpoint для отправки запроса авторизации с кнопками подтверждения
/// </summary>
public class SendAuthRequestEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/send-auth-request", HandleAsync)
            .WithName("SendAuthRequest")
            .Produces<SendAuthResultDto>()
            .ProducesValidationProblem();
    }

    private static async Task<IResult> HandleAsync(
        SendAuthRequestDto request,
        ICommand<SendAuthRequestCommand, SendAuthResultDto> handler,
        CancellationToken ct)
    {
        var command = new SendAuthRequestCommand(
            request.ChatId,
            request.UserId,
            request.AuthRequestId,
            request.Message,
            request.ResourceName,
            request.Metadata);
            
        var result = await handler.Handle(command, ct);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }
}
