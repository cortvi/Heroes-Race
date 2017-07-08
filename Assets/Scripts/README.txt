/// Además de esta breve introducción a los scripts recomiendo leer bien el códgio y los
/// comentarios de cada uno independientemente. Así mismo, recomiendo que cuando creeis
/// vuestros scripts respeteis los espacios de nombres, lo comenteis en profundidad
/// y añadais su descripción correspondiente a este archivo.
/// De no entender el funcionamiento de algo, whatsapp, skype o GitHub!
/// Recordad usar las Issues de GitHub :)

=> Core:
	Game.cs
		Esta clase contendrá de forma centralizada referencias a
		diferentes elementes del juego, para facilitar el acceso. Así como
		funcionalidades básicas de funcionamiento.

	Player.cs
		Esta clase contendrá todas las funcionalidades de los "Players"
	
=> Utilities:
	DevHotKeys.cs
		Se encarga simplemente de simplemente leer
		atajos de teclado con utilidades para nosotros
		( ej: activar conexiones de red, etc ).

	UI.cs
		Dentro de este script deben ponerse todas las funciones
		que se usen desde el UI, HUD, etc...
		( ej: Salir, Nueva Partida, pausa, etc ).

	Inputx.cs
		Aquí están centralizados los controles del jugador,
		de manera que es más sencillo cambiar los controles en un
		momento dado. También hay una serie de funciones ( GetKey, GetKeyDown,
		GetKeyUp ) que se usan igual que las de la clase Input de Unity, pero
		estas aceptan PlayerActions como parámetro en vez de un KeyCode. Por comodidad.