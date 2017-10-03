using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Selector : NetworkBehaviour
{
	#region INTERNAL DATA
	[SyncVar]
	public PJs pj;                      // El personaje selccionado en este selector (esta variable se sincroniza por red)
	int charId							// Forma fácil de modificar la variable 'pj' 
	{
		get { return ( int ) pj; }
		set { pj = ( PJs ) value; }
	}
	Sprite[] personajes;				// Los splasharts de los diferentes personajes
	//-> El orden tiene que
	//   coincidir con la enum!
	static bool[] takenPJs;

	public Vector2 pos;					// Posicion fijada del selector en pantalla
	public Image current;				// Splashart del personaje mostrado
	public Image next;					// Splashart del siguiente personaje a mostrar

	public GameObject focus;            // Marca de cual es nuestro selector
	public GameObject selected;         // Indicador de seleccion lista

	[SyncVar] bool done;				// Seleccion lista?
	[SyncVar] bool sliding;				// Animacion en marcha?

	RectTransform rect;
	Animator anim;
	#endregion

	#region SELECTING CHAR
	static int playersDone;             // Cantidad de jugadores listos
	[Command]
	void Cmd_Select( bool done ) 
	{
		// Mostrar/Ocultar indicador de seleccion hecha (servidor)
		selected.SetActive (done);
		// Bloquear sliding
		this.done = done;
		// Mostrar/Ocultar indicador de seleccion hecha (cliente)
		Rpc_Select (done);

		// Sumar/Restar a la suma de jugadores listos
		playersDone += done ? +1 : -1;
		if (playersDone == 3)
		{
			/// Cambiar en todas las recreativas a esta de TODOS LISTOS
			UI.manager.currentScreen = UI.Pantallas.TodosListos;
			NetworkManager.singleton.ServerChangeScene ("WaterTower");
		}

		/// Marcar personajes como (de)seleccionado
		if (pj!=PJs.NONE) takenPJs[charId] = done;
	}
	[ClientRpc]
	void Rpc_Select( bool done ) 
	{
		// Mostrar/Ocultar indicador de seleccion hecha (cliente)
		selected.SetActive (done);
	}
	#endregion

	#region SLIDING
	public void CorrectSprite () 
	{
		/// Esto se ejecuta desde el Animator
		/// para intercambiar los splasharts
		/// se note el cambio
		current.sprite = personajes[charId];
		if (isServer) sliding = false;
	}

	[Command]
	/// Corrige el cambio de splashart
	void Cmd_CorrectSlideID( int dir ) 
	{
		/// Bloquea el sliding (porque ya se esta ejecutando)
		sliding = true;
		var max = personajes.Length;

		/// Cambia los splasharts en base al movimiento
		/// Asegurarse de no marcar personajes ya elegidos
		do charId += dir;
		while (!takenPJs[charId]);
		/// Asegura el loop
		if (charId == -1) charId = max-1;
		else
		if (charId == max) charId = 0;

		/// Cambiar el splashart siguiente (servidor)
		next.sprite = personajes[charId];
		/// Cambiar el splashart siguiente (cliente)
		Rpc_CorrectSlideID (charId);
	}
	[ClientRpc]
	void Rpc_CorrectSlideID( int id ) 
	{
		/// Cambiar el splashart siguiente (cliente)
		next.sprite = personajes[id];
	}
	#endregion

	#region CALLBACKS
	private void Update() 
	{
		/// Se asegura que el selector esté en la posición correcta
		if (rect.anchoredPosition != pos) rect.anchoredPosition = pos;
		if (!hasAuthority || isServer) return;

		/// En caso de que se pulse tecla de mover
		/// deslizar targeta de personaje.
		var dir = InputX.GetMovement ();
		if (!sliding)
		{
			/// Animacion de cambio de splashart
			/// (sliding)
			if (dir != 0 && !done)
			{
				sliding = true;
				Cmd_CorrectSlideID (( int ) dir);
				anim.SetTrigger ((dir == -1) ? "SlideLeft" : "SlideRight");
			}

			/// Seleccionar personaje
			if (InputX.GetKeyDown (PlayerActions.GreenBtn))
			{
				if (!done)
				{
					// auto avoid player from sliding
					done = true;
					Cmd_Select (true);
				}
				else
				{
					// only can slide again when server provides
					selected.SetActive (false);
					Cmd_Select (false);
				}
			}
		}
	}

	public override void OnStartAuthority() 
	{
		base.OnStartAuthority ();
		/// Mostrar marcador de cual es nuestro selector
		if (isClient) focus.SetActive (true);
	}
	private void Start() 
	{
		/// Referencias internas
		personajes = UI.manager.personajes;
		rect = GetComponent<RectTransform> ();
		anim = GetComponent<Animator> ();
		current.sprite = personajes[charId];
		takenPJs = new bool[4];
	} 
	#endregion
}

public enum PJs 
{
	/// Lista de todos los personajes
	/// que se pueden seleccionar
	Indiana,
	Harley,
	Harry,
	NONE   // => Espectador
}