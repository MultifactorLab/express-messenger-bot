using MF.Express.Bot.Application.UseCases;
using MF.Express.Bot.Application.UseCases.Auth;
using MF.Express.Bot.Api.DTOs.SendAuthResult;

namespace MF.Express.Bot.Api.Endpoints;

public class SendAuthResultEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/send-auth-result", HandleAsync)
            .WithName("SendAuthResult")
            .Produces<SendAuthResultResponseDto>(StatusCodes.Status400BadRequest)
            .Produces<SendAuthResultResponseDto>(StatusCodes.Status500InternalServerError)
            .Produces<SendAuthResultResponseDto>(StatusCodes.Status502BadGateway)
            .ProducesValidationProblem();
    }

    private static async Task<IResult> HandleAsync(
        SendAuthResultDto dto,
        IUseCase<SendAuthResultRequest, SendAuthResultResult> useCase,
        CancellationToken ct)
    {
        var request = SendAuthResultDto.ToRequest(dto);
        var result = await useCase.ExecuteAsync(request, ct);
        var responseDto = SendAuthResultResponseDto.FromResult(result);
        
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


