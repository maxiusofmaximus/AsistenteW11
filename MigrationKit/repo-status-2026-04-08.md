# Repo Status - 2026-04-08

Estado verificado manualmente con `git fetch --prune` y comparación `HEAD...origin/<branch>`.

## Repos sincronizados (0 ahead / 0 behind)

- `AsistenteW11` -> `origin/main`
- `AsistHub` -> `origin/main`
- `Components` -> `origin/master`
- `Change_the_World` -> `origin/main` (remote: `trono-2415-maximus-2026`)
- `Claudia` -> `origin/main`
- `DEV-OS` -> `origin/master`
- `DevSecurityGuard` -> `origin/main`
- `Lista de Tareas de Programación` -> `origin/main` (remote: `notejob`)

## Repos con atención pendiente

- `ProtoAscend`
  - Rama principal sincronizada con `origin/main` (0/0).
  - Hay cambio local dentro de `game/stable-diffusion-webui` (`webui-user.bat` modificado y archivos nuevos).
  - Además, el gitlink existe sin `.gitmodules`.
  - Acción recomendada en Linux: decidir si ese directorio será submódulo formal o carpeta normal versionada.

## Acciones realizadas en esta sesión

- Push de cambios locales pendientes en:
  - `DEV-OS` (`.gitignore` para artefactos de semgrep).
  - `Lista de Tareas de Programación` (`wip` para no perder trabajo).
- Confirmación de estado remoto/local para:
  - `AsistenteW11`, `AsistHub`, `Components`, `Change_the_World`, `Claudia`, `DevSecurityGuard`, `DEV-OS`, `ProtoAscend`.
- Generación de backups Git bundles en `MigrationKit/bundles/`.
