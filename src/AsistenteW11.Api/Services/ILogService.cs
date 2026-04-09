using AsistenteW11.Api.Models;

namespace AsistenteW11.Api.Services;

public interface ILogService
{
    Task AppendAsync(LogEntryRequest request, CancellationToken cancellationToken = default);
}
