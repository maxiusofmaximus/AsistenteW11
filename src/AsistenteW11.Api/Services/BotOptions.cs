namespace AsistenteW11.Api.Services;

public sealed class BotOptions
{
    public const string SectionName = "Bot";
    public string LogFilePath { get; set; } = @"C:\ProgramData\AsistenteW11\logs\assistant.log";
    public string ToastAppId { get; set; } = "Microsoft.Windows.Explorer";
}
