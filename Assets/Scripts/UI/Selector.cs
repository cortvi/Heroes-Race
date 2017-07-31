using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

/// Rota el selector de personaje
/// y permite seleccionar uno
public class Selector : NetworkBehaviour
{
	#region REFERENCES
	[SyncVar]
	public PJs pj;						// El personaje selccionado
	int charId 
	{
		get { return ( int ) pj; }
		set { pj = ( PJs ) value; }
	}
	Sprite[] personajes;                // El orden tiene que coincidir con la enum!

	public Vector2 pos;
	public Image current;
	public Image next;
	public GameObject focus;            // Marca de cual es nuestro personje
	public GameObject selected;

	[SyncVar] bool done;				// Personaje seleccionado?
	[SyncVar] bool sliding;				// Animacion en marcha?
	Animator anim;
	RectTransform rect;
	#endregion

	#region SELECTING CHAR
	static int playersDone;
	[Command]
	void Cmd_Select ( bool done ) 
	{
		playersDone += done ? +1 : -1;

		selected.SetActive (done);
		this.done = done;
		Rpc_Select (done);
	}
	[ClientRpc]
	void Rpc_Select ( bool done ) 
	{
		selected.SetActive (done);
	}
	#endregion

	#region SLIDING
	public void CorrectSprite () 
	{
		current.sprite = personajes[charId];
		if (isServer) sliding = false;
	}

	[Command]
	void Cmd_CorrectSlideID ( int dir ) 
	{
		var max = personajes.Length;

		charId += dir;
		if (charId == -1) charId = max-1;
		else
		if (charId == max) charId = 0;

		next.sprite = personajes[charId];
		Rpc_CorrectSlideID (charId);
	}
	[ClientRpc]
	void Rpc_CorrectSlideID ( int id ) 
	{
		sliding = true;
		next.sprite = personajes[id];
	}
	#endregion

	#region CALLBACKS
	private void Update() 
	{
		if (rect.anchoredPosition != pos) rect.anchoredPosition = pos;
		if (!hasAuthority || isServer) return;

		/// En caso de que se pulse tecla de mover
		/// deslizar targeta de personaje.
		var dir = InputX.GetMovement ();
		if (!sliding)
		{
			/// Animacion
			if (dir != 0 && !done)
			{
				Cmd_CorrectSlideID (( int ) dir);
				anim.SetTrigger ((dir == -1) ? "SlideLeft" : "SlideRight"); 
			}

			/// Seleccionar personaje
			if ( InputX.GetKeyDown ( PlayerActions.GreenBtn ) )
			{
				if (!done)
				{
					done = true;
					Cmd_Select ( true );
				}
				else
				{
					selected.SetActive (false);
					Cmd_Select (false);
				}
			}
		}
	}

	public override void OnStartAuthority () 
	{
		base.OnStartAuthority ();
		if (isClient) focus.SetActive (true);
	}
	private void Start () 
	{
		personajes = GameObject.Find ("Canvas").GetComponent<UIManager> ().personajes;
		rect = GetComponent<RectTransform> ();
		anim = GetComponent<Animator> ();
		current.sprite = personajes[charId];
	}
	#endregion
}

public enum PJs 
{
	/// Lista de todos los personajes
	/// que se pueden seleccionar
	NONE,   // => Espectador
	Gatete,
	Sir,
	Random_0,
	Random_1
	// En un futuro cuidado con
	// cambiar los nombres! => El orden tiene que coincidir con la array!
}
