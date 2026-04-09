# Game Migration Backup

Respaldo local de guardados y configuraciones para migrar de Windows a Linux sin perder progreso.

## Que incluye

- Inventario detectado de juegos instalados en Steam y Epic.
- Copia de saves y configs de los juegos encontrados.
- Verificacion de rutas de Wuthering Waves.

## Estructura

- `data/`: respaldos por juego/plataforma.
- `inventory/backup-manifest.json`: resumen de rutas copiadas, cantidad de archivos y tamano.
- `inventory/steam-games.json`: inventario Steam.
- `inventory/epic-games.json`: inventario Epic.
- `inventory/wuthering-check.json`: chequeo rapido de rutas de Wuthering Waves.

## Rutas Linux recomendadas

- Steam Proton user data: `~/.steam/steam/userdata/<steamid>/<appid>/`
- Steam juegos instalados: `~/.steam/steam/steamapps/common/`
- Lutris/Bottles/Wine prefijos:
  - `~/.local/share/lutris/`
  - `~/.var/app/com.usebottles.bottles/data/bottles/`
  - `~/.wine/drive_c/users/<user>/`

## Sugerencia de restauracion

1. Instala cada juego en Linux y ejecutalo una vez.
2. Cierra el juego.
3. Copia el respaldo del juego desde `data/` a la ruta equivalente del prefijo Proton/Wine.
4. Reabre el juego y valida saves/config.

## Nota

No se incluyen binarios completos de instalacion de juegos. Solo datos de progreso y configuraciones.
