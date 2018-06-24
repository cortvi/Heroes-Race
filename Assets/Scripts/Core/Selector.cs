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
		private const float MaxSelection = 5f;
		#endregion

		#region UTILS
		[Client] IEnumerator ReadInput () 
		{
			while (true)
			{
				// Let player select the character
				if (Input.GetKeyDown (KeyCode.Return)) 
				{
					// Query an status update &
					// stop interaction until it's answered
					Cmd_SetReady (!anim.GetBool ("Ready"));
					yield break;
				}

				// Move selector
				int delta = (int)Input.GetAxisRaw ("Horizontal");
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
			int selection = anim.IncrementInt ("Selection", delta);
			// Correct selection & snap carousel to opposite bounds
			if (selection < 0 || selection > 5) 
			{
				if (selection == -1) anim.SetInt ("Selection", 3);
				else
				if (selection == 6) anim.SetInt ("Selection", 2);
			}
		}
		#endregion

		#region NETWORK COMMUNICATION
		[Command] private void Cmd_SetReady (bool state) 
		{
			SelectorsReady += (state? +1 : -1);
			#warning Need to change this to 3 for final version / LAN testing
			if (SelectorsReady == 2) 
			{
				Target_SetReady (id.clientAuthorityOwner, true, true);
				// Start loading actual game scene!

				// Test that hero is read correctly
				print (ReadHero ());
			}

			// Just allow ready-state update on Client animator
			else Target_SetReady (id.clientAuthorityOwner, state, false);
		}
		[TargetRpc] private void Target_SetReady (NetworkConnection target, bool state, bool block) 
		{
			anim.SetBool ("Ready", state);
			// Start reading movement input again
			if (!block) StartCoroutine (ReadInput ());
		}

		[Command] private void Cmd_EnableAnimator () 
		{
			anim.Animator.enabled = true;
			Rpc_EnableAnimator ();
		}
		[ClientRpc] private void Rpc_EnableAnimator () 
		{
			anim.Animator.enabled = true;
		}
		#endregion

		#region CALLBACKS
		protected override void OnStart () 
		{
			// Correct position && SceneID
			(transform as RectTransform).localPosition = cachePosition;

			// Show owner marks
			if (hasAuthority && isClient)
			{
				frame.sprite = goldenFrame;
				anchor.gameObject.SetActive (true);
				anim.SetInt ("Selection", initialSelection);
				Cmd_EnableAnimator ();

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
		private Game.Heroes ReadHero () 
		{
			Game.Heroes hero;

			// Translate selection into a Hero
			int selection = anim.GetInt ("Selection");
			if (selection == 0) hero = Game.Heroes.Harry;
			else
			if (selection == 5) hero = Game.Heroes.Espectador;
			else hero = (Game.Heroes)(selection - 1);

			return hero;
		}
		#endregion
	}
}
