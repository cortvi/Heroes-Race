using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeroesRace 
{
	public class CCStack 
	{
		#region DATA + CTOR + IDXER
		private readonly NetBehaviour owner;
		private readonly Dictionary<string, CCs> collection;
		private CCs summary;

		public CCStack (NetBehaviour owner) 
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
		public void Update () 
		{
			summary = CCs.None;
			foreach (var e in collection) 
				summary = summary.SetFlag (e.Value);
		}

		public void Add (string name, CCs cc, float duration = -1f) 
		{
			collection.Add (name, cc);
			if (duration > 0f)
				owner.StartCoroutine (RemoveAfter (this, name, duration));
		}

		public void Remove (string name) 
		{
			if (collection.ContainsKey (name))
				collection.Remove (name);
		}
		#endregion

		#region HELPERS
		public static IEnumerator RemoveAfter (CCStack stack, string name, float duration) 
		{
			float release = Time.time + duration;
			while (Time.time < release) yield return null;
			stack.collection.Remove (name);
		}
		#endregion
	}

	[Flags] public enum CCs 
	{
		Moving		= 1<< 0, 
		Rotating	= 1<< 1, 
		Jumping		= 1<< 2,

		// ——— Specials ———
		Locomotion = Moving | Rotating | Jumping,
		None = 0
	}
}
