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

	// This values overrides Animator selection
	[Range (0f, 5f)] public int _selection;
	private int Selection 
	{
		get { return anim.GetInt ("Selection"); }
		set { anim.SetInt ("Selection", value); }
	}

	private SmartAnimator anim;
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
				MoveSelection (delta);
				// Avoid abuse of movement
				yield return new WaitForSeconds (0.3f);
			}
			yield return null;
		}
	}
	private void MoveSelection (int delta) 
	{
		Selection += delta;
		if (Selection < 0 || Selection > 5)
		{
			// Correct selection
			if (Selection == -1) 
			{
				Selection = 3;
				// Snap carousel to opposite bounds
				anim.SetFloat ("Blend", 4f / 5f);
			}
			else
			if (Selection == +6) 
			{
				Selection = 2;
				// Snap carousel to opposite bounds
				anim.SetFloat ("Blend", 1f / 5f);
			}
		}
		UpdateHero ();
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
	#endregion

	#region CALLBACKS
	private void Update () 
	{
		// Move carousel towards selection
		float iPos = anim.GetFloat ("Blend");
		float tPos = Mathf.Lerp (iPos, Selection / 5f, Time.deltaTime * 5f);
		anim.SetFloat ("Blend", tPos);
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
		// Prepare animator
		anim = GetComponent<Animator> ().GoSmart ();
		anim.SetFloat ("Blend", _selection / 5f);
		anim.SetFloat ("Selection", _selection);
	}
	#endregion
}
