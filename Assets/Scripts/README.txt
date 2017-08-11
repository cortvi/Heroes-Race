/// Además de esta breve introducción a los scripts recomiendo leer bien el códgio y los
/// comentarios de cada uno independientemente.

/// Así mismo, recomiendo que cuando creeis vuestros scripts lo comenteis en profundidad
/// y añadais su descripción correspondiente.

/// De no entender el funcionamiento de algo, whatsapp, skype o GitHub!
/// Recordad usar las Issues de GitHub :)

Core/
		Game.cs
			---

		Player.cs
			Esta clase contendrá todas las funcionalidades de los personajes
			de cada jugador.

		Networker.cs
			NetworkManager personalizado. Gestiona algunas funciones internas
			de la red y las conexiones tanto de servidor como de cliente.


UI/
		UI.cs
			---

		Selector.cs
			Gestiona la lógica del selector de personaje.


Props/
		Waterfall.cs
			Controla el efecto del agua del fondo.
	

Utilities/
		Inputx.cs
			Aquí están centralizados los controles del jugador,
			de manera que es más sencillo cambiar los controles en un
			momento dado. También hay una serie de funciones, que equivalen a las
			de la clase Input de Unity ( GetKey, GetKeyDown y GetKeyUp ),
			que se usan igual que las de la clase Input de Unity, pero
			estas aceptan PlayerActions como parámetro en vez de un KeyCode.
			Por comodidad.

		Extensions.cs
			--- ignorar ---