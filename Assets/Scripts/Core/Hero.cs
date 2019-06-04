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
//		[SerializeField]
//		private float Squash

		private const float Speed = 10.0f;
		private const float JumpForce = 6.5f;

		[SyncVar] private Vector3 netPosition;      // Exact real position
		[SyncVar] private Quaternion netRotation;	// Transform rotation
		[SyncVar] private float netAngular;			// Speed around tower
		[SyncVar] private float netYSpeed;			// Vertical speed
		[SyncVar] internal float movingDir;			// This is used by the Hero Camera
		[SyncVar] [Info] public int floor;			// The floor the Hero is in ATM
		#endregion

		private void Update ()
		{
			// On Server, follow Driver
			if (Net.IsServer) SyncMotion ();
			else
			// On Clients, follow given motion
			if (Net.IsClient) KeepMotion ();
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
		public float SpeedMul 
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
		internal float vDir;

		public PowerUp Power { get; private set; }
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
				mods.Add ("On Jump", CCs.Jumping);
				driver.SwitchFriction (touchingFloor: false);
				anim.SetTrigger ("Jump");
			}
		}
		public void PowerCall () 
		{
			if (!mods[CCs.PowerUp]
			// Can't cast a shield if already immune
			&& !(Power == PowerUp.Shield && Immune)
			// No sense to speed up while in air
			&& !(Power == PowerUp.Speed && OnAir))
			{
				StartCoroutine (UsePower ());
				UpdatePower (PowerUp.None);
			}
		}

		private void SyncMotion () 
		{
			// Positionate character based on Driver & propagate over Net
			transform.position = netPosition = ComputePosition ();
			transform.rotation = netRotation = ComputeRotation ();

			// Save vertical moving direction for floor checks
			vDir = Mathf.Sign (transform.position.y - lastPos.y);

			// Vertical & angular speed are synced
			// based on whether they change or not
			var pos = transform.position;
			var last = lastPos; /**/ lastPos = pos;

			// Height sync
			netYSpeed =
				(pos.y - last.y).IsZero (0.00001f) ?
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
				mods.Remove ("On Jump");
				// Impulse Hero upwards if possible (may be CCd between animation)
				driver.body.AddForce (Vector3.up * JumpForce, ForceMode.VelocityChange);
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

			// Don't modify speed if CCed (probably a external force is moving the Hero)
			if (!mods[CCs.Moving])
			{
				if (OnAir)
				{
					// If on air, reduce non-vertical speed
					float downScale = 0.7f;
					velocity.x *= downScale;
					velocity.z *= downScale;
				}
				driver.body.angularVelocity = velocity;
			}
		}

		protected override void OnServerAwake () 
		{
			// Check if other Hero has appeared already
			bool notFirst = (Net.activeCam != null);

			// Initalize camera & hud for this Hero on Server
			OnStartOwnership ();
			if (notFirst)
			{
				// Disable if not first
				cam.gameObject.SetActive (false);
				hud.gameObject.SetActive (false);
			}
			else
			{
				// Set this as active:
				Net.activeCam = cam;
				Net.activeHUD = hud;
			}
			anim = GetComponent<Animator> ().GoSmart (networked: true);
			mods = GetComponent<ModifierStack> ();
		}
		#endregion

		#region HELPERS
		public void UpdatePower (PowerUp power) 
		{
			// Update HUD in Server & Client:
			if (Net.IsServer)
			{
				Target_UpdatePower (owner.Conn, power);
			}
			hud.UpdatePower (power);
			Power = power;
		}

		private IEnumerator UsePower () 
		{
			switch (Power)
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
					while (true)
					{
						// Wait until a CC is recieved
						if (mods.Debuffed)
						{
							// Clear all buffs & consume shield
							anim.SetTrigger ("Consume_Shield");
							mods.CleanCC ();
							break;
						}
						else
						// Or until shield runs out of time
						if (Time.time > endMark)
						{
							// Break shield
							anim.SetTrigger ("Break_Shield");
							break;
						}
						else yield return null;
					}
					// Preserve immunity for a little time
					yield return new WaitForSeconds (0.5f);
					Immune = false;
				}
				break;
			}
		}

		private Vector3 ComputePosition () 
		{
			// Do not sync with Driver if attached
			if (transform.parent != null) return transform.position;

			// Get capsule position, discard height
			var pos = driver.capsule.center; /* */ pos.y = 0f;
			// Return the position in world-space
			return driver.transform.TransformPoint (pos);
		}
		private Quaternion ComputeRotation () 
		{
			// Do not sync with Driver if attached OR blocked
			if (transform.parent != null || mods[CCs.Rotating])
				return transform.rotation;

			// Get signed facing rotation
			var faceDir = driver.transform.right * (movingDir > 0 ? 1f : -1f);
			var q = Quaternion.LookRotation (faceDir);

			// Lerp rotation for smooth turns
			return Quaternion.Slerp (transform.rotation, q, Time.deltaTime * 10f);
		}
		#endregion
	}

	public sealed partial class /* CLIENT */ Hero 
	{
		internal HeroCam cam;
		internal HeroHUD hud;

		private void KeepMotion () 
		{
			#region NOT ATTACHED
			// If not attached to anything
			if (transform.parent == null)
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
			}
			#endregion

			#region ATTACHED
			else
			// Otherwise do nothing unless far from real position
			if (Vector3.Distance (netPosition, transform.position) > 0.1f)
			{
				// Lerp to it if so
				var pos = transform.position;
				pos = Vector3.Lerp (pos, netPosition, Time.deltaTime * 5f);
				transform.position = pos;
			} 
			#endregion

			// Finally, always lerp rotation
			transform.rotation = Quaternion.Slerp (transform.rotation, netRotation, Time.deltaTime * 20f);
		}

		internal override void OnStartOwnership () 
		{
			// Create new Camera
			cam = HeroCam.New (name);
			cam.target = this;

			// Initialize local HUD
			hud = Instantiate (Resources.Load<HeroHUD> ("Prefabs/HUD"));
			hud.name = name + "_HUD";
		}

		[TargetRpc]
		private void Target_UpdatePower (NetworkConnection conn, PowerUp power) 
		{
			UpdatePower (power);
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

	[System.Flags]
	public enum CCs 
	{
		Moving		= 1 << 0,
		Rotating	= 1 << 1,
		Jumping		= 1 << 2,
		PowerUp		= 1 << 3,

		// ——— Specials ———
		Locomotion = Moving | Rotating | Jumping,
		None = 0,
		All = ~0
	}
	#endregion
}