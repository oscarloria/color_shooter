# Luminity (Shooter 2D de Colores)

*Última actualización basada en código del Lunes 14 de Abril, 2025.*

## Descripción General

Luminity es un juego de disparos 2D con vista cenital (top-down) para PC desarrollado en Unity (usando URP 2D). El juego fusiona la acción intensa de un shooter con mecánicas de puzzle basadas en la correspondencia de colores y elementos de bullet hell/shmup. El jugador pilota una nave capaz de cambiar de color y debe utilizar el color correcto para destruir a los enemigos energéticos que le acechan.

* **Género:** Shooter 2D Top-Down / Puzzle / Shmup
* **Motor:** Unity (versión no especificada, usando Universal Render Pipeline 2D)
* **Plataforma:** PC (con potencial para expandirse a otras)
* **Estado Actual:** Funcionalidad principal implementada, incluyendo ciclo de juego, múltiples armas, enemigos, sistema de puntuación, mejoras persistentes y sistema de animación avanzado para el jugador.

## Mecánicas Principales

* **Combate por Colores:** La nave y los enemigos tienen uno de cuatro colores (Amarillo, Azul, Verde, Rojo). Solo los proyectiles del mismo color que un enemigo pueden dañarlo.
* **Cambio de Color:** El jugador cambia el color activo de la nave (y sus proyectiles/habilidades) usando las teclas `WASD`. El estado neutral (sin tecla presionada) es blanco y no permite disparar armas principales.
* **Armas:** Incluye Pistola (inicio), Escopeta (spread), Rifle Automático (continuo) y Orbes Defensivos. Cada arma tiene su propio sistema de munición, recarga y script de manejo (`PlayerShooting`, `ShotgunShooting`, etc.).
* **Sistema de Proyectiles:** Utiliza prefabs específicos para cada color de proyectil (`ProjectileRedPrefab`, `ProjectileBluePrefab`, etc.), instanciados por los scripts de disparo correspondientes. Los proyectiles tienen un script `Projectile.cs` que almacena su color lógico para las colisiones.
* **Apuntado:** Soporta apuntado manual (la nave rota hacia el cursor del mouse) y apuntado automático (`autoAim` configurable) que rota la nave hacia el enemigo más cercano en pantalla.
* **Movimiento:** La nave actualmente rota sobre su eje en el centro (o cerca del centro) de la pantalla. El script `PlayerMovement.cs` controla esta rotación.
* **Animación del Jugador:** Sistema avanzado de sprites en 8 direcciones basado en estados. Utiliza scripts separados (`ShipBody[Weapon][State]8Directions.cs`) para mostrar diferentes animaciones según el arma equipada (Pistola, Escopeta, Rifle, Orbes) y el estado (Idle o Attack). Estos scripts leen la rotación del objeto padre (controlado por `PlayerMovement`) y muestran el sprite direccional correcto sin rotar el objeto hijo que contiene el `SpriteRenderer`. `PlayerController` gestiona la activación de los scripts Idle, y los scripts de disparo activan/desactivan los scripts de Attack correspondientes.
* **Lumi-Coins y Mejoras:** Los enemigos pueden soltar "Lumi-Coins" (gestionadas por `CoinManager` y guardadas en `PlayerPrefs`). Estas monedas se usan en el menú de pausa (`PauseMenuUpgrades`) para comprar mejoras persistentes para las armas (ej: tamaño del cargador y tiempo de recarga de la pistola, usando `PlayerPrefs`).
* **Puntuación:** Sistema de puntuación (`ScoreManager`) que registra el puntaje actual y mantiene un High Score persistente usando `PlayerPrefs`.
* **Salud y Daño:** El jugador tiene vidas (`PlayerHealth`, `LifeUI`). Recibir daño activa un periodo de invulnerabilidad y una explosión que destruye enemigos cercanos. Morir lleva a la pantalla de Game Over.
* **Enemigos:** Varios tipos (`Enemy`, `EnemyZZ`, `TankEnemy`, `ShooterEnemy`) con diferentes comportamientos (seguir, zigzag, tanque con punto débil, disparo + esquive + kamikaze). Son generados por `EnemySpawner` (complejo, con oleadas, eventos y dificultad incremental) o `EnemySpawnerSimple` (simple, cantidades fijas). Los enemigos también tienen colores y solo son vulnerables al color correspondiente.
* **Efectos Especiales:** Cámara Lenta (`SlowMotion`), Zoom de Cámara (`CameraZoom`), Vibración/Retroceso de Cámara (`CameraShake`), Indicadores de enemigos fuera de pantalla (`EnemyOffScreenIndicator`), línea de mira (`AimLineController`).
* **UI:** HUD principal (Score, Ammo, Vidas, Barra SlowMo, Indicadores Recarga), Menú de Pausa (con mejoras), Pantalla Game Over, Menú Principal, Opciones (resetear progreso), Selección de Slot.

## Estructura del Proyecto (Carpetas Clave en `Assets/`)

* **`Scripts/`:** Contiene todo el código C# que define la lógica del juego.
* **`Prefabs/`:** Contiene GameObjects preconfigurados (Enemigos, Proyectiles por color, Orbes, Efectos, Items, Indicadores UI).
    * **`projectile/`:** Subcarpeta con los prefabs de proyectiles específicos por color. (Nota: contiene subcarpetas y duplicados que podrían requerir organización).
* **`Scenes/`:** Todas las escenas del juego (MainMenu, SlotSelection, SampleScene (Juego Principal), GameOver, Options, Scoreboard, Credits).
* **`Material/`:** Materiales usados para renderizar objetos (fondo, partículas, etc.) y materiales de física 2D.
* **`Sprites/`:** Archivos de imagen usados en el juego (organizados en subcarpetas).
* **`Settings/`:** Archivos de configuración de Unity, especialmente para URP 2D.
* **`TextMesh Pro/`:** Recursos estándar del paquete TextMesh Pro para UI avanzada.

## Cómo Jugar (Controles Básicos)

* **Apuntar:** Mover el mouse (modo manual) / Automático (si `autoAim` está activo).
* **Seleccionar Color:** Teclas `W` (Amarillo), `A` (Azul), `S` (Verde), `D` (Rojo). Mantener presionada la tecla del color deseado.
* **Disparar:** Clic izquierdo del mouse.
* **Cambiar Arma:** Rueda del mouse o teclas `1` (Pistola), `2` (Escopeta), `3` (Rifle), `4` (Orbes).
* **Recargar:** Tecla `R`.
* **Zoom:** Clic derecho del mouse (toggle).
* **Cámara Lenta:** Barra espaciadora (consume carga).
* **Pausa:** Tecla `ESC`.

## Setup

1.  Clonar o descargar el repositorio.
2.  Abrir el proyecto usando Unity Hub (se recomienda una versión de Unity compatible con URP 2D, ej: 2021.3 LTS o posterior - verificar `ProjectVersion.txt`).
3.  Abrir la escena `MainMenuScene` desde `Assets/Scenes/`.
4.  Presionar Play.

## Posibles Mejoras Futuras / Puntos a Revisar

* Implementar sistema completo de Slots de Guardado.
* Añadir más tipos de mejoras y armas.
* Refinar/Expandir tipos de enemigos y jefes.
* Considerar un modo Campaña/Historia.
* Revisar la precisión de `AimLineController` (basado en `transform.up` vs. dirección de apuntado real).
* Reorganizar la carpeta `Assets/Prefabs/projectile/`.
* Revisar `OptionsController` si la escena de Opciones es independiente (posible error al buscar `PlayerShooting`).

## Créditos

* **Desarrollador:** Oscar Loria
