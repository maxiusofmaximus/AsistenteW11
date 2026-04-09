using System.Text.Json;

namespace AsistenteW11.Api.Models;

public sealed class LogEntryRequest
{
    public string? Message { get; init; }
    public string? Level { get; init; }
    public string? Source { get; init; }
    public JsonElement? Context { get; init; }

    public Dictionary<string, string[]> Validate()
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(Message))
        {
            errors["message"] = ["message es obligatorio."];
        }
        else if (Message.Length > 1024)
        {
            errors["message"] = ["message no puede superar 1024 caracteres."];
        }

        if (string.IsNullOrWhiteSpace(Level))
        {
            errors["level"] = ["level es obligatorio."];
        }
        else if (Level.Length > 32)
        {
            errors["level"] = ["level no puede superar 32 caracteres."];
        }

        if (string.IsNullOrWhiteSpace(Source))
        {
            errors["source"] = ["source es obligatorio."];
        }
        else if (Source.Length > 128)
        {
            errors["source"] = ["source no puede superar 128 caracteres."];
        }

        return errors;
    }
}
