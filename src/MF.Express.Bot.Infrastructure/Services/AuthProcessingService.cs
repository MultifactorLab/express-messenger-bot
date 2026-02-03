using Microsoft.Extensions.Logging;
using MF.Express.Bot.Application.Models.Common;
using MF.Express.Bot.Application.Interfaces;
using MF.Express.Bot.Application.Services;

namespace MF.Express.Bot.Infrastructure.Services;

public class AuthProcessingService : IAuthProcessingService
{
    private readonly IMfExpressApiService _mfExpressApiService;
    private readonly ILogger<AuthProcessingService> _logger;

    public AuthProcessingService(
        IMfExpressApiService mfExpressApiService,
        ILogger<AuthProcessingService> logger)
    {
        _mfExpressApiService = mfExpressApiService;
        _logger = logger;
    }

    public async Task<CommandProcessedResponse> ProcessAuthCallbackAsync(
        string callbackData,
        string chatId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Proxying auth callback to MF Express API");

            var success = await _mfExpressApiService.SendAuthCallbackAsync(
                callbackData,
                chatId,
                cancellationToken);

            if (!success)
            {
                _logger.LogWarning("Failed to send callback to MF Express API");
                return new CommandProcessedResponse(false, "Ошибка при отправке callback в MF Express API");
            }

            _logger.LogInformation("Callback sent successfully to MF Express API");
            return new CommandProcessedResponse(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception proxying auth callback");
            
            return new CommandProcessedResponse(false, $"Внутренняя ошибка: {ex.Message}");
        }
    }
}

