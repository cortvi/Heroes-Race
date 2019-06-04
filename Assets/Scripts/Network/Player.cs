using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace HeroesRace 
{
	public partial class /* COMMON */ Player : NetBehaviour 
	{
		[Info] public NetPawn pawn;

		public void ChangePawn (NetPawn newPawn) 
		{
			// De-authorize last Pawn, if any
			if (pawn)
			{
				pawn.owner = null;
				pawn.UpdateName ();
				if (Net.IsClient && isLocalPlayer) pawn.OnStopOwnership ();
			}
			// Authorize new Pawn
			pawn = newPawn;
			if (pawn)
			{
				pawn.owner = this;
				pawn.UpdateName ();
				if (Net.IsClient && isLocalPlayer) pawn.OnStartOwnership ();
			}
		}

		protected override void OnAwake () 
		{
			// Register self & preserve
			if (Net.IsClient && isLocalPlayer) Net.me = this;
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

		// Pawns get assigned when Server has loaded the scene:
		public readonly static Queue<Player> pawnQueue = new Queue<Player> ();

		// "Constructor":
		public void PlayerSetup (NetworkConnection fromConn) 
		{
			if (ID != 0)
			{
				Debug.LogError ("Player is already set up!", this);
				return;
			}

			Conn = fromConn;
			IP = Conn.address;
			ID = Net.players.Count (p=> p != null) + 1;

			name += "_" + ID;
			playingAs = Heroe.NONE;

			Log.LowDebug (string.Format ("Added Player {0} from {1}", ID, IP));
		}
		#endregion

		#region COMMANDS
		[Command]
		private void Cmd_RequestPawn () 
		{
			// Make sure because because on changing scenes shit happens
			if (!Conn.isReady) NetworkServer.SetClientReady (Conn);
			pawnQueue.Enqueue (this);
			SetPawns ();
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
			if (power) h.PowerCall ();
		}

		#region CHEATS
		[Command]
		public void Cmd_GrantPower (PowerUp power)
		{
			var h = pawn as Hero;
			h.UpdatePower (power);
		}
		#endregion
		#endregion

		public static void SetPawns () 
		{
			if (pawnQueue.Count != Net.PlayersNeeded || Q1.i == null)
				return;

			// This is fucking sick bullshit...
			while (pawnQueue.Count > 0)
			{
				var p = pawnQueue.Dequeue ();

				// Assign new Selector for Player if none
				if (Net.networkSceneName == "Selection")
				{
					if (!(p.pawn is Selector))
					{
						var check = new Func<Selector, bool> (s => s.SharedName == "Selector_" + p.ID);
						p.ChangePawn (FindObjectsOfType<Selector> ().First (check));
					}
				}
				else
				// Create new Hero for Player if none
				if (Net.networkSceneName == "Tower")
				{
					if (!(p.pawn is Hero))
					{
						// If bypassing selection menu
						if (p.playingAs == Heroe.NONE)
							p.playingAs = (Heroe) p.ID;

						// Spawn Heroe & set up its Driver
						var hero = Instantiate (Resources.Load<Hero> ("Prefabs/Heroes/" + p.playingAs));
						hero.driver = Instantiate (Resources.Load<Driver> ("Prefabs/Character_Driver"));
						hero.driver.name = p.playingAs + "_Driver";
						hero.driver.owner = hero;

						// Position Driver
						var spawn = Q1.i.GetSpawn (p.ID);
						hero.driver.transform.rotation = Quaternion.LookRotation (spawn);

						NetworkServer.Spawn (hero.gameObject);
						p.ChangePawn (hero);
					}
				}
				if (p.pawn) p.Rpc_SetPawn (p.pawn.gameObject);
			}
		}
	}

	public partial class /* CLIENT */ Player 
	{
		#region CALLBACKS
		[ClientCallback]
		private void Update () 
		{
			// Only work local Client & when having a valid Pawn
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
				if (delta != 0 || readySwitch) Cmd_Selection (delta, readySwitch);
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

		private void OnLevelLoaded (Scene scene, LoadSceneMode mode) 
		{
			// When Player object is ready, 
			// call for Pawn assigment:
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
		#endregion

		[ClientRpc]
		public void Rpc_SetPawn (GameObject netPawn) 
		{
			var pawn = netPawn.GetComponent<NetPawn> ();
			ChangePawn (pawn);
		}
	}
}

