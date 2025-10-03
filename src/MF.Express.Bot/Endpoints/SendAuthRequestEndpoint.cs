using MF.Express.Bot.Application.Commands;
using MF.Express.Bot.Api.DTOs.SendAuthRequest;
using MF.Express.Bot.Application.Models.SendAuthRequest;

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
            .Produces<SendAuthResponseDto>()
            .ProducesValidationProblem();
    }

    private static async Task<IResult> HandleAsync(
        SendAuthRequestDto dto,
        ICommand<SendAuthRequestCommand, SendAuthResultAppModel> handler,
        CancellationToken ct)
    {
        var command = SendAuthRequestDto.ToCommand(dto);
        var resultAppModel = await handler.Handle(command, ct);
        var responseDto = SendAuthResponseDto.FromAppModel(resultAppModel);
        return resultAppModel.Success ? Results.Ok(responseDto) : Results.BadRequest(responseDto);
    }
}
