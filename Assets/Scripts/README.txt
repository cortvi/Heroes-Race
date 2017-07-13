/// Además de esta breve introducción a los scripts recomiendo leer bien el códgio y los
/// comentarios de cada uno independientemente.

/// Así mismo, recomiendo que cuando creeis vuestros scripts lo comenteis en profundidad
/// y añadais su descripción correspondiente.

/// De no entender el funcionamiento de algo, whatsapp, skype o GitHub!
/// Recordad usar las Issues de GitHub :)
---

Core/
		Game.cs
			Esta clase contendrá de forma centralizada referencias a
			diferentes elementes del juego, para facilitar el acceso. Así como
			funcionalidades básicas de funcionamiento.

		Player.cs
			Esta clase contendrá todas las funcionalidades de los personajes
			de cada jugador.

		Networker.cs
			Gestiona funcionalidades de red tanto para el cliente como para el servidor.


UI/
		UIManager.cs
			Gestiona las funciones que se llaman desde el UI, cambios de pantalla, sincronizacion
			servidor-cliente y cualquier cosa que tenga que ver con menus, HUD etc.


Utilities/
		Inputx.cs
			Aquí están centralizados los controles del jugador,
			de manera que es más sencillo cambiar los controles en un
			momento dado. También hay una serie de funciones, que equivalen a las
			de la clase Input de Unity ( GetKey, GetKeyDown y GetKeyUp ),
			que se usan igual que las de la clase Input de Unity, pero
			estas aceptan PlayerActions como parámetro en vez de un KeyCode. Por comodidad.