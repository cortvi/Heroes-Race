/// Adem�s de esta breve introducci�n a los scripts recomiendo leer bien el c�dgio y los
/// comentarios de cada uno independientemente.

/// As� mismo, recomiendo que cuando creeis vuestros scripts lo comenteis en profundidad
/// y a�adais su descripci�n correspondiente.

/// De no entender el funcionamiento de algo, whatsapp, skype o GitHub!
/// Recordad usar las Issues de GitHub :)

Core/
		Game.cs
			Gestiona l�gica b�sica de cada jugador, sobre todo input
			relacionado con el UI. Cada jugador tiene uno propio.

		Player.cs
			Esta clase contendr� todas las funcionalidades de los 
			objetos-personajes de cada jugador.

		Networker.cs
			NetworkManager personalizado. Gestiona algunas funciones internas
			de la red y las conexiones tanto de servidor como de cliente.

		Cam.cs
			Gestiona la c�mara que sigue al personaje.
			< A�n por hacer... >


UI/
		UI.cs
			Contiene referencias y datos sobre el UI. Indica tanto a clientes
			como al servidor en que pantalla se encuentra el juego.

		Selector.cs
			Gestiona la l�gica del selector de personaje.


Props/
		Waterfall.cs
			Controla el efecto del agua del fondo.
	

Utilities/
		Inputx.cs
			Aqu� est�n centralizados los controles del jugador,
			de manera que es m�s sencillo cambiar los controles en un
			momento dado. Tambi�n hay una serie de funciones, que equivalen a las
			de la clase Input de Unity ( GetKey, GetKeyDown y GetKeyUp ),
			que se usan igual que las de la clase Input de Unity, pero
			estas aceptan PlayerActions o DevActions como par�metro en vez de un KeyCode.
			Por comodidad.

		PrefabSpawner.cs
			Sustituye el objeto en el que se coloque este script por un prefab. Sirve para
			colocar, al iniciar el juego, prefabs dentro/sobre de otros prefabs.
			< A�n por testear en red... >

		Extensions.cs
			--- ignorar por ahora ---