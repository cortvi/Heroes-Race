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
	public sealed partial class /* COMMON */ Hero : NetPawn 
	{
		#region DATA
		private const float Speed = 10.0f;
		private const float JumpForce = 6.6f;

		[SyncVar] private Vector3 netPosition;      // Exact real position
		[SyncVar] private Quaternion netRotation;   // Transform rotation
		[SyncVar] private float netAngular;         // Speed around tower
		[SyncVar] private float netYForce;          // Vertical speed 
		[SyncVar] internal float movingDir;         // This is used by the Hero Camera

		[Info] public int floor;                    // The floor the Hero is in ATM
		private PowerUp _power;
		#endregion

		private void Update () 
		{
			// On Server, follow Driver
			if (Net.isServer) SyncMotion ();
			else;
			// On Clients, follow given motion
//			if (Net.isClient) KeepMotion ();
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
		private float lastYRot;
		private float lastVPos;
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
				driver.SwitchFriction (touchingFloor: false);
				anim.SetTrigger ("Jump");
				OnAir = true;
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

			// Send angular speed to Client if Hero moved enough
			float yRot = driver.transform.eulerAngles.y;
			netAngular = (yRot - lastYRot).IsZero (0.001f) ?
				0f : driver.body.angularVelocity.y * Mathf.Rad2Deg;
			lastYRot = yRot;

			// Same for vertical force
			float yPos = driver.body.position.y;
			netYForce = (yPos - lastVPos).IsZero (0.01f) ?
				0f : driver.body.velocity.y;
			lastVPos = yPos;
		}
		#endregion

		#region ANIMATION CALLS
		[ServerCallback]
		private void Jump () 
		{
			if (!mods[CCs.Jumping] &&
				!mods[CCs.Moving])
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
			float speed = input * Speed * (OnAir ? 1f : SpeedMul);
			var velocity = Vector3.up * speed * Time.fixedDeltaTime;

			// Don't modify speed if CCed,
			// because probably a external force is moving the Hero
			if (!mods[CCs.Moving]) driver.body.angularVelocity = velocity;
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

		public void SwitchCamFloor (int toFloor) 
		{
			floor = toFloor;
			// Switch the camera level on both Server & Client
			StartCoroutine (TowerCamera.i.tracking.SwitchFloor ());
			Target_SwitchCamFloor (owner.connectionToClient);
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
				blocks.Remove (name);
				Update ();
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
			// Always lerp rotation
			transform.rotation = Quaternion.Slerp (transform.rotation, netRotation, Time.deltaTime * 20f);

			// Lerp vertical position
			if (netYForce.IsZero ()) 
			{
				var pos = transform.position;
				pos.y = Mathf.Lerp (pos.y, netPosition.y, Time.deltaTime * 30f);
				transform.position = pos;
			}
			// Otherwise move with given speed
			else transform.Translate (Vector3.up * netYForce * Time.deltaTime);

			// Lerp whole Hero position
			if (netAngular.IsZero ()) 
			{
				var pos = transform.position;
				pos = Vector3.Lerp (pos, netPosition, Time.deltaTime * 20f);
				transform.position = pos;
			}
			// Otherwise move with given angular momentum
			else transform.RotateAround (Vector3.zero, Vector3.up, netAngular * Time.deltaTime);
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
		private void Target_UpdateHUD (NetworkConnection target, PowerUp newPower) 
		{
			// Show new power-up on owner Client
			cam.hud.UpdatePower (newPower);
			_power = newPower;
		}

		[TargetRpc]
		public void Target_SwitchCamFloor (NetworkConnection target) 
		{
			// Move Client camera
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