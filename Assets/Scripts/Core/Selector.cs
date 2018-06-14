using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Selector : NetBehaviour 
{
	#region DATA
	[Header ("References")]
	public RectTransform carroussel;
	public Sprite goldenFrame;
	public Image frame;
	public Image anchor;
	[Space]

	// This values indicates the literal value in the carrousel
	[SyncVar] [Range (0f, 5f)]
	public int selection;

	private bool canMove;
	private Vector3 iPosition;
	private static int sceneIdFix;

	// Reference values
	private const float Offset = 387f;
	#endregion

	#region UTILS
	private void ReadInput () 
	{
		if (!canMove) return;
		int delta = (int) Input.GetAxisRaw ("Horizontal");
		if (delta != 0) 
		{
			Cmd_MoveSelection (delta);
			canMove = false;
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

			// Snap carroussel to opposite bounds
			SnapCarroussel (selection / 5f);
			Rpc_Snap (selection / 5f);
		}
		UpdateHero ();
	}

	private void MoveCarroussel () 
	{
		var pos = carroussel.localPosition;
		float tValue = Mathf.Lerp 
		(
			pos.x,
			Mathf.Lerp (Offset, -Offset * 4f, selection/5f),
			Time.deltaTime * 7f
		);
		// Check if carroussel is near enough for next movement
		canMove = (Mathf.Abs (pos.x - tValue) < 1f);

		pos.x = tValue;
		carroussel.localPosition = pos;
	}

	[ClientRpc]
	private void Rpc_Snap (float factor) 
	{ SnapCarroussel (factor); }
	private void SnapCarroussel (float factor) 
	{
		// Use a value [0, 1] to positionate the Carroussel
		float value = Mathf.Lerp (-Offset, Offset * 4f, factor);

		var pos = carroussel.localPosition;
		pos.x = -value;
		carroussel.localPosition = pos;
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
		SnapCarroussel (selection / 5f);
		UpdateHero ();
	}
	#endregion

	#region CALLBACKS
	private void Update () 
	{
		MoveCarroussel ();
		if (hasAuthority && isClient) ReadInput ();
	}

	private void Start () 
	{
		// Correct position && SceneID
		(transform as RectTransform).localPosition = iPosition;
		id.ForceSceneId (sceneIdFix++);

		// Show owner marks
		if (hasAuthority && isClient) 
		{
			frame.sprite = goldenFrame;
			anchor.gameObject.SetActive (true);
		}
	}

	protected override void OnAwake () 
	{
		// Cache position because it'll move when connected to server
		iPosition = (transform as RectTransform).localPosition;
	}
	#endregion
}
