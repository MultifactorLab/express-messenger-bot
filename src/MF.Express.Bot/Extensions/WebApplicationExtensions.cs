using MF.Express.Bot.Api.Endpoints;

namespace MF.Express.Bot.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication MapEndpoints(this WebApplication app)
    {
        var apiGroup = app.MapGroup("/api")
            .WithTags("Express Bot API");

        apiGroup.RegisterEndpoint<BotCommandEndpoint>();
        apiGroup.RegisterEndpoint<BotStatusEndpoint>();
        apiGroup.RegisterEndpoint<NotificationCallbackEndpoint>();
        apiGroup.RegisterEndpoint<SendAuthRequestEndpoint>();
        apiGroup.RegisterEndpoint<SendAuthResultEndpoint>();
        apiGroup.RegisterEndpoint<SendGreetingEndpoint>();
        apiGroup.RegisterEndpoint<VerifyBotEndpoint>();

        return app;
    }
    
    private static IEndpointRouteBuilder RegisterEndpoint<T>(this IEndpointRouteBuilder endpoints) where T : IEndpoint, new()
    {
        var endpoint = new T();
        endpoint.MapEndpoint(endpoints);
        return endpoints;
    }
}