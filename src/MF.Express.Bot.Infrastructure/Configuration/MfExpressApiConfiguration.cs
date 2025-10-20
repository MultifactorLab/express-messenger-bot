using System.ComponentModel.DataAnnotations;

namespace MF.Express.Bot.Infrastructure.Configuration;

public class MfExpressApiConfiguration
{
    public const string SectionName = "MfExpressApi";
    
    [Required]
    public string BaseUrl { get; set; } = null!;
    
    public int TimeoutSeconds { get; set; } = 30;
    
    public string ChatCreatedEndpoint { get; set; } = "/api/express/webhook/chat-created";
    
    public string AuthCallbackEndpoint { get; set; } = "/api/express/webhook/auth-callback";
}

