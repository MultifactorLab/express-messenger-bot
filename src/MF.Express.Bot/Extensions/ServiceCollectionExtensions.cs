using MF.Express.Bot.Application.UseCases;
using MF.Express.Bot.Application.UseCases.Auth;
using MF.Express.Bot.Application.UseCases.BotCommands;
using MF.Express.Bot.Application.UseCases.Greeting;
using MF.Express.Bot.Application.UseCases.Notifications;

namespace MF.Express.Bot.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddUseCases();
        
        return services;
    }

    private static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddScoped<IUseCase<BotCommandRequest, BotCommandResult>, ProcessBotCommandUseCase>();
        
        services.AddScoped<IUseCase<SendAuthRequestRequest, SendAuthRequestResult>, SendAuthRequestUseCase>();
        services.AddScoped<IUseCase<SendAuthResultRequest, SendAuthResultResult>, SendAuthResultUseCase>();
        services.AddScoped<IUseCase<SendGreetingRequest, SendGreetingResult>, SendGreetingUseCase>();
        
        services.AddScoped<IUseCase<NotificationCallbackRequest, NotificationCallbackResult>, ProcessNotificationCallbackUseCase>();
        
        return services;
    }
}
