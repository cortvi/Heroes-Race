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
	[NetworkSettings (channel = 1)]
	public sealed partial class /* COMMON */ Hero : NetBehaviour 
	{
		[SyncVar] internal Vector3 netPosition;
		[SyncVar] internal Quaternion netRotation;
		[SyncVar] internal Vector3 netMovingDir;

		private void Update () 
		{
			if (isServer)
			{
				blocks.Read ();
				SyncMotion ();
			}
			else
			if (isPawn) KeepMotion ();
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
		public bool OnAir 
		{
			get { return anim.GetBool ("OnAir"); }
			set { anim.SetBool ("OnAir", value); }
		}

		// ——— Locomotion ———
		internal float speed = 10.0f;
		internal float input;
		internal float movingDir;

		// ——— Air-Ground check ——— 
		private float leaveFloorTime;
		private bool touchingFloorLastFrame = true;
		private const float OnAirThreshold = 0.3f;
		private const float MinFloorHeight = 0.2f;
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
		
		private void SyncMotion () 
		{
			// Send the moving direction for client-side prediction
			var newPos = ComputePosition ();
			if (input == 0f) netMovingDir = Vector3.zero;
			else netMovingDir = driver.body.velocity;

			// Positionate character based on Driver & propagate over Net
			transform.position = netPosition = newPos;
			transform.rotation = netRotation = ComputeRotation ();
		}
		#endregion

		#region PHYISICS
		[ServerCallback]
		private void Jump () 
		{
			driver.body.AddForce (Vector3.up * 6f, ForceMode.VelocityChange);
		}

		public void DriverCollision (Collision collision) 
		{
			return;

			// Find lowest contact point and check if it's low enough
			bool touchingFloor = collision.contacts.Min (c=> c.point.y) <= MinFloorHeight;
			if (touchingFloor)
			{
				// Reset fall-check
				touchingFloorLastFrame = true;

				// If hit floor from air (and in mid-air animation), land character
				if (anim.GetBool ("OnAir") && anim.IsInState ("Locomotion.Air.Mid_Air"))
				{
					anim.SetTrigger ("Land");
					anim.SetBool ("OnAir", false);
				}
			}
			else
			// Don't start time if on-air already
			if (!anim.GetBool ("OnAir")) 
			{
				// Start timer
				if (touchingFloorLastFrame) 
				{
					leaveFloorTime = Time.time + OnAirThreshold;
					touchingFloorLastFrame = false;
				}
				else
				// Must stay on-air some time before starting falling
				if (Time.time > leaveFloorTime) anim.SetBool ("OnAir", true);
			}
		}
		#endregion

		#region CALLBACKS
		[ServerCallback]
		private void FixedUpdate () 
		{
			float velocity = input * speed * Time.fixedDeltaTime;
			driver.body.angularVelocity = velocity * Vector3.up;
		}

		protected override void OnServerStart () 
		{
			// Get references
			anim = new SmartAnimator (GetComponent<Animator> (), networked: true);
			blocks = new CCStack (this);
		}
		#endregion

		#region HELPERS
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
		public class CCStack 
		{
			#region DATA + CTOR + IDXER
			private readonly Hero owner;
			private readonly Dictionary<string, CCs> collection;
			private CCs summary;

			public CCStack (Hero owner) 
			{
				this.owner = owner;
				collection = new Dictionary<string, CCs> ();
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
				foreach (var e in collection)
					summary = summary.SetFlag (e.Value);
			}

			public void Add (string name, CCs cc, float duration = -1f) 
			{
				collection.Add (name, cc);
				if (duration > 0f) owner.StartCoroutine 
						(RemoveAfter (name, duration));
			}

			public void Remove (string name) 
			{
				if (collection.ContainsKey (name))
					collection.Remove (name);
			}

			private IEnumerator RemoveAfter (string name, float duration) 
			{
				float release = Time.time + duration;
				while (Time.time < release) yield return null;
				collection.Remove (name);
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
			// If Hero stopped, smoothly lerp to real transform
			if (netMovingDir == Vector3.zero) transform.position = Vector3.Lerp (transform.position, netPosition, 3f);
			// Otherwise move in the last recieved direction
			else transform.Translate (netMovingDir * Time.deltaTime, Space.World);

			// Rotation is always lerped, it works pretty well out of the box
			transform.rotation = Quaternion.Slerp (transform.rotation, netRotation, 3f);
		}

		public override void OnBecomePawn () 
		{
			if (!cam) 
			{
				// Initialize camera to focus local Client
				cam = Camera.main.gameObject.AddComponent<HeroCamera> ();
				cam.target = this;
			}
		}
	}

	public enum Heroes 
	{
		NONE = -1,

		Espectador,
		Indiana,
		Harley,
		Harry,

		Count
	}

	[Flags] public enum CCs 
	{
		Moving = 1 << 0,
		Rotating = 1 << 1,
		Jumping = 1 << 2,

		// ——— Specials ———
		Locomotion = Moving | Rotating | Jumping,
		None = 0
	}
}
