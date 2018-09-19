using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

namespace HeroesRace 
{
	public partial class /* COMMON */ Selector : NetBehaviour 
	{
		#region DATA
		// ——— Inspector data ———
		[Header ("References")]
		public Sprite goldenFrame;
		public Image frame;
		public Image anchor;
		[Space]

		[Range (0f, 5f)]
		[Tooltip ("Indiana is 2")]
		public int initialSelection;

		// ——— Helpers ———
		private SmartAnimator anim;
		private const float SelectionMax = ((int)Heroes.Count + 1f);
		private Vector3 cachePosition;
		#endregion

		#region CALLBACKS
		private void Update () 
		{
			if (isLocalPlayer) 
			{
				float input = Input.GetAxis ("Horizontal");
				if (input != 0f) Cmd_Input (input>0f? +1 : -1);

				// Set READY using any-key except axis
//				if (Input.GetAxis ("Vertical") == 0f
//				&& Input.anyKeyDown) Cmd_SwitchReady ();
			}

			// Blend the animator space for all instances
			float blend = anim.GetFloat ("Blend");
			float target = anim.GetInt ("Selection") / SelectionMax;
			float lerp = Mathf.Lerp (blend, target, Time.deltaTime * 5f);
			anim.SetFloat ("Blend", lerp);

			if (isServer) closeEnough = Mathf.Abs(target - blend) <= 0.05f;
		}

		protected override void OnAwake () 
		{
			if (NetworkServer.active && heroesLocked == null) 
			{
				// This dictionary will tell if a Hero is already picked
				heroesLocked = new Dictionary<Heroes, bool> 
				{
					{ Heroes.Indiana, false },
					{ Heroes.Harley, false },
					{ Heroes.Harry, false }
				};
			}
			anim = GetComponent<Animator> ().GoSmart (networked: true);

			// Cache position because it'll move when connected to Server
			cachePosition = (transform as RectTransform).localPosition;
		}

		protected override void OnStart () 
		{
			// Recover original position in case it's been moved by the Server
			(transform as RectTransform).localPosition = cachePosition;
		}
		#endregion
	}

	public partial class /* SERVER */ Selector 
	{
		#region DATA
		private static int SelectorsReady;
		private static Dictionary<Heroes, bool> heroesLocked;
		private bool closeEnough;
		#endregion

		#region SELECTOR MOTION
		[Command]
		private void Cmd_Input (int delta) 
		{
			// Won't move selection if locked OR already moving
			if (anim.GetBool("Ready") || !closeEnough) 
				return;

			int selection = anim.IncrementInt ("Selection", delta);
			// Correct carrousel if out of bounds
			if (selection == -1)
			{
				anim.SetInt ("Selection", (int)SelectionMax - 2);
				anim.SetFloat ("Blend", (SelectionMax - 1f) / SelectionMax);
			}
			else
			if (selection == 6)
			{
				anim.SetInt ("Selection", 2);
				anim.SetFloat ("Blend", 1f / SelectionMax);
			}
			
			// Animator is owned by the client, so send the new Selection
			Rpc_Move (selection);
		}

		[Command]
		private void Cmd_SwitchReady () 
		{
			// Can't switch ready if moving carrousel
			if (anim.Animator.IsInTransition (0))
				return;

			// Flip the ready state
			bool ready = anim.GetBool ("Ready");
			ready = !ready;
			anim.SetBool ("Ready", !ready);
			SelectorsReady += (ready? +1 : -1);


			if (SelectorsReady == Net.UsersNeeded) 
				GoToTower ();
		}
		#endregion

		#region HELPERS
		public Heroes ReadHero () 
		{
			// Translate selection into a Hero
			int selection = anim.GetInt ("Selection");
			if (selection == 0) return Heroes.Harry;
			else
			if (selection == 5) return Heroes.Espectador;
			else return (Heroes)(selection - 1);
		} 

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
			Log.Debug ("Heroes saved, now going to Tower map!");
        }
        #endregion
    }

	public partial class /* CLIENT */ Selector 
	{
		#region SELECTOR MOTION
		[ClientRpc]
		private void Rpc_Move (int newSelection) 
		{
			// Correct carrousel if out of bounds
			if (newSelection == -1)
			{
				anim.SetInt ("Selection", (int)SelectionMax - 2);
				anim.SetFloat ("Blend", (SelectionMax - 1f) / SelectionMax);
			}
			else
			if (newSelection == 6)
			{
				anim.SetInt ("Selection", 2);
				anim.SetFloat ("Blend", 1f / SelectionMax);
			}
			else anim.SetInt ("Selection", newSelection);
		}
		#endregion

		protected override void OnClientAuthority () 
		{
			if (!isLocalPlayer) return;

			// Show owner marks
			frame.sprite = goldenFrame;
			anchor.gameObject.SetActive (true);
		}
	}
}
