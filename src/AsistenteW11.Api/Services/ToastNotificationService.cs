using System.Diagnostics;
using System.Text;
using AsistenteW11.Api.Models;
using Microsoft.Extensions.Options;

namespace AsistenteW11.Api.Services;

public sealed class ToastNotificationService
{
    private readonly BotOptions _options;
    private readonly ILogger<ToastNotificationService> _logger;

    public ToastNotificationService(IOptions<BotOptions> options, ILogger<ToastNotificationService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task ShowAsync(LogEntryRequest request, CancellationToken cancellationToken = default)
    {
        var level = request.Level!.Trim();
        var source = request.Source!.Trim();
        var title = $"{level.ToUpperInvariant()} - {source}";
        var message = request.Message!.Trim();

        var script = $@"
[Windows.UI.Notifications.ToastNotificationManager, Windows.UI.Notifications, ContentType = WindowsRuntime] > $null
[Windows.Data.Xml.Dom.XmlDocument, Windows.Data.Xml.Dom.XmlDocument, ContentType = WindowsRuntime] > $null

$title = '{EscapePowerShellString(title)}'
$message = '{EscapePowerShellString(message)}'
$xml = @""<toast><visual><binding template='ToastGeneric'><text>$title</text><text>$message</text></binding></visual></toast>""@

$doc = New-Object Windows.Data.Xml.Dom.XmlDocument
$doc.LoadXml($xml)
$toast = [Windows.UI.Notifications.ToastNotification]::new($doc)
[Windows.UI.Notifications.ToastNotificationManager]::CreateToastNotifier('{EscapePowerShellString(_options.ToastAppId)}').Show($toast)
";

        var encoded = Convert.ToBase64String(Encoding.Unicode.GetBytes(script));

        var startInfo = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -NonInteractive -WindowStyle Hidden -ExecutionPolicy Bypass -EncodedCommand {encoded}",
            UseShellExecute = false,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        if (process is null)
        {
            _logger.LogWarning("No se pudo iniciar powershell para notificacion toast.");
            return;
        }

        await process.WaitForExitAsync(cancellationToken);
        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync(cancellationToken);
            _logger.LogWarning("Fallo toast notification. ExitCode={ExitCode}. Error={Error}", process.ExitCode, error);
        }
    }

    private static string EscapePowerShellString(string input) => input.Replace("'", "''");
}
