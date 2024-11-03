📡 Color Shooter
Un juego desarrollado en Unity donde controlas una nave espacial y debes defenderte de enemigos de diferentes colores.

📖 Descripción
En este juego, tomas el control de una nave espacial triangular que puede disparar proyectiles de distintos colores. Los enemigos se acercan a ti y deben ser destruidos con proyectiles del color correspondiente. Si el color del proyectil coincide con el del enemigo, este será destruido y una explosión del mismo color aparecerá en su lugar.

🚀 Características
Control de Nave: La nave sigue el puntero del ratón y gira para apuntar en esa dirección.

Selección de Color:
Mantén presionada una de las teclas W, A, S, D para seleccionar el color del proyectil y de la nave.
W: Amarillo
A: Azul
S: Verde
D: Rojo
Si no se presiona ninguna tecla, la nave es de color blanco y no puede disparar.

Disparo:
Haz clic con el botón izquierdo del ratón para disparar un proyectil del color seleccionado.
La nave realiza un efecto de escalado al disparar para proporcionar feedback visual.

Enemigos:
Se mueven hacia el jugador y rotan aleatoriamente.
Deben ser destruidos con un proyectil del color correspondiente.
Al ser destruidos, generan una explosión de partículas del mismo color.

Efectos Visuales:
Efecto de escalado en la nave al disparar.
Explosiones personalizadas al destruir enemigos, ajustadas en tamaño y duración.
Feedback al Jugador:
La nave cambia de color para reflejar el color seleccionado.
No se puede disparar si no se está presionando una tecla de color.

🎮 Controles
Movimiento y Apuntado: La nave sigue el puntero del ratón.
Selección de Color:
W: Selecciona el color Amarillo.
A: Selecciona el color Azul.
S: Selecciona el color Verde.
D: Selecciona el color Rojo.
Disparo: Clic izquierdo del ratón (solo si se está manteniendo presionada una tecla de color).

🖥️ Requisitos del Sistema
Unity: Versión 2021.3 LTS o superior.
Plataformas Soportadas: Windows, macOS, Linux.

📁 Estructura del Proyecto
Assets/Scripts:
PlayerController.cs: Controla el comportamiento de la nave del jugador.
Enemy.cs: Maneja el comportamiento de los enemigos.
Projectile.cs: Controla los proyectiles disparados por el jugador.
AutoDestroyParticle.cs: Destruye automáticamente los efectos de partículas una vez finalizados.
Assets/Prefabs:
Player: Prefab de la nave del jugador.
Enemy: Prefab de los enemigos.
Projectile: Prefab de los proyectiles.
ExplosionEffect: Prefab del efecto de explosión personalizado.
Assets/Materials:
Materiales utilizados para los efectos visuales y las partículas.
Assets/Scenes:
MainScene.unity: Escena principal del juego.

📌 Próximos Pasos
Implementar Sistema de Puntuación: Añadir una interfaz que muestre la puntuación del jugador y registre los enemigos destruidos.
Mejorar la IA de los Enemigos: Introducir diferentes tipos de enemigos con comportamientos variados.
Añadir Efectos Sonoros: Incorporar sonidos al disparar y al destruir enemigos para mejorar la inmersión.
Optimizar el Rendimiento: Implementar un sistema de pooling para proyectiles y explosiones.
Desarrollar Niveles: Crear diferentes niveles o aumentar la dificultad progresivamente.

🤝 Contribución
¡Las contribuciones son bienvenidas! Si deseas colaborar:

Haz un fork del repositorio.
Crea una rama para tu funcionalidad: git checkout -b mi-nueva-funcionalidad.
Realiza tus cambios y haz commits descriptivos.
Envía tus cambios al repositorio remoto: git push origin mi-nueva-funcionalidad.
Abre un Pull Request explicando tus cambios.

📄 Licencia
Este proyecto está bajo la Licencia MIT. Consulta el archivo LICENSE para más detalles.

💡 Créditos
Desarrollado por Oscar L.
