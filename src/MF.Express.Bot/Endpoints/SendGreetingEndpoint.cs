using MF.Express.Bot.Application.UseCases;
using MF.Express.Bot.Application.UseCases.Auth;
using MF.Express.Bot.Application.UseCases.Greeting;
using MF.Express.Bot.Api.DTOs.SendGreeting;

namespace MF.Express.Bot.Api.Endpoints;

public class SendGreetingEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/send-greeting", HandleAsync)
            .WithName("SendGreeting")
            .Produces<SendGreetingResponseDto>(StatusCodes.Status400BadRequest)
            .Produces<SendGreetingResponseDto>(StatusCodes.Status500InternalServerError)
            .Produces<SendGreetingResponseDto>(StatusCodes.Status502BadGateway)
            .ProducesValidationProblem();
    }

    private static async Task<IResult> HandleAsync(
        SendGreetingDto dto,
        IUseCase<SendGreetingRequest, SendGreetingResult> useCase,
        CancellationToken ct)
    {
        var request = SendGreetingDto.ToRequest(dto);
        var result = await useCase.ExecuteAsync(request, ct);
        var responseDto = SendGreetingResponseDto.FromResult(result);
        
        if (result.Success)
        {
            return Results.Ok(responseDto);
        }

        return result.ErrorType switch
        {
            SendAuthErrorType.ValidationError => Results.BadRequest(responseDto),
            SendAuthErrorType.ExternalServiceError => Results.Json(responseDto, statusCode: StatusCodes.Status502BadGateway),
            SendAuthErrorType.InternalError => Results.Json(responseDto, statusCode: StatusCodes.Status500InternalServerError),
            _ => Results.Json(responseDto, statusCode: StatusCodes.Status500InternalServerError)
        };
    }
}

