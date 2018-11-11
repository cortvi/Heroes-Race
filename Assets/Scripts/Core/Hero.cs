using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

/* The Hero mesh & animator, as well as this script,
* are contained on a single GameObject.
* 
* The rigidbody and colliders that control its physic movement (angular rotation)
* and such are separated in a Driver GameObject and only present on the Server.
* 
* All the Clients get their 3D data from the Server, then it's lerped for a fluid movement.
* No Client makes actual physics logic, everything is computed on the Server and passed to the Clients. */
namespace HeroesRace
{
	[NetworkSettings (channel = 2, sendInterval = 0f)]
	public sealed partial class /* COMMON */ Hero : NetPawn 
	{
		#region DATA
		private const float Speed = 10.0f;
		private const float JumpForce = 6.3f;

		[SyncVar] private Vector3 netPosition;      // Exact real position
		[SyncVar] private Quaternion netRotation;	// Transform rotation
		[SyncVar] private float netAngular;			// Speed around tower
		[SyncVar] private float netYSpeed;			// Vertical speed
		[SyncVar] internal float movingDir;			// This is used by the Hero Camera

		[Info] public int floor;					// The floor the Hero is in ATM
		private PowerUp _power;
		#endregion

		private void Update () 
		{
			// On Server, follow Driver
			if (Net.isServer) SyncMotion ();
			else
			// On Clients, follow given motion
			if (Net.isClient) KeepMotion ();
		}
	}

	public sealed partial class /* SERVER */ Hero 
	{
		#region DATA
		// ——— Helpers ———
		internal Driver driver;
		internal ModifierStack mods;

		// ——— Animation ———
		public SmartAnimator anim;
		private float SpeedMul 
		{
			get { return anim.GetFloat ("SpeedMul"); }
			set { anim.SetFloat ("SpeedMul", value); }
		}
		public bool Immune 
		{
			get { return anim.GetBool ("Immune"); }
			set { anim.SetBool ("Immune", value); }
		}
		public bool Moving 
		{
			get { return anim.GetBool ("Moving"); }
			set { anim.SetBool ("Moving", value); }
		}
		public bool OnAir 
		{
			get { return anim.GetBool ("OnAir"); }
			set { anim.SetBool ("OnAir", value); }
		}

		// ——— Locomotion ———
		private float input;
		private Vector3 lastPos;
		internal PowerUp power 
		{
			get { return _power; }
			set
			{
				Target_UpdateHUD (owner.Conn, value);
				_power = value;
			}
		}
		#endregion

		#region LOCOMOTION
		public void Movement (float axis) 
		{
			if (!mods[CCs.Moving])
			{
				// Save input for later physics
				if (axis != 0f) movingDir = axis;
				input = axis;
			}
			else input = 0f;

			if (input != 0f) Moving = true;
			else Moving = false;
		}
		public void Jumping () 
		{
			if (!OnAir
			&& !mods[CCs.Jumping]) 
			{
				mods.Block ("On Jump", CCs.Jumping);
				driver.SwitchFriction (touchingFloor: false);
				anim.SetTrigger ("Jump");
			}
		}
		public void Power () 
		{
			if (!mods[CCs.PowerUp]
			// Can't cast a shield if already immune
			&& !(power == PowerUp.Shield && Immune)
			// No sense to speed up while in air
			&& !(power == PowerUp.Speed && OnAir))
			{
				StartCoroutine (UsePower ());
				power = PowerUp.None;
			}
		}

		private void SyncMotion () 
		{
			// Positionate character based on Driver & propagate over Net
			transform.position = netPosition = ComputePosition ();
			transform.rotation = netRotation = ComputeRotation ();
			// Check if floor changed
			CheckFloor ();

			// Vertical & angular speed are synced
			// based on whether they change or not
			var pos = transform.position;
			var last = lastPos; /**/ lastPos = pos;

			// Height sync
			netYSpeed =
				(pos.y - lastPos.y).IsZero (0.00001f) ?
				0f : driver.body.velocity.y;

			// Project both
			last.y = 0f; /**/ pos.y = 0f;

			// Speed sync
			netAngular =
				Vector3.Distance (pos, last).IsZero () ?
				0f : driver.body.angularVelocity.y * Mathf.Rad2Deg;
		}
		#endregion

