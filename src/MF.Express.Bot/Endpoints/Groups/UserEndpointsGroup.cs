using MF.Express.Bot.Application.Commands;
using MF.Express.Bot.Application.DTOs;

namespace MF.Express.Bot.Api.Endpoints.Groups;

/// <summary>
/// Группа endpoints для управления пользователями
/// </summary>
public static class UserEndpointsGroup
{
    /// <summary>
    /// Регистрирует группу endpoints для пользователей
    /// </summary>
    public static RouteGroupBuilder MapUserEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/users")
            .WithTags("User Management")
            .WithOpenApi();

        group.MapPost("/register", RegisterUserAsync)
            .WithName("RegisterUser")
            .Produces<RegisterUserResultDto>()
            .ProducesValidationProblem();

        return group;
    }

    private static async Task<IResult> RegisterUserAsync(
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
