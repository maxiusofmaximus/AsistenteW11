using System.Text.Json;
using AsistenteW11.Api.Models;
using Microsoft.Extensions.Options;

namespace AsistenteW11.Api.Services;

public sealed class FileLogService : ILogService
{
    private static readonly SemaphoreSlim WriteLock = new(1, 1);
    private readonly BotOptions _options;

    public FileLogService(IOptions<BotOptions> options)
    {
        _options = options.Value;
    }

    public async Task AppendAsync(LogEntryRequest request, CancellationToken cancellationToken = default)
    {
        var path = _options.LogFilePath;
        var directory = Path.GetDirectoryName(path);

        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new InvalidOperationException("LogFilePath no es valido.");
        }

        Directory.CreateDirectory(directory);

        var entry = new
        {
            utc = DateTimeOffset.UtcNow,
            level = request.Level!.Trim(),
            source = request.Source!.Trim(),
            message = request.Message!.Trim(),
            context = request.Context?.ValueKind == JsonValueKind.Undefined ? null : request.Context
        };

        var line = JsonSerializer.Serialize(entry) + Environment.NewLine;

        await WriteLock.WaitAsync(cancellationToken);
        try
        {
            await File.AppendAllTextAsync(path, line, cancellationToken);
        }
        finally
        {
            WriteLock.Release();
        }
    }
}
