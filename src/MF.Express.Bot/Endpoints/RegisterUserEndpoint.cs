using MF.Express.Bot.Application.Commands;
using MF.Express.Bot.Application.DTOs;

namespace MF.Express.Bot.Api.Endpoints;

/// <summary>
/// Endpoint для регистрации пользователя в чате с ботом
/// </summary>
public class RegisterUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/register-user", HandleAsync)
            .WithName("RegisterUser")
            .WithOpenApi()
            .WithSummary("Регистрация пользователя в чате с ботом")
            .WithDescription("Регистрирует пользователя в чате и отправляет приветственное сообщение")
            .Produces<RegisterUserResultDto>()
            .ProducesValidationProblem()
            .WithTags("User Management");
    }

    private static async Task<IResult> HandleAsync(
        RegisterUserRequestDto request,
        ICommand<RegisterUserCommand, RegisterUserResultDto> handler,
        CancellationToken ct)
    {
        var command = new RegisterUserCommand(
            request.ChatId,
            request.UserId,
            request.Username,
            request.FirstName,
            request.LastName,
            request.Metadata);
            
        var result = await handler.Handle(command, ct);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }
}
