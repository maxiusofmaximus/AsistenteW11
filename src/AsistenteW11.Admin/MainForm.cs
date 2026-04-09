using System.Net.Http.Json;
using System.Text.Json;

namespace AsistenteW11.Admin;

public sealed class MainForm : Form
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(5) };

    private readonly TextBox _txtBaseUrl;
    private readonly TextBox _txtLogPath;
    private readonly TextBox _txtTestMessage;
    private readonly TextBox _txtTestSource;
    private readonly ComboBox _cmbLevel;
    private readonly Label _lblHealth;
    private readonly Label _lblStatus;
    private readonly DataGridView _gridSources;
    private readonly DataGridView _gridLogs;

    public MainForm()
    {
        Text = "AsistenteW11 Admin";
        Width = 1200;
        Height = 760;
        StartPosition = FormStartPosition.CenterScreen;

        var topPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 170,
            Padding = new Padding(10)
        };

        var lblBaseUrl = new Label { Text = "Base URL API", Left = 10, Top = 12, Width = 110 };
        _txtBaseUrl = new TextBox { Left = 130, Top = 8, Width = 320, Text = "http://127.0.0.1:5055" };

        var lblLogPath = new Label { Text = "Ruta log bot", Left = 470, Top = 12, Width = 100 };
        _txtLogPath = new TextBox
        {
            Left = 580,
            Top = 8,
            Width = 420,
            Text = @"C:\ProgramData\AsistenteW11\logs\assistant.log"
        };

        var btnHealth = new Button { Left = 10, Top = 44, Width = 140, Height = 30, Text = "Verificar Health" };
        btnHealth.Click += async (_, _) => await CheckHealthAsync();

        _lblHealth = new Label
        {
            Left = 165,
            Top = 50,
            Width = 420,
            Text = "Estado API: pendiente",
            AutoSize = false
        };

        var btnLoadLogs = new Button { Left = 580, Top = 44, Width = 140, Height = 30, Text = "Recargar Logs" };
        btnLoadLogs.Click += async (_, _) => await LoadLogsAsync();

        var lblLevel = new Label { Text = "Nivel", Left = 10, Top = 94, Width = 40 };
        _cmbLevel = new ComboBox
        {
            Left = 60,
            Top = 90,
            Width = 90,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _cmbLevel.Items.AddRange(["Info", "Warn", "Error", "Debug", "Trace", "Fatal"]);
        _cmbLevel.SelectedIndex = 0;

        var lblSource = new Label { Text = "Software", Left = 165, Top = 94, Width = 60 };
        _txtTestSource = new TextBox { Left = 235, Top = 90, Width = 180, Text = "AsistenteW11.Admin" };

        var lblMsg = new Label { Text = "Mensaje", Left = 430, Top = 94, Width = 60 };
        _txtTestMessage = new TextBox { Left = 500, Top = 90, Width = 500, Text = "Prueba de notificacion toast desde Admin UI." };

        var btnTestToast = new Button { Left = 1010, Top = 88, Width = 150, Height = 30, Text = "Enviar Test Toast" };
        btnTestToast.Click += async (_, _) => await SendTestToastAsync();

        _lblStatus = new Label
        {
            Left = 10,
            Top = 130,
            Width = 1150,
            Text = "Listo.",
            AutoSize = false
        };

        topPanel.Controls.AddRange([
            lblBaseUrl, _txtBaseUrl, lblLogPath, _txtLogPath, btnHealth, _lblHealth, btnLoadLogs, lblLevel, _cmbLevel,
            lblSource, _txtTestSource, lblMsg, _txtTestMessage, btnTestToast, _lblStatus
        ]);

        var splitMain = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 220
        };

        var pnlSources = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
        var lblSources = new Label
        {
            Text = "Softwares/Fuentes detectadas (campo source)",
            Dock = DockStyle.Top,
            Height = 24
        };

        _gridSources = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AutoGenerateColumns = true
        };

        pnlSources.Controls.Add(_gridSources);
        pnlSources.Controls.Add(lblSources);

        var pnlLogs = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
        var lblLogs = new Label
        {
            Text = "Logs recientes",
            Dock = DockStyle.Top,
            Height = 24
        };

        _gridLogs = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AutoGenerateColumns = true
        };

        pnlLogs.Controls.Add(_gridLogs);
        pnlLogs.Controls.Add(lblLogs);

        splitMain.Panel1.Controls.Add(pnlSources);
        splitMain.Panel2.Controls.Add(pnlLogs);

        Controls.Add(splitMain);
        Controls.Add(topPanel);

        Shown += async (_, _) =>
        {
            await CheckHealthAsync();
            await LoadLogsAsync();
        };
    }

    private string GetBaseUrl() => _txtBaseUrl.Text.Trim().TrimEnd('/');

    private async Task CheckHealthAsync()
    {
        try
        {
            var baseUrl = GetBaseUrl();
            var response = await Http.GetAsync($"{baseUrl}/health");
            if (response.IsSuccessStatusCode)
            {
                _lblHealth.Text = $"Estado API: OK ({(int)response.StatusCode})";
                _lblHealth.ForeColor = Color.DarkGreen;
                _lblStatus.Text = "Health check correcto.";
            }
            else
            {
                _lblHealth.Text = $"Estado API: Error ({(int)response.StatusCode})";
                _lblHealth.ForeColor = Color.DarkRed;
                _lblStatus.Text = $"Health check fallo con codigo {(int)response.StatusCode}.";
            }
        }
        catch (Exception ex)
        {
            _lblHealth.Text = "Estado API: sin conexion";
            _lblHealth.ForeColor = Color.DarkRed;
            _lblStatus.Text = $"No se pudo conectar a la API: {ex.Message}";
        }
    }

    private async Task SendTestToastAsync()
    {
        var message = _txtTestMessage.Text.Trim();
        var source = _txtTestSource.Text.Trim();
        var level = _cmbLevel.SelectedItem?.ToString() ?? "Info";

        if (string.IsNullOrWhiteSpace(message) || string.IsNullOrWhiteSpace(source))
        {
            _lblStatus.Text = "Completa mensaje y software antes de enviar prueba.";
            return;
        }

        try
        {
            var payload = new
            {
                message,
                level,
                source,
                context = new
                {
                    origin = "AdminUI",
                    machine = Environment.MachineName
                }
            };

            var baseUrl = GetBaseUrl();
            var response = await Http.PostAsJsonAsync($"{baseUrl}/logs", payload);
            if (response.IsSuccessStatusCode)
            {
                _lblStatus.Text = "Log de prueba enviado. Si notificaciones estan habilitadas, deberias ver toast.";
                await LoadLogsAsync();
            }
            else
            {
                var body = await response.Content.ReadAsStringAsync();
                _lblStatus.Text = $"Fallo envio de prueba ({(int)response.StatusCode}): {body}";
            }
        }
        catch (Exception ex)
        {
            _lblStatus.Text = $"Error enviando prueba: {ex.Message}";
        }
    }

    private async Task LoadLogsAsync()
    {
        try
        {
            var path = _txtLogPath.Text.Trim();
            if (!File.Exists(path))
            {
                _gridLogs.DataSource = null;
                _gridSources.DataSource = null;
                _lblStatus.Text = $"No existe el archivo de log: {path}";
                return;
            }

            var lines = await File.ReadAllLinesAsync(path);
            var parsed = lines
                .Select(ParseLine)
                .Where(x => x is not null)
                .Select(x => x!)
                .ToList();

            var recent = parsed
                .OrderByDescending(x => x.Utc)
                .Take(300)
                .ToList();

            var sources = parsed
                .GroupBy(x => x.Source)
                .Select(g => new SourceSummary
                {
                    Source = g.Key,
                    Total = g.Count(),
                    LastUtc = g.Max(x => x.Utc)
                })
                .OrderByDescending(x => x.Total)
                .ThenBy(x => x.Source)
                .ToList();

            _gridLogs.DataSource = recent;
            _gridSources.DataSource = sources;
            _lblStatus.Text = $"Logs cargados: {parsed.Count}. Fuentes detectadas: {sources.Count}.";
        }
        catch (Exception ex)
        {
            _lblStatus.Text = $"Error leyendo logs: {ex.Message}";
        }
    }

    private static LogRow? ParseLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(line);
            var root = doc.RootElement;

            var utcText = TryGetString(root, "utc") ?? TryGetString(root, "timestamp");
            var utc = DateTimeOffset.TryParse(utcText, out var parsedUtc) ? parsedUtc : DateTimeOffset.MinValue;

            var level = TryGetString(root, "level") ?? "n/a";
            var source = TryGetString(root, "source") ?? "n/a";
            var message = TryGetString(root, "message") ?? string.Empty;

            return new LogRow
            {
                Utc = utc,
                Level = level,
                Source = source,
                Message = message
            };
        }
        catch
        {
            return null;
        }
    }

    private static string? TryGetString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private sealed class LogRow
    {
        public DateTimeOffset Utc { get; init; }
        public string Level { get; init; } = string.Empty;
        public string Source { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
    }

    private sealed class SourceSummary
    {
        public string Source { get; init; } = string.Empty;
        public int Total { get; init; }
        public DateTimeOffset LastUtc { get; init; }
    }
}
