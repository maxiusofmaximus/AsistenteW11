using AsistenteW11.Api.Models;

namespace AsistenteW11.Api.Services;

public interface IToastNotificationService
{
    Task ShowAsync(LogEntryRequest request, CancellationToken cancellationToken = default);
}
