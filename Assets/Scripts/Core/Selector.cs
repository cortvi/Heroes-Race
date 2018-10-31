using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

namespace HeroesRace 
{
	public partial class /* COMMON */ Selector : NetPawn  
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
		private const float SelectionMax = ((int)Heroe.Count + 1f);
		private Vector3 cachePosition;
		#endregion

		#region CALLBACKS
		private void Update () 
		{
			// Blend the animator space for all instances
			float blend = anim.GetFloat ("Blend");
			float target = anim.GetInt ("Selection") / SelectionMax;
			float lerp = Mathf.Lerp (blend, target, Time.deltaTime * 8f);
			anim.SetFloat ("Blend", lerp);

			// Only allow movement after some transition time
			if (isServer) closeEnough = Mathf.Abs(target - blend) <= 0.025f;
		}

		protected override void OnStart () 
		{
			if (Net.isServer) anim.SetInt ("Selection", initialSelection);
			// Recover original position in case it's been moved by the Server
			(transform as RectTransform).localPosition = cachePosition;
		}

		protected override void OnAwake () 
		{
			anim = GetComponent<Animator> ().GoSmart (networked: true);
			// Cache position because it'll move when connected to Server
			cachePosition = (transform as RectTransform).localPosition;
		}
		#endregion
	}

	public partial class /* SERVER */ Selector   
	{
		private static int SelectorsReady;
		private bool closeEnough;

		public void Move (int delta) 
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
			closeEnough = false;
			
			// Animator is owned by the client, so send the new Selection
			Rpc_Move (selection);
		}

		public void SwitchReady () 
		{
			// Can't switch ready if moving carrousel
			if (!closeEnough) return;

			// Flip the ready state
			bool ready = !anim.GetBool ("Ready");
			SelectorsReady += (ready? +1 : -1);
			anim.SetBool ("Ready", ready);

			if (SelectorsReady == Net.PlayersNeeded) 
				StartCoroutine (GoToTower ());
		}

		#region HELPERS
		public Heroe ReadHero () 
		{
			// Translate selection into a Hero
			int selection = anim.GetInt ("Selection");
			if (selection == 0) return Heroe.Harry;
			else
			if (selection == 5) return Heroe.Espectador;
			else return (Heroe)(selection - 1);
		} 

		#warning in the future, make the scene change smoother!
        public static IEnumerator GoToTower () 
        {
			// Read all the selected heroes
			for (int i = 0; i != Net.PlayersNeeded; i++)
            {
                var selector = FindObjectsOfType<Selector> ()[i];
                Net.players[i].playingAs = selector.ReadHero ();
            }
			Log.Debug ("Heroes saved, now going to Tower map!");

            // Change scene
			yield return new WaitForSeconds (1f);
            Net.worker.ServerChangeScene ("Tower");
        }
        #endregion
    }

	public partial class /* CLIENT */ Selector 
	{
		[ClientRpc]
		private void Rpc_Move (int newSelection) 
		{
			// Correct carrousel if out of bounds
			if (newSelection == -1) anim.SetFloat ("Blend", (SelectionMax - 1f) / SelectionMax);
			else
			if (newSelection == +6) anim.SetFloat ("Blend", 1f / SelectionMax);
		}

		internal override void OnStartOwnership () 
		{
			frame.sprite = goldenFrame;
			anchor.gameObject.SetActive (true);
		}
	}
}
