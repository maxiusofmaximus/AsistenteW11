using AsistenteW11.Api.Models;
using AsistenteW11.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<BotOptions>(builder.Configuration.GetSection(BotOptions.SectionName));
builder.Services.AddSingleton<ILogService, FileLogService>();
builder.Services.AddSingleton<IToastNotificationService, ToastNotificationService>();

var app = builder.Build();

app.MapGet("/health", () =>
{
    return Results.Ok(new
    {
        status = "ok",
        utc = DateTimeOffset.UtcNow
    });
});

app.MapPost("/logs", async (
    LogEntryRequest request,
    ILogService logService,
    IToastNotificationService toastService,
    CancellationToken cancellationToken) =>
{
    var validationErrors = request.Validate();
    if (validationErrors.Count > 0)
    {
        return Results.ValidationProblem(validationErrors);
    }

    await logService.AppendAsync(request, cancellationToken);
    await toastService.ShowAsync(request, cancellationToken);

    return Results.Accepted(value: new
    {
        status = "logged",
        utc = DateTimeOffset.UtcNow
    });
});

app.Run();
