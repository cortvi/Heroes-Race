using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

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
	public sealed partial class /* COMMON */ Hero : NetBehaviour 
	{
		#region DATA
		private const float Speed = 10.0f;
		private const float JumpForce = 6f;

		[SyncVar] internal Vector3 netPosition;     // Exact real position
		[SyncVar] internal float netAngular;        // Speed around tower
		[SyncVar] internal Quaternion netRotation;  // Transform rotation
		[SyncVar] internal float movingDir;         // This is used by the Hero Camera

		private PowerUp _power;
		#endregion

		private void Update () 
		{
			// On Server, follow Driver
			if (isServer) SyncMotion ();
			// On Clients, follow given motion
			else if (isClient) KeepMotion ();
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
		internal float input;
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
				anim.SetTrigger ("Jump");
				OnAir = true;
				driver.SwitchFriction (false);
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

			// Send angular speed to allow client-side prediction
			// If speed is too low, asume it's zero
			float angular = driver.body.angularVelocity.y;
			if (input != 0f && angular >= 0.9f) netAngular = (Mathf.Rad2Deg * angular);
			else netAngular = 0f;
		}
		#endregion

		#region ANIMATION CALLS
		[ServerCallback]
		private void Jump () 
		{
			if (!mods[CCs.Jumping]
			&& (!mods[CCs.Moving]))
			{
				// Impulse Hero upwards if possible (may be CCd between animation)
				driver.body.AddForce (Vector3.up * JumpForce, ForceMode.VelocityChange);
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
			if (!mods[CCs.Moving]) driver.body.angularVelocity = velocity;
		}

		protected override void OnServerAwake () 
		{
			// Get references
			anim = new SmartAnimator (GetComponent<Animator> (), networked: true);
			mods = new ModifierStack (this);

			#warning adding a Hero camera for testing in the server!
			OnStartOwnership ();
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
			{get
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
				foreach (var b in blocks)		summary = summary.SetFlag (b.Value);
				foreach (var i in impairings)	summary = summary.SetFlag (i.Value);

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
				blocks.Remove (name);
				Update ();
			}

			public bool AddCC (string name, CCs cc, float duration, bool unique = true) 
			{
				// Add framestamp to remove uniqueness
				if (!unique) name += Time.frameCount;
				// If should be unique, but it's not, just skip it
				else if (impairings.ContainsKey (name)) return false;

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
		private HeroCamera cam;
		private HeroHUD hud;

		private void KeepMotion () 
		{
			// Height is lerped always
			var pos = transform.position;
			pos.y = Mathf.Lerp (pos.y, netPosition.y, Time.deltaTime * 30f);
			transform.position = pos;

			// Lerp to real position if stopped, otherwise move around tower in given speed
			if (netAngular == 0f) transform.position = Vector3.Lerp (transform.position, netPosition, Time.deltaTime * 10f);
			else transform.RotateAround (Vector3.zero, Vector3.up, netAngular * Time.deltaTime);

			// Rotation is always lerped too
			transform.rotation = Quaternion.Slerp (transform.rotation, netRotation, Time.deltaTime * 30f);
		}

		internal override void OnStartOwnership () 
		{
			if (!cam) 
			{
				// Initialize camera to focus local Client
				cam = Camera.main.gameObject.AddComponent<HeroCamera> ();
				cam.target = this;
			}
			if (!hud && isClient)
			{
				// Initialize Client-side HUD
				hud = Instantiate (Resources.Load<HeroHUD> ("Prefabs/HUD"));
			}
		}

		[TargetRpc]
		private void Target_UpdateHUD (NetworkConnection target, PowerUp newPower) 
		{
			// Show new power-up on owner Client
			hud.UpdatePower (newPower);
			_power = newPower;
		}
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

	[Flags] public enum CCs 
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