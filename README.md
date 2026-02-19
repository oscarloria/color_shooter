# Luminity 🎮
### Shooter 2D de Colores

> Última actualización: Febrero 2026

---

## Descripción General

Luminity es un juego de disparos 2D con vista cenital (top-down) para PC desarrollado en Unity (usando URP 2D). El juego fusiona la acción intensa de un shooter con mecánicas de puzzle basadas en la correspondencia de colores y elementos de bullet hell/shmup. El jugador pilota una nave capaz de cambiar de color y debe utilizar el color correcto para destruir a los enemigos energéticos que le acechan.

| | |
|---|---|
| **Género** | Shooter 2D Top-Down / Puzzle / Shmup |
| **Motor** | Unity (Universal Render Pipeline 2D) |
| **Plataforma** | PC (con potencial para expandirse) |
| **Estado** | Funcionalidad principal implementada: ciclo de juego, múltiples armas, enemigos variados, sistema de jefes, puntuación, mejoras persistentes y animación avanzada |

---

## 🎯 Principios de Diseño

**Posicionamiento = Fairness** — El jugador siempre está centrado con tiempo de reacción equitativo desde todos los bordes. Si el juego mueve al jugador (ej: boss fights), el spawning se adapta para mantener la equidad.

**Color = Ofensivo Solamente** — El color del jugador nunca lo protege. Cualquier proyectil o enemigo que toque al jugador causa daño, sin importar el color seleccionado. La única defensa es disparar y destruir amenazas activamente.

**Ricochet como Identidad** — El rebote de proyectiles en mismatch de color es una mecánica central que se mantiene consistente en todos los enemigos y jefes del juego.

---

## 🕹️ Mecánicas Principales

**Combate por Colores** — La nave y los enemigos tienen uno de cuatro colores (Amarillo, Azul, Verde, Rojo). Solo los proyectiles del mismo color pueden dañar a un enemigo.

**Cambio de Color** — El jugador cambia el color activo usando WASD. El estado neutral (sin tecla presionada) es blanco y no permite disparar armas principales.

**Ricochet** — Los proyectiles que impactan un enemigo o proyectil enemigo de color diferente rebotan manteniendo su energía cinética. Solo el color correcto destruye; todo lo demás rebota.

**Armas** — Pistola (inicio), Escopeta (spread), Rifle Automático (continuo) y Orbes Defensivos. Cada arma tiene munición, recarga y comportamiento propio.

**Apuntado** — Manual (mouse) o automático (autoAim hacia el enemigo más cercano).

**Animación** — Sistema de sprites en 8 direcciones basado en estados, con animaciones diferentes según arma equipada (Pistola, Escopeta, Rifle, Orbes) y estado (Idle/Attack).

---

## 👾 Enemigos

### Enemigos Normales

| Tipo | Comportamiento | HP |
|---|---|---|
| **Enemy** (Normal) | Avanza directamente hacia el jugador | 1 |
| **TankEnemy** | Resistente, punto débil trasero | Múltiple |
| **ShooterEnemy** | Dispara proyectiles, esquiva, carga en modo kamikaze | Variable |
| **EnemyZZ** | Patrón zigzag hacia el jugador | 1 |
| **CometEnemy** | Fly-by rápido, orbita 360° al jugador dejando 3 proyectiles dormidos que hacen homing lento tras 1.5s | 1 |

Todos los enemigos spawean desde su cuadrante de color (Superior=Amarillo, Derecho=Rojo, Inferior=Verde, Izquierdo=Azul) con indicador off-screen previo a su entrada.

### Jefes (Boss)

**RouletteEnemy** — Jefe orbital con 4 cañones de colores que cambian por fase. 3 fases con velocidad y agresividad creciente.

**Zuma Boss** — Jefe inspirado en Zuma. Una serpiente de orbes de colores recorre un camino en espiral hacia el jugador. Si la cabeza toca al jugador, es Game Over inmediato. 3 fases:

| Fase | Colores | Orbes | Velocidad |
|---|---|---|---|
| 1 | Rojo, Azul | 20 | Lenta |
| 2 | Verde, Amarillo | 30 | Media |
| 3 | Los 4 colores | 40 | Rápida |

Destruir orbes retrocede la cadena. Al eliminar todos, la cabeza queda vulnerable y alterna colores cada 3 segundos. Ricochet completo en orbes y cabeza.

### Arquitectura

Todos los enemigos normales extienden `EnemyBase`, que centraliza: color, velocidad, vida, muerte, registro en `EnemyManager`, colisiones, explosión y carga de SlowMotion. Nuevos enemigos se agregan con código mínimo.

---

## ⚙️ Sistemas de Soporte

**Lumi-Coins y Mejoras** — Los enemigos sueltan monedas usadas en el menú de pausa para comprar mejoras persistentes de armas.

**Puntuación** — Sistema de score con High Score persistente vía `PlayerPrefs`.

**Salud y Daño** — Vidas con invulnerabilidad temporal y explosión AoE al recibir daño.

**EnemySpawner** — Oleadas con dificultad incremental y eventos especiales (RapidWave, EliteWave, SingleColorWave, FormationWave).

**Efectos** — Cámara Lenta, Zoom, Vibración de Cámara, Indicadores off-screen, Línea de mira.

**UI** — HUD, Menú de Pausa con mejoras, Game Over, Menú Principal, Opciones, Selección de Slot.

---

## 📁 Estructura del Proyecto
```
Assets/
├── Scripts/          # Código C# — toda la lógica del juego
├── Prefabs/          # Enemigos, Proyectiles, Orbes, Efectos, Boss
├── Scenes/           # MainMenu, SlotSelection, SampleScene, GameOver, Options, Scoreboard, Credits
├── Material/         # Materiales de renderizado y física 2D
├── Sprites/          # Imágenes organizadas en subcarpetas
├── Settings/         # Configuración URP 2D
└── TextMesh Pro/     # Recursos TMP
```

---

## 🎮 Controles

| Acción | Control |
|---|---|
| Apuntar | Mouse (manual) / Automático |
| Seleccionar Color | `W` Amarillo · `A` Azul · `S` Verde · `D` Rojo |
| Disparar | Clic izquierdo |
| Cambiar Arma | Rueda del mouse o `1` `2` `3` `4` |
| Recargar | `R` |
| Zoom | Clic derecho (toggle) |
| Cámara Lenta | `Espacio` (consume carga) |
| Pausa | `ESC` |

---

## 🚀 Setup

1. Clonar o descargar el repositorio
2. Abrir el proyecto con Unity Hub (versión compatible con URP 2D)
3. Abrir la escena `MainMenuScene` desde `Assets/Scenes/`
4. Presionar Play

---

## 🔮 Mejoras Futuras

- Sistema completo de Slots de Guardado
- Más tipos de mejoras y armas
- Bosses adicionales (Hydra, Eclipse, Nexus en consideración)
- Modo Campaña/Historia
- Pixel art final para sprites
- Mecánica de mover al jugador fuera del centro en boss fights especiales

---

## 👤 Créditos

**Desarrollador:** Oscar Loria
