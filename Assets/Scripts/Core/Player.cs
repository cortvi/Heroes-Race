using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	public partial class /* ALL */ Player : NetBehaviour 
	{
		internal NetBehaviour pawn;

		#region SERVER
		[Command]
		private void Cmd_Selection (int delta, bool readySwitch) 
		{
			var s = pawn as Selector;
			if (delta != 0) s.Move (delta);
			if (readySwitch) s.SwitchReady ();
		}

		[Command]
		private void Cmd_Tower (float axis, bool jump) 
		{
			var h = pawn as Hero;
			h.Movement (axis);
			if (jump) h.Jumping ();
		}
		#endregion

		#region CLIENT
		[ClientCallback]
		private void Update () 
		{
			if (!isLocalPlayer || !pawn) return;
			#region SELECTOR
			if (pawn is Selector)
			{
				// Initialize input
				int delta = 0;
				bool readySwitch = false;

				// Collect & pre-process data
				float axis = Input.GetAxis ("Horizontal");
				if (axis != 0f) delta = (axis > 0f ? +1 : -1);
				else
				if (Input.GetAxis ("Vertical") == 0f   // Set READY using any-key except axis
				&& Input.anyKeyDown) readySwitch = true;

				// Send to Server
				if (delta != 0 || readySwitch)
					Cmd_Selection (delta, readySwitch);
			} 
			#endregion
			else
			#region HERO
			if (pawn is Hero) 
			{
				// Collect all input
				float axis = Input.GetAxis ("Horizontal");
				bool jump = Input.GetButton ("Jump");

				// Send to Server
				Cmd_Tower (-axis, jump);
			} 
			#endregion
		}

		[TargetRpc]
		public void Target_SetPawn (NetworkConnection target, GameObject newPawn) 
		{
			// De-authorize last pawn, if any
			if (pawn) pawn.isPawn = false;
			pawn = newPawn.GetComponent<NetBehaviour> ();
			pawn.OnBecomePawn ();
			pawn.isPawn = true;
			pawn.UpdateName ();
		}
		#endregion
	}
}

