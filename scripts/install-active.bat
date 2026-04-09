@echo off
setlocal EnableExtensions

set "BOT_ENDPOINT=%~1"
if "%BOT_ENDPOINT%"=="" set "BOT_ENDPOINT=http://127.0.0.1:5055/logs"

set "SCRIPT_DIR=C:\ProgramData\Scripts"
set "ACTIVE_PS1=%SCRIPT_DIR%\Active.ps1"
set "STARTUP_ACTIVE=%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup\Active.ps1"

echo [1/4] Creando carpeta de scripts...
if not exist "%SCRIPT_DIR%" mkdir "%SCRIPT_DIR%"

echo [2/4] Generando Active.ps1 con logger local + envio HTTP...
powershell.exe -NoProfile -ExecutionPolicy Bypass -Command ^
  "$endpoint = '%BOT_ENDPOINT%';" ^
  "$scriptPath = 'C:\ProgramData\Scripts\Active.ps1';" ^
  "$content = @'
param(
    [string]$Endpoint = ''__ENDPOINT__''
)

Set-StrictMode -Version Latest
$ErrorActionPreference = ''Stop''

$LogDir = Join-Path $env:ProgramData ''Scripts\Logs''
$LogFile = Join-Path $LogDir ''Active.log''
if (-not (Test-Path $LogDir)) {
    New-Item -Path $LogDir -ItemType Directory -Force | Out-Null
}

function Write-ActiveLog {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Message,
        [ValidateSet(''Trace'', ''Debug'', ''Info'', ''Warn'', ''Error'', ''Fatal'')]
        [string]$Level = ''Info'',
        [string]$Source = ''Active.ps1'',
        [object]$Context = $null
    )

    $entry = [ordered]@{
        timestamp = (Get-Date).ToString(''o'')
        level = $Level
        source = $Source
        message = $Message
        context = $Context
    }

    ($entry | ConvertTo-Json -Depth 8 -Compress) | Add-Content -Path $LogFile -Encoding UTF8

    try {
        $payload = [ordered]@{
            message = $Message
            level = $Level
            source = $Source
            context = $Context
        }

        Invoke-RestMethod -Uri $Endpoint -Method Post -ContentType ''application/json'' -Body ($payload | ConvertTo-Json -Depth 8) -TimeoutSec 4 | Out-Null
    }
    catch {
        $fallback = [ordered]@{
            timestamp = (Get-Date).ToString(''o'')
            level = ''Warn''
            source = ''Active.ps1''
            message = ''No se pudo enviar log al bot local.''
            context = @{
                endpoint = $Endpoint
                error = $_.Exception.Message
            }
        }
        ($fallback | ConvertTo-Json -Depth 8 -Compress) | Add-Content -Path $LogFile -Encoding UTF8
    }
}

Write-ActiveLog -Message ''ActiveElevated iniciado en logon.'' -Level ''Info'' -Source ''Active.ps1'' -Context @{
    user = $env:USERNAME
    machine = $env:COMPUTERNAME
    endpoint = $Endpoint
}
'@;" ^
  "$content = $content.Replace('__ENDPOINT__', $endpoint);" ^
  "Set-Content -Path $scriptPath -Value $content -Encoding UTF8"

if errorlevel 1 (
  echo ERROR: No se pudo crear Active.ps1
  exit /b 1
)

echo [3/4] Eliminando script de Startup si existe...
if exist "%STARTUP_ACTIVE%" del /f /q "%STARTUP_ACTIVE%"

echo [4/4] Registrando/actualizando tarea programada ActiveElevated...
powershell.exe -NoProfile -ExecutionPolicy Bypass -Command ^
  "$taskName = 'ActiveElevated';" ^
  "$scriptPath = 'C:\ProgramData\Scripts\Active.ps1';" ^
  "$endpoint = '%BOT_ENDPOINT%';" ^
  "if (Get-ScheduledTask -TaskName $taskName -ErrorAction SilentlyContinue) { Unregister-ScheduledTask -TaskName $taskName -Confirm:$false };" ^
  "$arg = '-NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden -File \"' + $scriptPath + '\" -Endpoint \"' + $endpoint + '\"';" ^
  "$action = New-ScheduledTaskAction -Execute 'powershell.exe' -Argument $arg;" ^
  "$trigger = New-ScheduledTaskTrigger -AtLogOn;" ^
  "$principal = New-ScheduledTaskPrincipal -UserId ($env:USERDOMAIN + '\' + $env:USERNAME) -LogonType Interactive -RunLevel Highest;" ^
  "$settings = New-ScheduledTaskSettingsSet -Hidden -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries;" ^
  "Register-ScheduledTask -TaskName $taskName -Action $action -Trigger $trigger -Principal $principal -Settings $settings -Force | Out-Null;"

if errorlevel 1 (
  echo ERROR: No se pudo registrar la tarea ActiveElevated. Ejecuta este .bat como Administrador.
  exit /b 1
)

echo Instalacion completada.
echo Endpoint configurado: %BOT_ENDPOINT%
endlocal
