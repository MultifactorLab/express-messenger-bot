using MF.Express.Bot.Application.Commands;
using MF.Express.Bot.Application.DTOs;

namespace MF.Express.Bot.Api.Endpoints.Groups;

/// <summary>
/// Группа endpoints для авторизации
/// </summary>
public static class AuthEndpointsGroup
{
    /// <summary>
    /// Регистрирует группу endpoints для авторизации
    /// </summary>
    public static RouteGroupBuilder MapAuthEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/auth")
            .WithTags("Authentication")
            .WithOpenApi();

        group.MapPost("/send-request", SendAuthRequestAsync)
            .WithName("SendAuthRequest")
            .Produces<SendAuthResultDto>()
            .ProducesValidationProblem();

        return group;
    }

    private static async Task<IResult> SendAuthRequestAsync(
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
