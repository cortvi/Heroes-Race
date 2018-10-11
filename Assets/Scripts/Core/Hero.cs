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
		public const float Speed = 10.0f;

		[SyncVar] internal Vector3 netPosition;     // Exact real position
		[SyncVar] internal float netAngular;        // Speed around tower
		[SyncVar] internal Quaternion netRotation;  // Transform rotation
		[SyncVar] internal float movingDir;         // This is used by the Hero Camera
		#endregion

		private void Update () 
		{
			if (isServer)
			{
				blocks.Read ();
				SyncMotion ();
			}
			// On Clients, follow given motion
			else if (isClient) KeepMotion ();
		}
	}

	public sealed partial class /* SERVER */ Hero 
	{
		#region DATA
		// ——— Helpers ———
		internal Driver driver;
		internal CCStack blocks;

		// ——— Animation ———
		public SmartAnimator anim;
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
		public float SpeedMul 
		{
			get { return anim.GetFloat ("SpeedMul"); }
			set { anim.SetFloat ("SpeedMul", value); }
		}

		// ——— Locomotion ———
		internal float input;
		internal PowerUp power;
		#endregion

		#region LOCOMOTION
		public void Movement (float axis) 
		{
			if (!blocks[CCs.Moving])
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
			&& !blocks[CCs.Jumping]) 
			{
				anim.SetTrigger ("Jump");
				OnAir = true;
			}
		}
		public void Power () 
		{
			if (!OnAir
			&& !blocks[CCs.PowerUp])
			{
				StartCoroutine (UsePower ());
				Target_UpdateHUD (owner.Conn, power = PowerUp.None);
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
			driver.body.AddForce
				(Vector3.up * 6f, ForceMode.VelocityChange);
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
			if (!blocks[CCs.Moving]) driver.body.angularVelocity = velocity;
		}

		protected override void OnServerAwake () 
		{
			// Get references
			anim = new SmartAnimator (GetComponent<Animator> (), networked: true);
			blocks = new CCStack (this);

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
					SpeedMul *= 1.35f;
					yield return new WaitForSeconds (1.5f);
					SpeedMul /= 1.35f;
				}
				break;
				case PowerUp.Shield:
				{
					anim.SetTrigger ("Open_Shield");
					float endMark = Time.time + 2.2f;
					yield return new WaitForSeconds (0.2f);

					while (true) 
					{
						if (SpeedMul < 1f || blocks[CCs.Moving]) 
						{
							// Clear all buffs & consume shield
							anim.SetTrigger ("Consume_Shield");
							StartCoroutine (blocks.CleanCC ());
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
				}
				break;
			}
			// Add a little CD to powers
			yield return new WaitForSeconds (0.3f);
			blocks.Remove ("Using power");
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
			// Get signed facing rotation
			var faceDir = driver.transform.right * (movingDir > 0 ? 1f : -1f);
			var q = Quaternion.LookRotation (faceDir);

			// Lerp rotation for smooth turns
			return Quaternion.Slerp (transform.rotation, q, Time.deltaTime * 10f);
		}
		#endregion

		#region CC STACK
		internal class CCStack 
		{
			#region DATA + CTOR + IDXER
			private readonly Hero owner;
			private readonly Dictionary<string, CCs> stack;
			private CCs summary;

			public CCStack (Hero owner) 
			{
				this.owner = owner;
				stack = new Dictionary<string, CCs> ();
			}

			public bool this[CCs cc] 
			{
				get { return summary.HasFlag (cc); }
			}
			#endregion

			#region UTILS
			public void Read () 
			{
				summary = CCs.None;
				foreach (var e in stack)
					summary = summary.SetFlag (e.Value);
			}

			public IEnumerator CleanCC () 
			{
				// Find all debuffs 
				string[] keys = (from pair in stack
								 where pair.Value.HasFlag (CCs.Moving)
								 select pair.Key).ToArray ();

				// Copy string values to other memory references
				string[] Keys = new string[keys.Length];
				for (int i=0; i!=Keys.Length; i++)
					Keys[i] = string.Copy(keys[i]);

				// Remove them from the stack
				foreach (string key in Keys)
					stack.Remove (key);

				// Remove any slows
				if (owner.SpeedMul < 1f)
					owner.SpeedMul = 1f;

				owner.anim.Animator.SetLayerWeight ()
			}

			public void Add (string name, CCs cc, float duration = -1f, bool unique = true) 
			{
				// Add timestamp to remove uniqueness
				if (!unique) name += Time.time;

				stack.Add (name, cc);
				if (duration > 0f) owner.StartCoroutine 
						(RemoveAfter (name, duration));
			}

			public void Remove (string name) 
			{
				stack.Remove (name);
			}

			private IEnumerator RemoveAfter (string name, float duration) 
			{
				float release = Time.time + duration;
				while (Time.time < release) yield return null;
				stack.Remove (name);
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
			pos.y = Mathf.Lerp (pos.y, netPosition.y, Time.deltaTime * 50f);
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

		#region HUD
		[TargetRpc]
		public void Target_UpdateHUD (NetworkConnection target, PowerUp newPower) 
		{
			// Show new power-up on owner Client
			hud.UpdatePower (newPower);
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
