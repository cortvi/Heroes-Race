/// Adem�s de esta breve introducci�n a los scripts recomiendo leer bien el c�dgio y los
/// comentarios de cada uno independientemente.

/// As� mismo, recomiendo que cuando creeis vuestros scripts lo comenteis en profundidad
/// y a�adais su descripci�n correspondiente.

/// De no entender el funcionamiento de algo, whatsapp, skype o GitHub!
/// Recordad usar las Issues de GitHub :)
---

Core/
		Game.cs
			Esta clase contendr� de forma centralizada referencias a
			diferentes elementes del juego, para facilitar el acceso. As� como
			funcionalidades b�sicas de funcionamiento.

		Player.cs
			Esta clase contendr� todas las funcionalidades de los personajes
			de cada jugador.

		Networker.cs
			Gestiona funcionalidades de red tanto para el cliente como para el servidor.


Props/
		Waterfall.cs
			Controla el efecto del agua del fondo.
	

Utilities/
		DevHotKeys.cs
			Se encarga simplemente de simplemente leer
			atajos de teclado con utilidades para nosotros
			( ej: activar conexiones de red, etc ).

		UIManager.cs
			Dentro de este script deben ponerse todas las funciones
			que se usen desde el UI, HUD, etc...
			( ej: Salir, Nueva Partida, pausa, etc ).

		Inputx.cs
			Aqu� est�n centralizados los controles del jugador,
			de manera que es m�s sencillo cambiar los controles en un
			momento dado. Tambi�n hay una serie de funciones ( GetKey, GetKeyDown,
			GetKeyUp ) que se usan igual que las de la clase Input de Unity, pero
			estas aceptan PlayerActions como par�metro en vez de un KeyCode. Por comodidad.

		ShowDisconnected.cs
			Los objetos que llevan este script solo aparecen visibles cuando
			NO estamos conectados al servidor.