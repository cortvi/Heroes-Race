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
		[Header ("References")]
		public Sprite goldenFrame;
		public Image frame;
		public Image anchor;
		[Space]

		[Range (0f, 5f)]
        [Tooltip ("Indiana is 2")]
        public int initialSelection;

		private SmartAnimator anim;
		private Vector3 cachePosition;

		private static int SelectorsReady;
		private const float MaxSelection = 5f;
		#endregion

		#region NETWORK COMMUNICATION
		[Command]
		private void Cmd_SetReady (bool state) 
		{
			SelectorsReady += (state? +1 : -1);
			if (SelectorsReady == Net.UsersNeeded) 
			{
				Target_SetReady (id.clientAuthorityOwner, state: true, block: true);
				GoToTower ();
            }
			// Just allow ready-state update on Client animator
			else Target_SetReady (id.clientAuthorityOwner, state, block: false);
		}
		[TargetRpc]
		private void Target_SetReady (NetworkConnection target, bool state, bool block) 
		{
			anim.SetBool ("Ready", state);
			// Start reading movement input again
			if (!block) StartCoroutine (ReadInput ());
		}

		[Command]
		private void Cmd_EnableAnimator () 
		{
			anim.Animator.enabled = true;
			Rpc_EnableAnimator ();
		}
		[ClientRpc]
		private void Rpc_EnableAnimator () 
		{
			anim.Animator.enabled = true;
		}
		#endregion

		#region CALLBACKS
		[Client]
		IEnumerator ReadInput () 
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
				if (delta != 0 && !anim.GetBool ("Ready"))
				{
					MoveSelection (delta);
					// Avoid abuse of movement
					yield return new WaitForSeconds (0.3f);
				}
				yield return null;
			}
		}

		/*
		[ClientCallback]
		protected override void OnAuthoritySet () 
		{
			// Show owner marks
			frame.sprite = goldenFrame;
			anchor.gameObject.SetActive (true);

			// Start animator
			anim.SetInt ("Selection", initialSelection);
			Cmd_EnableAnimator ();

			// Start reading movement input
			StartCoroutine (ReadInput ());
		}

		protected override void OnStart () 
		{
			// Correct position
			(transform as RectTransform).localPosition = cachePosition;
		}

		protected override void OnAwake () 
		{
			// Cache position because it'll move when connected to server
			cachePosition = (transform as RectTransform).localPosition;
			anim = GetComponent<Animator> ().GoSmart (networked: true);
		}
		*/
		#endregion

		#region HELPERS
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

		public Heroes ReadHero () 
		{
			Heroes hero;

			// Translate selection into a Hero
			int selection = anim.GetInt ("Selection");
			if (selection == 0) hero = Heroes.Harry;
			else
			if (selection == 5) hero = Heroes.Espectador;
			else hero = (Heroes)(selection - 1);

			return hero;
		} 

        [Server]
		#warning in the future, make the scene change smoother!
        public static void GoToTower () 
        {
			// Read all the selected heroes
			for (int i = 0; i != Net.UsersNeeded; i++)
            {
                var selector = FindObjectsOfType<Selector> ()[i];
                Net.users[i].playingAs = selector.ReadHero ();
            }
            // Change scene
            Net.worker.ServerChangeScene ("Tower");
        }
        #endregion
    }
}
