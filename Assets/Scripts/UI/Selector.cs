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
	Sprite[] personajes;                // El orden tiene que coincidir con la enum!
	[SyncVar] public PJs pj;			// El personaje selccionado
	int charId 
	{
		get { return ( int ) pj; }
		set { pj = ( PJs ) value; }
	}

	public Vector2 pos;
	public Image current;
	public Image next;
	public GameObject focus;            // Marca cual es nuestro personje

	bool sliding;
	Animator anim;
	RectTransform rect;
	#endregion

	#region SLIDING
	public void CorrectSprite () 
	{
		current.sprite = personajes[charId];
		sliding = false;
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
		next.sprite = personajes[id];
	}
	#endregion

	#region CALLBACKS
	private void Update() 
	{
		if (rect.anchoredPosition != pos) rect.anchoredPosition = pos;

		if (!hasAuthority || isServer) return;

		/// En caso de que se pulse tecla de mover
		/// ( y si no se está moviendo ya ),
		/// deslizar targeta de personaje.
		var dir = InputX.GetMovement ();
		if (!sliding && dir != 0)
		{
			sliding = true;
			Cmd_CorrectSlideID (( int ) dir);
			anim.SetTrigger ((dir == -1) ? "SlideLeft" : "SlideRight");
		}
	}

	public override void OnStartAuthority () 
	{
		base.OnStartAuthority ();
		anim = GetComponent<Animator> ();

		if (isClient) focus.SetActive (true);
	}
	private void Start () 
	{
		rect = GetComponent<RectTransform> ();
		personajes = GameObject.Find ("Canvas").GetComponent<UIManager> ().personajes;
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
