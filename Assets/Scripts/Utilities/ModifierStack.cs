using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace HeroesRace 
{
	public class ModifierStack : MonoBehaviour 
	{
		#region DATA
		private Hero owner;
		private CCs summary;
		public bool this[CCs cc] 
		{
			get { return summary.HasFlag (cc); }
		}

		public List<Mod> mods;
		private Triggers triggers;

		private float speedBuff;
		private float speedDebuff;

		public bool Debuffed 
		{
			get
			{
				// Debuffed if slowed or directly impaired
				return (speedDebuff != 1f && speedBuff == 1f)
					|| mods.Any (m=> m.duration > 0f);
			}
		}

		[Serializable]
		public class Mod 
		{
			public readonly bool unique;
			public readonly string name;
			public readonly float duration;

			public readonly Triggers trigger;
			public readonly CCs cc;
			public float timer;

			public Mod (string name, CCs cc, float duration, Triggers trigger, bool unique) 
			{
				this.name = name;
				this.cc = cc;
				this.trigger = trigger;
				this.duration = duration;
				this.unique = unique;
			}

			public bool Update () 
			{
				if (duration > 0f)
				{
					// Return only false when Mod's expired
					if (timer > duration) return false;
					else timer += Time.deltaTime;
				}
				return true;
			}
		}
		#endregion

		private void LateUpdate () 
		{
			// Reset values
			summary = CCs.None;
			Triggers triggers = Triggers.None;

			// Read mods (iterating backwards allows removing)
			for (int i=mods.Count-1; i>=0; --i) 
			{
				// Move mod timer
				if (mods[i].Update ())
				{
					summary |= mods[i].cc;
					triggers |= mods[i].trigger;
				}
				// Remove if expired
				else mods.RemoveAt (i);
			}

			// Iterate over all trigger flags
			for (int t = 1; t != (int)Triggers.Count; t = t<<1)
			{
				var flag = (Triggers) t;
				bool value = triggers.HasFlag (flag);

				// If a value changed, notify animator
				if (this.triggers.HasFlag (flag) != value)
					owner.anim.SetBool (flag.ToString (), value);
			}
			// Save new values
			this.triggers = triggers;
		}

		private void Awake () 
		{
			if (Net.IsServer)
			{
				owner = GetComponent<Hero> ();
				mods = new List<Mod> ();
				speedBuff = speedDebuff = 1f;
			}
			// Not useful at all on Client
			else if (Net.IsClient) Destroy (this);
		}

		#region CCs
		public void Add (string name, CCs cc, float duration = -1f, Triggers trigger = Triggers.None, bool unique = true) 
		{
			// If mod with same name already exists
			var mod = mods.SingleOrDefault (m=> m.name == name);
			if (mod != null)
			{
				// Do nothing if unique
				if (unique) return;
				// Reset otherwise
				else mod.timer = 0f;
			}
			else
			{
				// Create & add if it doesn't exist
				mod = new Mod (name, cc, duration, trigger, unique);
				mods.Add (mod);
			}
		}

		public void Remove (string name) 
		{
			var mod = mods.SingleOrDefault (m => m.name == name);
			if (mod != null) mods.Remove (mod);
		}

		public void CleanCC () 
		{
			// Remove non-block mods
			for (int i=mods.Count-1; i>=0; --i) 
			{
				if (mods[i].duration > 0f)
					mods.RemoveAt (i);
			}
			// Claen speed de-buff
			speedDebuff = 1f;
		}
		#endregion

		#region SPEED
		public void SpeedUp (float amount) 
		{
			if (amount == 0f) speedBuff = 1f;
			else speedBuff *= (1 + amount);

			UpdateSpeed ();
		}
		public void SpeedDown (float amount) 
		{
			if (amount == 0f) speedDebuff = 1f;
			else speedDebuff *= (1 - amount);

			UpdateSpeed ();
		}

		private void UpdateSpeed () 
		{
			// Buff has priority over de-buff
			if (speedBuff != 1f) owner.SpeedMul = speedBuff;
			else
			if (speedDebuff != 1f) owner.SpeedMul = speedDebuff;

			// If no buff is present
			else owner.SpeedMul = 1f;
		}
		#endregion
	}

	[Flags]
	public enum Triggers 
	{
		None,

		Hit			= 1<< 0,
		Scared		= 1<< 1,
		Squashed	= 1<< 2,
		Count		= 1<< 3
	}
}
