using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace HeroesRace 
{
	[NetworkSettings (sendInterval = 0f)]
	public partial class /* COMMON */ Player : NetBehaviour 
	{
		[Info] public NetPawn pawn;
		[SyncVar] public Pawns pawnType;

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
		[Command]
		private void Cmd_RequestPawn () 
		{
			// Assign new Selector for Player if none
			if (Net.networkSceneName == "Selection")
			{
				if (!(pawn is Selector))
				{
					var check = new Func<Selector, bool> (s => s.SharedName == "Selector_" + ID);
					ChangePawn (FindObjectsOfType<Selector> ().First (check));
				}
				pawnType = Pawns.Selector;
			}
			else
			// Create new Hero for Player if none
			if (Net.networkSceneName == "Tower")
			{
				if (!(pawn is Hero))
				{
					// If bypassing selection menu
					if (playingAs == Heroe.NONE)
						playingAs = (Heroe)ID;

					// Spawn Heroe & set up its Driver
					var hero = Instantiate (Resources.Load<Hero> ("Prefabs/Heroes/" + playingAs));
					hero.driver = Instantiate (Resources.Load<Driver> ("Prefabs/Character_Driver"));
					hero.driver.name = playingAs + "_Driver";
					hero.driver.owner = hero;

					NetworkServer.Spawn (hero.gameObject);
				}
				pawnType = Pawns.Hero;
			}
		}

		[Command (channel = 2)]
		private void Cmd_Selection (int delta, bool readySwitch) 
		{
			if (Net.paused) return;
			var s = pawn as Selector;

			if (delta != 0) s.Move (delta);
			if (readySwitch) s.SwitchReady ();
		}

		[Command (channel = 2)]
		private void Cmd_Hero (float axis, bool jump, bool power) 
		{
			if (Net.paused) return;
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
	}

	public partial class /* CLIENT */ Player 
	{
		[ClientCallback]
		private void Update () 
		{
			// Only work local Client & when having a valid Pawn
			if (!isLocalPlayer || pawnType == Pawns.None) return;

			#region SELECTOR
			if (pawnType == Pawns.Selector)
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
				if (delta != 0 || readySwitch) Cmd_Selection (delta, readySwitch);
			}
			#endregion
			else
			#region HERO
			if (pawnType == Pawns.Hero)
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

		private void OnLevelLoaded (Scene scene, LoadSceneMode mode) 
		{
			// When Player object is ready, call for Pawn assigment
			Cmd_RequestPawn ();
		}

		[ClientCallback]
		private void OnEnable () 
		{
			SceneManager.sceneLoaded += OnLevelLoaded;
		}
		[ClientCallback]
		private void OnDisable () 
		{
			SceneManager.sceneLoaded -= OnLevelLoaded;
		}
	}

	public enum Pawns 
	{
		None,
		Hero,
		Selector
	}
}

