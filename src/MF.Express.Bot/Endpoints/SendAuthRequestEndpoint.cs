using MF.Express.Bot.Application.UseCases;
using MF.Express.Bot.Application.UseCases.Auth;
using MF.Express.Bot.Api.DTOs.SendAuthRequest;

namespace MF.Express.Bot.Api.Endpoints;

public class SendAuthRequestEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/send-auth-request", HandleAsync)
            .WithName("SendAuthRequest")
            .Produces<SendAuthResponseDto>(StatusCodes.Status400BadRequest)
            .Produces<SendAuthResponseDto>(StatusCodes.Status500InternalServerError)
            .Produces<SendAuthResponseDto>(StatusCodes.Status502BadGateway)
            .ProducesValidationProblem();
    }

    private static async Task<IResult> HandleAsync(
        SendAuthRequestDto dto,
        IUseCase<SendAuthRequestRequest, SendAuthRequestResult> useCase,
        CancellationToken ct)
    {
        var request = SendAuthRequestDto.ToRequest(dto);
        var result = await useCase.ExecuteAsync(request, ct);
        var responseDto = SendAuthResponseDto.FromResult(result);
        
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
