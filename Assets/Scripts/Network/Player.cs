using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	public partial class /* COMMON */ Player : NetBehaviour 
	{
		[Info] public NetPawn pawn;

		private void ChangePawn (NetPawn newPawn) 
		{
			// De-authorize last Pawn, if any
			if (pawn)
			{
				pawn.owner = null;
				pawn.UpdateName ();
				if (Net.isClient) pawn.OnStopOwnership ();
			}
			// Authorize new Pawn
			pawn = newPawn;
			if (pawn)
			{
				pawn.owner = this;
				pawn.UpdateName ();
				if (Net.isClient) pawn.OnStartOwnership ();
			}
		}

		protected override void OnAwake () 
		{
			// Register self & preserve
			if (Net.isClient) Net.me = this;
			DontDestroyOnLoad (gameObject);
		}
	}

	public partial class /* SERVER */ Player 
	{
		#region DATA + CTOR
		[Info] public int ID;
		[Info] public string IP;
		[Info] public NetworkConnection Conn;
		[Info] public Heroe playingAs;

		public void PlayerSetup (NetworkConnection fromConn) 
		{
			if (ID != 0)
			{
				Debug.LogError ("Player is already set up!", this);
				return;
			}

			Conn = fromConn;
			IP = Conn.address;
			ID = Conn.connectionId;
			playingAs = Heroe.NONE;

			Log.LowDebug (string.Format ("Added Player {0} from {1}", ID, IP));
		} 
		#endregion

		#region COMMANDS
		[Command (channel = 2)]
		private void Cmd_Selection (int delta, bool readySwitch) 
		{
			var s = pawn as Selector;
			if (delta != 0) s.Move (delta);
			if (readySwitch) s.SwitchReady ();
		}

		[Command (channel = 2)]
		private void Cmd_Hero (float axis, bool jump, bool power) 
		{
			var h = pawn as Hero;
			h.Movement (axis);
			if (jump) h.Jumping ();
			if (power) h.Power ();
		}

		#region CHEATS
		[Command]
		public void Cmd_GrantPower (PowerUp power)
		{
			var h = pawn as Hero;
			h.power = power;
		}
		#endregion
		#endregion

		public void SetPawn (NetPawn newPawn) 
		{
			ChangePawn (newPawn);
			Rpc_SetPawn (newPawn!=null? newPawn.gameObject : null);
		}
	}

	public partial class /* CLIENT */ Player 
	{
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
				bool jump = Input.GetButtonDown ("Jump");
				bool power = Input.GetButtonDown ("Power");

				// Send to Server
				Cmd_Hero (-axis, jump, power);
			}
			#endregion
		}

		[ClientRpc]
		private void Rpc_SetPawn (GameObject newPawn) 
		{
			var pawn = newPawn.GetComponent<NetPawn> ();
			ChangePawn (pawn);
		}
	}
}

