using System.ComponentModel.DataAnnotations;

namespace MF.Express.Bot.Infrastructure.Configuration;

public class MultifactorApiConfiguration
{
    public const string SectionName = "MultifactorApi";
    
    [Required]
    public string BaseUrl { get; set; } = default!;
    
    [Required]
    public string ApiKey { get; set; } = default!;
    
    public int TimeoutSeconds { get; set; } = 30;
    
    public int RetryCount { get; set; } = 3;
    
    public string UserChatInfoEndpoint { get; set; } = "/api/bot/user-chat-info";
    
    public string AuthorizationResultEndpoint { get; set; } = "/api/bot/authorization-result";

    public string UserStartCommandEndpoint { get; set; } = "/api/bot/user-start-data";
}