		#region ANIMATION CALLS
		[ServerCallback]
		private void Jump () 
		{
			if (!mods[CCs.Moving])
			{
				// Impulse Hero upwards if possible (may be CCd between animation)
				driver.body.AddForce (Vector3.up * JumpForce, ForceMode.VelocityChange);
				mods.Unblock ("On Jump");
				OnAir = true;
			}
		}
		#endregion

		#region CALLBACKS
		[ServerCallback]
		private void FixedUpdate () 
		{
			// If on-air, don't apply speed modifiers
			float speed = input * Speed * (OnAir? 1f : SpeedMul);
			var velocity = Vector3.up * speed * Time.fixedDeltaTime;

			// Don't modify speed if CCed,
			// because probably a external force is moving the Hero
			if (!mods[CCs.Moving])
			{
				if (OnAir) driver.body.AddTorque (velocity, ForceMode.Acceleration);
				driver.body.angularVelocity = velocity;
			}
		}

		protected override void OnServerAwake () 
		{
			anim = GetComponent<Animator> ().GoSmart (networked: true);
			mods = new ModifierStack (this);
		}

		protected override void OnServerStart () 
		{
			// Notify Tower Camera if first player to enter!
			if (TowerCamera.i.tracking.target == null)
				TowerCamera.i.tracking.target = this;
		}
		#endregion

		#region HELPERS
		private IEnumerator UsePower () 
		{
			switch (power)
			{
				case PowerUp.Speed:
				{
					// Speed up Hero
					mods.SpeedUp (0.4f);
					yield return new WaitForSeconds (1.5f);
					mods.SpeedUp (0f);
				}
				break;
				case PowerUp.Shield:
				{
					Immune = true;
					anim.SetTrigger ("Open_Shield");
					float endMark = Time.time + 2f;
					// Wait until a CC is recieved
					// or shield just runs out of time
					while (true)
					{
						if (mods.Debuffed)
						{
							// Clear all buffs & consume shield
							anim.SetTrigger ("Consume_Shield");
							mods.CleanCC ();
							break;
						}
						else
						if (Time.time > endMark)
						{
							// Break shield
							anim.SetTrigger ("Break_Shield");
							break;
						}
						else yield return null;
					}
					// Preserver immunity for X seconds
					yield return new WaitForSeconds (0.5f);
					Immune = false;
				}
				break;
			}
		}

		private Vector3 ComputePosition () 
		{
			// Get capsule position, discard height
			var pos = driver.capsule.center; pos.y = 0f;
			// Return the position in world-space
			return driver.transform.TransformPoint (pos);
		}
		private Quaternion ComputeRotation () 
		{
			if (mods[CCs.Rotating]) return transform.rotation;

			// Get signed facing rotation
			var faceDir = driver.transform.right * (movingDir > 0 ? 1f : -1f);
			var q = Quaternion.LookRotation (faceDir);

			// Lerp rotation for smooth turns
			return Quaternion.Slerp (transform.rotation, q, Time.deltaTime * 10f);
		}

		private void CheckFloor () 
		{
			float height = transform.position.y;
			float floorStart = floor * HeroCamera.FloorHeigth;
			int dir = (int) Mathf.Sign (height - lastPos.y);

			// Use a treshold for both going up & down
			if (height > floorStart - 0.45f && height < floorStart + 0.75f) 
			{
				floor += dir;
				SwitchFloor (floor);
			}
		}

		private void SwitchFloor (int floor) 
		{
			print ("Changing to floor " + floor);
			// Switch the camera level on both Server & Client
			StartCoroutine (TowerCamera.i.tracking.SwitchFloor ());
			Target_SwitchCamFloor (owner.connectionToClient, floor);
		}
		#endregion

		#region MODIFIER STACK
		internal class ModifierStack 
		{
			#region DATA
			private readonly Hero owner;
			private readonly Dictionary<string, CCs> blocks;
			private readonly Dictionary<string, CCs> impairings;

			private CCs summary;
			private float speedBuff;
			private float speedDebuff;

			public bool Debuffed 
			{
				get
				{
					// Debuffed if slowed or directly impaired
					return (speedDebuff != 1f && speedBuff == 1f)
						|| impairings.Count > 0;
				}
			}
			#endregion

			public ModifierStack (Hero owner) 
			{
				this.owner = owner;
				blocks = new Dictionary<string, CCs> ();
				impairings = new Dictionary<string, CCs> ();
				speedBuff = speedDebuff = 1f;
			}
			public bool this[CCs cc] 
			{
				get { return summary.HasFlag (cc); }
			}

