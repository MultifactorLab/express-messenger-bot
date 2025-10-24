using MF.Express.Bot.Application.UseCases.Auth;
using MF.Express.Bot.Application.UseCases.BotCommands;
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
        // Main router
        services.AddScoped<IProcessBotCommandUseCase, ProcessBotCommandUseCase>();
        
        // Command routers
        services.AddScoped<IProcessUserCommandUseCase, ProcessUserCommandUseCase>();
        services.AddScoped<IProcessSystemCommandUseCase, ProcessSystemCommandUseCase>();
        
        // Specific handlers
        services.AddScoped<IHandleStartCommandUseCase, HandleStartCommandUseCase>();
        services.AddScoped<IHandleAuthCallbackUseCase, HandleAuthCallbackUseCase>();
        services.AddScoped<IHandleChatCreatedUseCase, HandleChatCreatedUseCase>();
        services.AddScoped<IProcessIncomingMessageUseCase, ProcessIncomingMessageUseCase>();
        
        // Auth
        services.AddScoped<ISendAuthRequestUseCase, SendAuthRequestUseCase>();
        services.AddScoped<ISendAuthResultUseCase, SendAuthResultUseCase>();
        
        // Notifications
        services.AddScoped<IProcessNotificationCallbackUseCase, ProcessNotificationCallbackUseCase>();
        
        return services;
    }
}
