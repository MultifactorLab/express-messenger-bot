using System.ComponentModel.DataAnnotations;

namespace MF.Express.Bot.Infrastructure.Configuration;

public class MfExpressApiConfiguration
{
    [Required] public string BaseUrl { get; set; } = "https://mf-express-service.ru.tuna.am/";
    
    public int TimeoutSeconds { get; set; } = 30;
    
    public string ChatCreatedEndpoint { get; set; } = "/api/express/webhook/chat-created";
    
    public string AuthCallbackEndpoint { get; set; } = "/api/express/webhook/auth-callback";
}

