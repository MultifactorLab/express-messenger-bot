using System.ComponentModel.DataAnnotations;

namespace MF.Express.Bot.Infrastructure.Configuration;

public class ExpressBotConfiguration
{
    public const string SectionName = "ExpressBot";

    [Required]
    public string BotId { get; set; } = string.Empty;

    [Required]
    public string BotSecretKey { get; set; } = string.Empty;

    [Required]
    public string BotXApiBaseUrl { get; set; } = string.Empty;

    [Required]
    public string ExpectedIssuer { get; set; } = string.Empty;

    public int RequestTimeoutSeconds { get; set; } = 30;
}

