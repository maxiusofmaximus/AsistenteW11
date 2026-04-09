# AsistenteW11

Asistente local para Windows 11 en C#/.NET que recibe logs por HTTP, los guarda en archivo y lanza notificaciones toast.

## Requisitos

- Windows 11
- .NET SDK 9.x
- PowerShell 5.1 o superior
- Permisos de administrador para registrar tarea con `RunLevel Highest`

## Estructura

- `src/AsistenteW11.Api`: API local en ASP.NET Core Minimal API
- `src/AsistenteW11.Admin`: interfaz gráfica WinForms para administrar el bot
- `scripts/install-active.bat`: instalador de `Active.ps1` + tarea programada

## Ejecutar API local

1. Restaurar y compilar:

```bash
dotnet restore
dotnet build AsistenteW11.sln
```

2. Ejecutar API:

```bash
dotnet run --project src/AsistenteW11.Api
```

### Ejecutar interfaz gráfica (Admin)

En otra terminal:

```bash
dotnet run --project src/AsistenteW11.Admin
```

Funciones principales de la UI:

- Verificar estado de la API (`/health`)
- Enviar log de prueba para disparar toast (`/logs`)
- Ver logs recientes desde archivo local
- Ver softwares/fuentes detectadas (agrupadas por `source`)

3. Verificar salud:

```bash
curl http://127.0.0.1:5055/health
```

## Endpoints

### GET `/health`

Responde estado del servicio.

### POST `/logs`

Payload JSON esperado:

```json
{
  "message": "Texto del evento",
  "level": "Info",
  "source": "Active.ps1",
  "context": {
    "k": "v"
  }
}
```

Reglas:

- `message` obligatorio, max 1024
- `level` obligatorio, max 32
- `source` obligatorio, max 128
- `context` opcional

Ejemplo:

```bash
curl -X POST "http://127.0.0.1:5055/logs" ^
  -H "Content-Type: application/json" ^
  -d "{\"message\":\"Prueba\",\"level\":\"Info\",\"source\":\"manual\"}"
```

## Ubicación de logs

- API local: `C:\ProgramData\AsistenteW11\logs\assistant.log`
- Script Active: `C:\ProgramData\Scripts\Logs\Active.log`

## Instalación de Active.ps1 + tarea programada

Ejecuta PowerShell o CMD como **Administrador** y corre:

```bat
scripts\install-active.bat
```

Con endpoint personalizado:

```bat
scripts\install-active.bat http://127.0.0.1:5055/logs
```

El instalador realiza:

1. Crea `C:\ProgramData\Scripts\Active.ps1`
2. Elimina `%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup\Active.ps1` si existe
3. Registra/actualiza la tarea `ActiveElevated` (AtLogOn, Hidden, RunLevel Highest, PowerShell Hidden)

## Troubleshooting

- La tarea no se registra:
  - Ejecuta `scripts\install-active.bat` como Administrador.
  - Verifica en Programador de tareas que exista `ActiveElevated`.
- No llegan logs por HTTP:
  - Confirma que la API esté en `http://127.0.0.1:5055`.
  - Prueba manual con `curl` a `/logs`.
- No aparecen toasts:
  - Revisa que Windows permita notificaciones.
  - Revisa errores en salida de la API y en `assistant.log`.
  - Mantén `ToastAppId` en `appsettings.json` como `Microsoft.Windows.Explorer` (o ajusta según política del equipo).
- UI no muestra logs o fuentes:
  - Verifica en la UI la ruta de log (`C:\ProgramData\AsistenteW11\logs\assistant.log` por defecto).
  - Pulsa `Recargar Logs` tras enviar pruebas.
- Error de permisos sobre `C:\ProgramData`:
  - Ejecuta API/instalador con permisos suficientes o cambia `Bot:LogFilePath` en `appsettings.json`.

## GitHub (inicialización y push)

Si aún no existe repo Git inicializado:

```bash
git init
git add .
git commit -m "feat: asistente local W11 con API C# logs y toast"
```

Conecta remoto y sube:

```bash
git branch -M main
git remote add origin https://github.com/maxiusofmaximus/AsistenteW11.git
git push -u origin main
```
