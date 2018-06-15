using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Selector : NetBehaviour 
{
	#region DATA
	[Header ("References")]
	public RectTransform carousel;
	public Sprite goldenFrame;
	public Image frame;
	public Image anchor;
	[Space]

	// This values indicates the literal value in the carrousel
	[SyncVar] [Range (0f, 5f)]
	public int selection;

	private bool canMove;
	private Vector3 iPosition;

	// Reference values
	private const float Offset = 387f;
	#endregion

	#region UTILS
	IEnumerator ReadInput () 
	{
		while (true) 
		{
			int delta = (int) Input.GetAxisRaw ("Horizontal");
			if (delta != 0) 
			{
				Cmd_MoveSelection (delta);
				// Avoid anuse of movement
				yield return new WaitForSeconds (0.5f); 
			}
			yield return null;
		}
	}
	[Command] private void Cmd_MoveSelection (int delta) 
	{
		selection += delta;
		if (selection < 0 || selection > 5)
		{
			// Correct selection
			if (selection == -1) selection = 4;
			else
			if (selection == +6) selection = 1;

			// Snap carousel to opposite bounds
			SnapCarousel (selection / 5f);
			Rpc_Snap (selection / 5f);
		}
		UpdateHero ();
	}

	[ClientRpc]
	private void Rpc_Snap (float factor) 
	{ SnapCarousel (factor); }
	private void SnapCarousel (float factor) 
	{
		// Use a value [0, 1] to positionate the carousel
		float value = Mathf.Lerp (-Offset, Offset * 4f, factor);

		var pos = carousel.localPosition;
		pos.x = -value;
		carousel.localPosition = pos;
	}
	#endregion

	#region HELPERS
	private void UpdateHero () 
	{
//		if (selection == 0) selectedHero = Game.Heroes.Harry;
//		else
//		if (selection == 5) selectedHero = Game.Heroes.Espectador;
//		else selectedHero = (Game.Heroes)(selection - 1);
	}

	[ContextMenu ("Snap to selected")]
	public void UpdateSelector () 
	{
		SnapCarousel (selection / 5f);
		UpdateHero ();
	}
	#endregion

	#region CALLBACKS
	private void Update () 
	{
		// Move carousel towards selection
		var pos = carousel.localPosition;
		float tValue = Mathf.Lerp
		(
			pos.x,
			Mathf.Lerp (Offset, -Offset * 4f, selection / 5f),
			Time.deltaTime * 7f
		);
		pos.x = tValue;
		carousel.localPosition = pos;
	}

	private void Start () 
	{
		// Correct position && SceneID
		(transform as RectTransform).localPosition = iPosition;

		if (hasAuthority && isClient) 
		{
			// Show owner marks
			frame.sprite = goldenFrame;
			anchor.gameObject.SetActive (true);
			// Start reading movement input
			StartCoroutine (ReadInput ());
		}
	}

	protected override void OnAwake () 
	{
		// Cache position because it'll move when connected to server
		iPosition = (transform as RectTransform).localPosition;
	}
	#endregion
}
