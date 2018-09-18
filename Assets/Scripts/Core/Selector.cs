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
		[Header ("References")]
		public Sprite goldenFrame;
		public Image frame;
		public Image anchor;
		[Space]

		[Range (0f, 5f)]
		[Tooltip ("Indiana is 2")]
		public int initialSelection;

		private Vector3 cachePosition;
		#endregion

		#region CALLBACKS
		protected override void OnAwake () 
		{
			if (NetworkServer.active)
			{
				anim = GetComponent<Animator> ().GoSmart (networked: true);

				if (heroesLocked == null)
					// This dictionary will tell if a Hero is already picked
					heroesLocked = new Dictionary<Heroes, bool> 
					{
						{ Heroes.Indiana, false },
						{ Heroes.Harley, false },
						{ Heroes.Harry, false }
					};
			}
			else anim.SetInt ("Selection", initialSelection);

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
		private SmartAnimator anim;
		private const float MaxSelection = 5f;

		private static int SelectorsReady;
		private static Dictionary<Heroes, bool> heroesLocked;
		#endregion

		#region SELECTOR MOTION
		[Command]
		private void Cmd_Input (int delta) 
		{
			// Won't move selection if locked OR already moving
			if (anim.GetBool("Ready") || anim.Animator.IsInTransition (0))
				return;

			int selection = anim.IncrementInt ("Selection", delta);
			// Correct selection & snap carousel to opposite bounds
			if (selection == -1) anim.SetInt ("Selection", 3);
			else
			if (selection == 6) anim.SetInt ("Selection", 2);
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
        }
        #endregion
    }

	public partial class /* CLIENT */ Selector 
	{
		[ClientCallback]
		private void Update () 
		{
			if (!isLocalPlayer) return;
			float input = Input.GetAxis ("Horizontal");

			// Send carrousel movement 
			if (input != 0) Cmd_Input (Mathf.CeilToInt (input));
			else
			// Set READY using any-key except axis
			if (Input.GetAxis ("Vertical") == 0f
			&& Input.anyKeyDown) Cmd_SwitchReady ();
		}

		protected override void OnClientAuthority () 
		{
			// Show owner marks
			frame.sprite = goldenFrame;
			anchor.gameObject.SetActive (true);
		}
	}
}
