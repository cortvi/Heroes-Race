using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

namespace HeroesRace 
{
	public class Selector : NetBehaviour
	{
		#region DATA
		public override string SharedName 
		{
			get { return "Selector"; }
		}

		[Header ("References")]
		public Sprite goldenFrame;
		public Image frame;
		public Image anchor;
		[Space]

		[Tooltip ("Indiana is 2")]
		// This values indicates the literal value in the carrousel
		[Range (0f, 5f)] public int initialSelection;

		private SmartAnimator anim;
		private Vector3 cachePosition;

		private static int SelectorsReady;
		private const float Offset = 387f;
		private const float MaxSelection = 5f;
		#endregion

		#region UTILS
		[Client] IEnumerator ReadInput () 
		{
			while (true)
			{
				#region SELECTOR READY
				if (Input.GetKeyDown (KeyCode.Return)) 
				{
					// Let player select the character
					anim.SetBool ("Ready", true);
					while (anim.GetBool ("Ready")) 
					{

						// Block movement until it's been de-selected
						if (Input.GetKeyDown (KeyCode.Return))
						{
							anim.SetBool ("Ready", false);
						}
					}
				}
				#endregion

				#region SELECTOR MOVEMENT
				int delta = (int)Input.GetAxisRaw ("Horizontal");
				if (delta != 0)
				{
					MoveSelection (delta);
					// Avoid abuse of movement
					yield return new WaitForSeconds (0.3f);
				}
				yield return null; 
				#endregion
			}
		}

		private void MoveSelection (int delta) 
		{
			int selection = anim.IncrementInt ("Selection", delta);
			// Correct selection & snap carousel to opposite bounds
			if (selection < 0 || selection > 5) 
			{
				if (selection == -1) 
				{
					SnapCarousel (4);
					anim.SetInt ("Selection", 3);
				}
				else
				if (selection == +6) 
				{
					SnapCarousel (1);
					anim.SetInt ("Selection", 2);
				}
			}
			UpdateHero ();
		}
		#endregion

		#region CALLBACKS
		private void Update () 
		{
			// Move carousel towards selection
			int selection = anim.GetInt ("Selection");
			float iValue = anim.GetFloat ("Blend");
			float tValue = Mathf.Lerp (iValue, selection / MaxSelection, Time.deltaTime * 7f);
			anim.SetFloat ("Blend", tValue);
		}

		protected override void OnStart () 
		{
			// Correct position && SceneID
			(transform as RectTransform).localPosition = cachePosition;

			SnapCarousel (initialSelection);
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
			cachePosition = (transform as RectTransform).localPosition;
			anim = GetComponent<Animator> ().GoSmart (networked: true);
		}
		#endregion

		#region HELPERS
		private void SnapCarousel (int selection) 
		{
			float factor = selection / MaxSelection;
			anim.SetInt ("Selection", selection);
			anim.SetFloat ("Blend", factor);
		}

		private void UpdateHero () 
		{
			//		if (selection == 0) selectedHero = Game.Heroes.Harry;
			//		else
			//		if (selection == 5) selectedHero = Game.Heroes.Espectador;
			//		else selectedHero = (Game.Heroes)(selection - 1);
		}
		#endregion
	}
}
