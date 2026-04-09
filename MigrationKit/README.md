# Migration Kit (Windows -> Linux)

Este kit deja todo listo para migrar tus proyectos a CachyOS o PikaOS sin perder trabajo.

## Incluye

- `repo-status-2026-04-08.md`: estado repo por repo (local vs GitHub).
- `export-repo-status.ps1`: genera inventario actualizado de estado de repos.
- `clone-repos-linux.sh`: script para clonar rápido en Linux.
- `bundles/`: backups `.bundle` de Git (ignorados por este repo).

## Flujo recomendado antes de desinstalar Windows

1. Ejecutar `export-repo-status.ps1` para tener foto final.
2. Verificar que cada repo clave esté `ahead 0 / behind 0`.
3. Verificar que los `.bundle` existan en `bundles/`.
4. Copiar `MigrationKit/` a nube/disco externo.
5. Confirmar acceso a GitHub (`gh auth status`) desde Linux tras instalar.

## Restauración en Linux

1. Instalar `git`, `gh`, `dotnet`, `node`, `python` según proyecto.
2. Ejecutar `clone-repos-linux.sh`.
3. Entrar a cada repo y correr instalación de dependencias.
4. Usar los `.bundle` como fallback si algún remoto no responde.

## Nota importante

`ProtoAscend` tiene un subrepositorio gitlink (`game/stable-diffusion-webui`) sin `.gitmodules`.
Esto no bloquea la migración, pero conviene normalizarlo después en Linux.