			private void Update () 
			{
				summary = CCs.None;
				foreach (var b in blocks) summary = summary.SetFlag (b.Value);
				foreach (var i in impairings) summary = summary.SetFlag (i.Value);

				// Speed Buffs have priority!
				if (speedBuff != 1f) owner.SpeedMul = speedBuff;
				else
				if (speedDebuff != 1f) owner.SpeedMul = speedDebuff;

				else owner.SpeedMul = 1f;
			}

			#region UTILS
			public bool Block (string name, CCs cc) 
			{
				// Skip if not unique
				if (blocks.ContainsKey (name)) return false;

				blocks.Add (name, cc);
				Update ();
				return true;
			}
			public void Unblock (string name) 
			{
				if (blocks.ContainsKey (name))
				{
					blocks.Remove (name);
					Update ();
				}
			}

			public bool AddCC (string name, CCs cc, float duration) 
			{
				// Skip if not unique
				if (impairings.ContainsKey (name)) return false;

				impairings.Add (name, cc);
				owner.StartCoroutine (RemoveCCAfter (name, duration));
				Update ();
				return true;
			}
			private IEnumerator RemoveCCAfter (string name, float duration) 
			{
				float release = Time.time + duration;
				while (Time.time < release) yield return null;
				impairings.Remove (name);
				Update ();
			}

			public void CleanCC () 
			{
				impairings.Clear ();
				speedDebuff = 1f;
				Update ();
			}

			public void SpeedUp (float amount) 
			{
				if (amount != 0f) speedBuff *= (1 + amount);
				else speedBuff = 1f;
				Update ();
			}
			public void SpeedDown (float amount) 
			{
				if (amount != 0f) speedDebuff *= (1 - amount);
				else speedDebuff = 1f;
				Update ();
			}
			#endregion
		}
		#endregion
	}

	public sealed partial class /* CLIENT */ Hero 
	{
		internal HeroCamera cam;

		private void KeepMotion () 
		{
			// Lerp vertically
			if (netYSpeed.Is (0f)) 
			{
				var pos = transform.position;
				pos.y = Mathf.Lerp (pos.y, netPosition.y, Time.deltaTime * 30f);
				transform.position = pos;
			}
			// Otherwise move with given speed
			else transform.Translate (Vector3.up * netYSpeed * Time.deltaTime);

			// Lerp whole Hero position if not moving OR moved too far from networked position
			if (netAngular.Is (0f) || Vector3.Distance (netPosition, transform.position) > 0.1f) 
			{
				var pos = transform.position;
				pos = Vector3.Lerp (pos, netPosition, Time.deltaTime * 10f);
				transform.position = pos;
			}
			// Otherwise move with given angular momentum
			else transform.RotateAround (Vector3.zero, Vector3.up, netAngular * Time.deltaTime);

			// Finally, lerp rotation always
			transform.rotation = Quaternion.Slerp (transform.rotation, netRotation, Time.deltaTime * 20f);
		}

		internal override void OnStartOwnership () 
		{
			if (!cam)
			{
				// Initialize camera to focus local Client
				cam = Camera.main.gameObject.AddComponent<HeroCamera> ();
				cam.target = this;
			}
		}

		#region RPC CALLS
		[TargetRpc]
		private void Target_UpdateHUD (NetworkConnection conn, PowerUp newPower) 
		{
			// Show new power-up on owner Client
			cam.hud.UpdatePower (newPower);
			_power = newPower;
		}

		[TargetRpc]
		public void Target_SwitchCamFloor (NetworkConnection conn, int toFloor) 
		{
			floor = toFloor;
			StartCoroutine (cam.SwitchFloor ());
		}
		#endregion
	}

	#region ENUMS
	public enum Heroe 
	{
		NONE = -1,

		Espectador,
		Indiana,
		Harley,
		Harry,

		Count
	}

	public enum PowerUp 
	{
		None,
		Speed,
		Shield,
		//		Bomb,

		Count
	}

	[System.Flags]
	public enum CCs 
	{
		Moving = 1 << 0,
		Rotating = 1 << 1,
		Jumping = 1 << 2,
		PowerUp = 1 << 3,

		// ——— Specials ———
		Locomotion = Moving | Rotating | Jumping,
		None = 0,
		All = ~0
	}
	#endregion
}