param(
    [string]$ProjectsRoot = "C:\Users\maxli\OneDrive\Documentos\Projects",
    [string]$OutputFile = ""
)

if ([string]::IsNullOrWhiteSpace($OutputFile)) {
    $stamp = Get-Date -Format "yyyy-MM-dd_HHmmss"
    $OutputFile = Join-Path $PSScriptRoot ("repo-status-" + $stamp + ".json")
}

$rows = @()
$dirs = Get-ChildItem -Path $ProjectsRoot -Directory -ErrorAction Stop

foreach ($d in $dirs) {
    git -C $d.FullName rev-parse --is-inside-work-tree 1>$null 2>$null
    if ($LASTEXITCODE -ne 0) { continue }

    $origin = git -C $d.FullName remote get-url origin 2>$null
    if (-not $origin) { continue }

    $branch = git -C $d.FullName rev-parse --abbrev-ref HEAD 2>$null
    $local = git -C $d.FullName rev-parse --short HEAD 2>$null
    $dirty = ((git -C $d.FullName status --porcelain 2>$null) | Measure-Object -Line).Lines

    git -C $d.FullName fetch origin --prune --quiet 2>$null
    $upstream = git -C $d.FullName rev-parse --abbrev-ref --symbolic-full-name '@{upstream}' 2>$null

    $ahead = ""
    $behind = ""
    if ($upstream) {
        $counts = git -C $d.FullName rev-list --left-right --count ("HEAD..." + $upstream) 2>$null
        if ($counts) {
            $parts = $counts -split '\s+'
            if ($parts.Length -ge 2) {
                $ahead = $parts[0]
                $behind = $parts[1]
            }
        }
    }

    $rows += [pscustomobject]@{
        name = $d.Name
        path = $d.FullName
        branch = $branch
        local = $local
        upstream = $upstream
        ahead = $ahead
        behind = $behind
        dirty = $dirty
        origin = $origin
    }
}

$rows | Sort-Object name | ConvertTo-Json -Depth 3 | Out-File -FilePath $OutputFile -Encoding utf8
Write-Host "Exported: $OutputFile"
