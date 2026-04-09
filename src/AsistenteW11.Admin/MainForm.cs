using System.Net.Http.Json;
using System.Text.Json;
using System.Diagnostics;

namespace AsistenteW11.Admin;

public sealed class MainForm : Form
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(5) };

    private readonly Button _btnApiControl;
    private readonly Button _btnHealth;
    private readonly Button _btnLoadLogs;
    private readonly Button _btnTestToast;
    private readonly CheckBox _chkDarkMode;
    private readonly TextBox _txtBaseUrl;
    private readonly TextBox _txtLogPath;
    private readonly TextBox _txtTestMessage;
    private readonly TextBox _txtTestSource;
    private readonly ComboBox _cmbLevel;
    private readonly Label _lblHealth;
    private readonly Label _lblApiBadge;
    private readonly Label _lblStatus;
    private readonly DataGridView _gridSources;
    private readonly DataGridView _gridLogs;
    private readonly Label _lblSourcesTitle;
    private readonly Label _lblLogsTitle;
    private readonly Panel _topCard;
    private readonly Panel _sourcesCard;
    private readonly Panel _logsCard;
    private readonly TableLayoutPanel _shell;
    private bool _isDarkMode;
    private Process? _apiProcess;

    public MainForm()
    {
        Text = "AsistenteW11 Admin";
        Width = 1320;
        Height = 820;
        MinimumSize = new Size(1120, 700);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 10f, FontStyle.Regular);
        BackColor = Color.FromArgb(243, 248, 255);

        _shell = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1,
            BackColor = BackColor,
            Padding = new Padding(14, 14, 14, 10)
        };
        _shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 140));
        _shell.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
        _shell.RowStyles.Add(new RowStyle(SizeType.Percent, 60));

        _topCard = CreateCard();
        var top = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 4,
            ColumnCount = 8,
            BackColor = _topCard.BackColor,
            Padding = new Padding(10)
        };
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 17));
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 17));
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16));
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16));
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14));
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        top.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        top.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        top.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        top.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

        var lblBaseUrl = CreateLabel("Base URL API");
        _txtBaseUrl = CreateTextBox("http://127.0.0.1:5055");
        var lblLogPath = CreateLabel("Ruta log bot");
        _txtLogPath = CreateTextBox(@"C:\ProgramData\AsistenteW11\logs\assistant.log");

        _btnApiControl = CreatePrimaryButton("Iniciar API");
        _btnApiControl.Click += async (_, _) => await HandleApiControlClickAsync();
        _btnApiControl.TabIndex = 2;
        _btnApiControl.TabStop = true;
        AttachFocusStyle(_btnApiControl);

        _btnHealth = CreateSecondaryButton("Verificar");
        _btnHealth.Click += async (_, _) => await CheckHealthAsync();
        _btnHealth.TabIndex = 3;
        AttachFocusStyle(_btnHealth);

        _btnLoadLogs = CreateSecondaryButton("Recargar Logs");
        _btnLoadLogs.Click += async (_, _) => await LoadLogsAsync();
        _btnLoadLogs.TabIndex = 4;
        AttachFocusStyle(_btnLoadLogs);

        _chkDarkMode = new CheckBox
        {
            Text = "&Modo oscuro",
            Dock = DockStyle.Fill,
            Checked = false,
            TextAlign = ContentAlignment.MiddleLeft
        };
        _chkDarkMode.CheckedChanged += async (_, _) =>
        {
            _isDarkMode = _chkDarkMode.Checked;
            ApplyTheme();
            await RefreshApiControlStateAsync();
            await CheckHealthAsync();
        };

        var lblLevel = CreateLabel("Nivel");
        _cmbLevel = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            FlatStyle = FlatStyle.Flat
        };
        _cmbLevel.Items.AddRange(["Info", "Warn", "Error", "Debug", "Trace", "Fatal"]);
        _cmbLevel.SelectedIndex = 0;
        AttachFocusStyle(_cmbLevel);

        var lblSource = CreateLabel("Software");
        _txtTestSource = CreateTextBox("AsistenteW11.Admin");
        AttachFocusStyle(_txtTestSource);

        var lblMsg = CreateLabel("Mensaje");
        _txtTestMessage = CreateTextBox("Prueba de notificacion toast desde Admin UI.");
        AttachFocusStyle(_txtTestMessage);

        _btnTestToast = CreatePrimaryButton("Enviar Test");
        _btnTestToast.Click += async (_, _) => await SendTestToastAsync();
        AttachFocusStyle(_btnTestToast);

        AttachFocusStyle(_txtBaseUrl);
        AttachFocusStyle(_txtLogPath);

        _lblHealth = new Label
        {
            Dock = DockStyle.Fill,
            Text = "Estado API: pendiente",
            ForeColor = Color.FromArgb(28, 75, 125),
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI Semibold", 10f, FontStyle.Regular)
        };
        _lblApiBadge = new Label
        {
            Dock = DockStyle.Fill,
            Text = "OFFLINE",
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.White,
            BackColor = Color.FromArgb(184, 31, 31),
            Font = new Font("Segoe UI Semibold", 9f, FontStyle.Regular),
            Margin = new Padding(6, 4, 0, 4)
        };
        _lblStatus = new Label
        {
            Dock = DockStyle.Fill,
            Text = "Listo.",
            ForeColor = Color.FromArgb(66, 80, 104),
            TextAlign = ContentAlignment.MiddleLeft
        };

        top.Controls.Add(lblBaseUrl, 0, 0);
        top.Controls.Add(_txtBaseUrl, 1, 0);
        top.SetColumnSpan(_txtBaseUrl, 2);
        top.Controls.Add(lblLogPath, 3, 0);
        top.Controls.Add(_txtLogPath, 4, 0);
        top.SetColumnSpan(_txtLogPath, 3);
        top.Controls.Add(_btnApiControl, 7, 0);

        top.Controls.Add(_btnHealth, 0, 1);
        top.Controls.Add(_btnLoadLogs, 1, 1);
        top.Controls.Add(_lblHealth, 2, 1);
        top.SetColumnSpan(_lblHealth, 4);
        top.Controls.Add(_chkDarkMode, 6, 1);
        top.Controls.Add(_lblApiBadge, 7, 1);

        top.Controls.Add(lblLevel, 0, 2);
        top.Controls.Add(_cmbLevel, 1, 2);
        top.Controls.Add(lblSource, 2, 2);
        top.Controls.Add(_txtTestSource, 3, 2);
        top.Controls.Add(lblMsg, 4, 2);
        top.Controls.Add(_txtTestMessage, 5, 2);
        top.SetColumnSpan(_txtTestMessage, 2);
        top.Controls.Add(_btnTestToast, 7, 2);
        top.Controls.Add(_lblStatus, 0, 3);
        top.SetColumnSpan(_lblStatus, 8);
        _topCard.Controls.Add(top);

        _sourcesCard = CreateCard();
        var sourcesContainer = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1,
            BackColor = _sourcesCard.BackColor,
            Padding = new Padding(10)
        };
        sourcesContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        sourcesContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _lblSourcesTitle = new Label
        {
            Text = "Softwares/Fuentes detectadas",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI Semibold", 10.5f, FontStyle.Regular),
            ForeColor = Color.FromArgb(22, 57, 102),
            TextAlign = ContentAlignment.MiddleLeft
        };

        _gridSources = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AutoGenerateColumns = true
        };
        ApplyGridTheme(_gridSources);

        sourcesContainer.Controls.Add(_lblSourcesTitle, 0, 0);
        sourcesContainer.Controls.Add(_gridSources, 0, 1);
        _sourcesCard.Controls.Add(sourcesContainer);

        _logsCard = CreateCard();
        var logsContainer = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1,
            BackColor = _logsCard.BackColor,
            Padding = new Padding(10)
        };
        logsContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        logsContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _lblLogsTitle = new Label
        {
            Text = "Logs recientes",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI Semibold", 10.5f, FontStyle.Regular),
            ForeColor = Color.FromArgb(22, 57, 102),
            TextAlign = ContentAlignment.MiddleLeft
        };

        _gridLogs = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AutoGenerateColumns = true
        };
        ApplyGridTheme(_gridLogs);

        logsContainer.Controls.Add(_lblLogsTitle, 0, 0);
        logsContainer.Controls.Add(_gridLogs, 0, 1);
        _logsCard.Controls.Add(logsContainer);

        _shell.Controls.Add(_topCard, 0, 0);
        _shell.Controls.Add(_sourcesCard, 0, 1);
        _shell.Controls.Add(_logsCard, 0, 2);
        Controls.Add(_shell);

        ApplyTheme();

        Shown += async (_, _) =>
        {
            await RefreshApiControlStateAsync();
            await EnsureApiRunningOrReloadAsync();
        };
        FormClosing += (_, _) => StopManagedApiProcess();
    }

    private string GetBaseUrl() => _txtBaseUrl.Text.Trim().TrimEnd('/');

    private async Task EnsureApiRunningOrReloadAsync()
    {
        _btnApiControl.Enabled = false;
        try
        {
            if (await IsApiHealthyAsync())
            {
                _lblStatus.Text = "La API ya esta en ejecucion. Recargando estado y datos...";
                await RefreshDashboardAsync();
                return;
            }

            var root = FindSolutionRoot();
            if (root is null)
            {
                _lblStatus.Text = "No se encontro AsistenteW11.sln para iniciar la API automaticamente.";
                return;
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project \"src/AsistenteW11.Api\" --urls \"{GetBaseUrl()}\"",
                WorkingDirectory = root,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            _apiProcess = Process.Start(startInfo);
            if (_apiProcess is null)
            {
                _lblStatus.Text = "No se pudo iniciar el proceso de la API.";
                return;
            }

            _lblStatus.Text = "Iniciando API...";
            for (var i = 0; i < 20; i++)
            {
                await Task.Delay(500);
                if (await IsApiHealthyAsync())
                {
                    _lblStatus.Text = "API iniciada correctamente.";
                    await RefreshDashboardAsync();
                    return;
                }
            }

            _lblStatus.Text = "Se inicio el proceso pero la API no respondio a tiempo.";
            await CheckHealthAsync();
        }
        catch (Exception ex)
        {
            _lblStatus.Text = $"Error iniciando/recargando API: {ex.Message}";
        }
        finally
        {
            await RefreshApiControlStateAsync();
            _btnApiControl.Enabled = true;
        }
    }

    private async Task HandleApiControlClickAsync()
    {
        _btnApiControl.Enabled = false;
        try
        {
            var healthy = await IsApiHealthyAsync();
            if (healthy)
            {
                if (IsManagedApiRunning())
                {
                    StopManagedApiProcess();
                    _lblStatus.Text = "API detenida desde Admin UI.";
                    await CheckHealthAsync();
                    await RefreshApiControlStateAsync();
                    return;
                }

                _lblStatus.Text = "API externa detectada. Recargando estado y datos...";
                await RefreshDashboardAsync();
                await RefreshApiControlStateAsync();
                return;
            }

            await EnsureApiRunningOrReloadAsync();
        }
        catch (Exception ex)
        {
            _lblStatus.Text = $"Error en control de API: {ex.Message}";
        }
        finally
        {
            _btnApiControl.Enabled = true;
        }
    }

    private static string? FindSolutionRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var slnPath = Path.Combine(dir.FullName, "AsistenteW11.sln");
            if (File.Exists(slnPath))
            {
                return dir.FullName;
            }
            dir = dir.Parent;
        }

        return null;
    }

    private async Task RefreshDashboardAsync()
    {
        await CheckHealthAsync();
        await LoadLogsAsync();
    }

    private bool IsManagedApiRunning() => _apiProcess is { HasExited: false };

    private void StopManagedApiProcess()
    {
        try
        {
            if (!IsManagedApiRunning())
            {
                return;
            }

            _apiProcess!.Kill(entireProcessTree: true);
            _apiProcess.WaitForExit(2500);
        }
        catch
        {
            // Si no se puede detener, seguimos sin bloquear la UI.
        }
        finally
        {
            _apiProcess?.Dispose();
            _apiProcess = null;
        }
    }

    private async Task RefreshApiControlStateAsync()
    {
        var palette = _isDarkMode ? DarkPalette : LightPalette;
        var healthy = await IsApiHealthyAsync();
        if (!healthy)
        {
            SetApiButtonStyle("Iniciar API", palette.PrimaryButton, palette.PrimaryText);
            SetApiBadge("OFFLINE", Color.FromArgb(184, 31, 31));
            return;
        }

        if (IsManagedApiRunning())
        {
            SetApiButtonStyle("Detener API", Color.FromArgb(191, 38, 51), Color.White);
            SetApiBadge("ONLINE", Color.FromArgb(16, 122, 79));
            return;
        }

        SetApiButtonStyle("Recargar API", palette.SecondaryButton, palette.SecondaryText);
        SetApiBadge("ONLINE", Color.FromArgb(16, 122, 79));
    }

    private void SetApiButtonStyle(string text, Color backColor, Color foreColor)
    {
        _btnApiControl.Text = text;
        _btnApiControl.BackColor = backColor;
        _btnApiControl.ForeColor = foreColor;
    }

    private void SetApiBadge(string text, Color backColor)
    {
        _lblApiBadge.Text = text;
        _lblApiBadge.BackColor = backColor;
        _lblApiBadge.ForeColor = Color.White;
    }

    private void ApplyTheme()
    {
        var palette = _isDarkMode ? DarkPalette : LightPalette;
        BackColor = palette.AppBackground;
        _shell.BackColor = palette.AppBackground;
        ForeColor = palette.BodyText;

        ApplyCardTheme(_topCard, palette);
        ApplyCardTheme(_sourcesCard, palette);
        ApplyCardTheme(_logsCard, palette);
        ApplyThemeRecursive(_shell, palette);

        _chkDarkMode.ForeColor = palette.BodyText;
        _chkDarkMode.BackColor = Color.Transparent;
        _lblSourcesTitle.ForeColor = palette.TitleText;
        _lblLogsTitle.ForeColor = palette.TitleText;
        _lblStatus.ForeColor = palette.MutedText;

        ApplyInputTheme(_txtBaseUrl, palette);
        ApplyInputTheme(_txtLogPath, palette);
        ApplyInputTheme(_txtTestSource, palette);
        ApplyInputTheme(_txtTestMessage, palette);

        _cmbLevel.BackColor = palette.InputBackground;
        _cmbLevel.ForeColor = palette.BodyText;

        ApplyPrimaryStyle(_btnTestToast, palette);
        ApplySecondaryStyle(_btnHealth, palette);
        ApplySecondaryStyle(_btnLoadLogs, palette);

        ApplyGridTheme(_gridSources);
        ApplyGridTheme(_gridLogs);

        UpdateHealthLabelTone();
    }

    private void UpdateHealthLabelTone()
    {
        var palette = _isDarkMode ? DarkPalette : LightPalette;
        if (_lblHealth.Text.Contains("OK", StringComparison.OrdinalIgnoreCase))
        {
            _lblHealth.ForeColor = palette.Success;
            return;
        }

        if (_lblHealth.Text.Contains("Error", StringComparison.OrdinalIgnoreCase) ||
            _lblHealth.Text.Contains("sin conexion", StringComparison.OrdinalIgnoreCase))
        {
            _lblHealth.ForeColor = palette.Error;
            return;
        }

        _lblHealth.ForeColor = palette.Info;
    }

    private static void ApplyCardTheme(Panel card, ThemePalette palette)
    {
        card.BackColor = palette.CardBackground;
    }

    private static void ApplyInputTheme(TextBox textBox, ThemePalette palette)
    {
        textBox.BackColor = palette.InputBackground;
        textBox.ForeColor = palette.BodyText;
        textBox.BorderStyle = BorderStyle.FixedSingle;
    }

    private static void ApplyThemeRecursive(Control parent, ThemePalette palette)
    {
        foreach (Control child in parent.Controls)
        {
            switch (child)
            {
                case Label:
                    child.ForeColor = palette.BodyText;
                    break;
                case TableLayoutPanel:
                    child.BackColor = child.Parent?.BackColor ?? palette.CardBackground;
                    break;
                case ComboBox combo:
                    combo.BackColor = palette.InputBackground;
                    combo.ForeColor = palette.BodyText;
                    break;
            }

            if (child.HasChildren)
            {
                ApplyThemeRecursive(child, palette);
            }
        }
    }

    private static void ApplyPrimaryStyle(Button button, ThemePalette palette)
    {
        button.BackColor = palette.PrimaryButton;
        button.ForeColor = palette.PrimaryText;
        button.FlatAppearance.MouseOverBackColor = palette.PrimaryHover;
        button.FlatAppearance.MouseDownBackColor = palette.PrimaryDown;
        button.FlatAppearance.BorderSize = 0;
    }

    private static void ApplySecondaryStyle(Button button, ThemePalette palette)
    {
        button.BackColor = palette.SecondaryButton;
        button.ForeColor = palette.SecondaryText;
        button.FlatAppearance.BorderColor = palette.SecondaryBorder;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.MouseOverBackColor = palette.SecondaryHover;
        button.FlatAppearance.MouseDownBackColor = palette.SecondaryDown;
    }

    private void AttachFocusStyle(Control control)
    {
        control.GotFocus += (_, _) =>
        {
            if (control is Button button)
            {
                button.FlatAppearance.BorderColor = Color.FromArgb(255, 174, 0);
                button.FlatAppearance.BorderSize = Math.Max(1, button.FlatAppearance.BorderSize);
            }
            else
            {
                control.BackColor = Color.FromArgb(255, 251, 230);
            }
        };

        control.LostFocus += (_, _) =>
        {
            var palette = _isDarkMode ? DarkPalette : LightPalette;
            if (control is TextBox textBox)
            {
                textBox.BackColor = palette.InputBackground;
                return;
            }

            if (control is ComboBox comboBox)
            {
                comboBox.BackColor = palette.InputBackground;
                return;
            }

            if (control is Button button)
            {
                if (button == _btnApiControl || button == _btnTestToast)
                {
                    button.FlatAppearance.BorderSize = 0;
                }
                else
                {
                    button.FlatAppearance.BorderColor = palette.SecondaryBorder;
                    button.FlatAppearance.BorderSize = 1;
                }
            }
        };
    }

    private async Task<bool> IsApiHealthyAsync()
    {
        try
        {
            var baseUrl = GetBaseUrl();
            var response = await Http.GetAsync($"{baseUrl}/health");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private async Task CheckHealthAsync()
    {
        var palette = _isDarkMode ? DarkPalette : LightPalette;
        try
        {
            var baseUrl = GetBaseUrl();
            var response = await Http.GetAsync($"{baseUrl}/health");
            if (response.IsSuccessStatusCode)
            {
                _lblHealth.Text = $"Estado API: OK ({(int)response.StatusCode})";
                _lblHealth.ForeColor = palette.Success;
                _lblStatus.Text = "Health check correcto.";
            }
            else
            {
                _lblHealth.Text = $"Estado API: Error ({(int)response.StatusCode})";
                _lblHealth.ForeColor = palette.Error;
                _lblStatus.Text = $"Health check fallo con codigo {(int)response.StatusCode}.";
            }
        }
        catch (Exception ex)
        {
            _lblHealth.Text = "Estado API: sin conexion";
            _lblHealth.ForeColor = palette.Error;
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

    private static Panel CreateCard()
    {
        return new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(252, 254, 255),
            Margin = new Padding(0, 0, 0, 10),
            Padding = new Padding(0),
            BorderStyle = BorderStyle.FixedSingle
        };
    }

    private static Label CreateLabel(string text)
    {
        return new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = Color.FromArgb(36, 57, 84),
            Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Regular)
        };
    }

    private static TextBox CreateTextBox(string value)
    {
        return new TextBox
        {
            Dock = DockStyle.Fill,
            Text = value,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.White,
            ForeColor = Color.FromArgb(28, 40, 58)
        };
    }

    private static Button CreatePrimaryButton(string text)
    {
        var button = new Button
        {
            Text = text,
            Dock = DockStyle.Fill,
            Height = 34,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(26, 115, 232),
            ForeColor = Color.White
        };
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(34, 128, 248);
        button.FlatAppearance.MouseDownBackColor = Color.FromArgb(21, 97, 198);
        return button;
    }

    private static Button CreateSecondaryButton(string text)
    {
        var button = new Button
        {
            Text = text,
            Dock = DockStyle.Fill,
            Height = 34,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(236, 244, 255),
            ForeColor = Color.FromArgb(28, 75, 125)
        };
        button.FlatAppearance.BorderColor = Color.FromArgb(194, 214, 242);
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(223, 236, 254);
        button.FlatAppearance.MouseDownBackColor = Color.FromArgb(206, 226, 252);
        return button;
    }

    private void ApplyGridTheme(DataGridView grid)
    {
        var palette = _isDarkMode ? DarkPalette : LightPalette;
        grid.BackgroundColor = palette.GridBackground;
        grid.BorderStyle = BorderStyle.None;
        grid.GridColor = palette.GridLines;
        grid.RowHeadersVisible = false;
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersDefaultCellStyle.BackColor = palette.GridHeaderBackground;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = palette.GridHeaderText;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Regular);
        grid.DefaultCellStyle.SelectionBackColor = palette.GridSelectionBackground;
        grid.DefaultCellStyle.SelectionForeColor = palette.GridSelectionText;
        grid.DefaultCellStyle.BackColor = palette.GridBackground;
        grid.DefaultCellStyle.ForeColor = palette.BodyText;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.RowsDefaultCellStyle.BackColor = palette.GridBackground;
        grid.AlternatingRowsDefaultCellStyle.BackColor = palette.GridAltBackground;
    }

    private readonly record struct ThemePalette(
        Color AppBackground,
        Color CardBackground,
        Color BodyText,
        Color TitleText,
        Color MutedText,
        Color InputBackground,
        Color PrimaryButton,
        Color PrimaryText,
        Color PrimaryHover,
        Color PrimaryDown,
        Color SecondaryButton,
        Color SecondaryText,
        Color SecondaryBorder,
        Color SecondaryHover,
        Color SecondaryDown,
        Color GridBackground,
        Color GridAltBackground,
        Color GridLines,
        Color GridHeaderBackground,
        Color GridHeaderText,
        Color GridSelectionBackground,
        Color GridSelectionText,
        Color Success,
        Color Error,
        Color Info
    );

    private static ThemePalette LightPalette => new(
        AppBackground: Color.FromArgb(243, 248, 255),
        CardBackground: Color.FromArgb(252, 254, 255),
        BodyText: Color.FromArgb(30, 42, 60),
        TitleText: Color.FromArgb(22, 57, 102),
        MutedText: Color.FromArgb(66, 80, 104),
        InputBackground: Color.White,
        PrimaryButton: Color.FromArgb(26, 115, 232),
        PrimaryText: Color.White,
        PrimaryHover: Color.FromArgb(34, 128, 248),
        PrimaryDown: Color.FromArgb(21, 97, 198),
        SecondaryButton: Color.FromArgb(236, 244, 255),
        SecondaryText: Color.FromArgb(28, 75, 125),
        SecondaryBorder: Color.FromArgb(194, 214, 242),
        SecondaryHover: Color.FromArgb(223, 236, 254),
        SecondaryDown: Color.FromArgb(206, 226, 252),
        GridBackground: Color.White,
        GridAltBackground: Color.FromArgb(248, 251, 255),
        GridLines: Color.FromArgb(221, 233, 250),
        GridHeaderBackground: Color.FromArgb(236, 244, 255),
        GridHeaderText: Color.FromArgb(26, 58, 98),
        GridSelectionBackground: Color.FromArgb(221, 236, 255),
        GridSelectionText: Color.FromArgb(20, 39, 64),
        Success: Color.FromArgb(16, 122, 79),
        Error: Color.FromArgb(184, 31, 31),
        Info: Color.FromArgb(28, 75, 125)
    );

    private static ThemePalette DarkPalette => new(
        AppBackground: Color.FromArgb(17, 22, 31),
        CardBackground: Color.FromArgb(24, 31, 43),
        BodyText: Color.FromArgb(222, 230, 243),
        TitleText: Color.FromArgb(171, 209, 255),
        MutedText: Color.FromArgb(154, 171, 194),
        InputBackground: Color.FromArgb(31, 39, 52),
        PrimaryButton: Color.FromArgb(62, 139, 255),
        PrimaryText: Color.White,
        PrimaryHover: Color.FromArgb(80, 151, 255),
        PrimaryDown: Color.FromArgb(43, 120, 236),
        SecondaryButton: Color.FromArgb(35, 47, 66),
        SecondaryText: Color.FromArgb(181, 212, 255),
        SecondaryBorder: Color.FromArgb(63, 83, 110),
        SecondaryHover: Color.FromArgb(46, 61, 84),
        SecondaryDown: Color.FromArgb(31, 43, 62),
        GridBackground: Color.FromArgb(24, 31, 43),
        GridAltBackground: Color.FromArgb(29, 38, 52),
        GridLines: Color.FromArgb(54, 71, 95),
        GridHeaderBackground: Color.FromArgb(31, 39, 52),
        GridHeaderText: Color.FromArgb(181, 212, 255),
        GridSelectionBackground: Color.FromArgb(55, 89, 138),
        GridSelectionText: Color.FromArgb(240, 246, 255),
        Success: Color.FromArgb(68, 199, 138),
        Error: Color.FromArgb(240, 104, 104),
        Info: Color.FromArgb(127, 178, 247)
    );

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
