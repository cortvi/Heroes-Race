/// Adapted from: https://gist.github.com/cortvi/fdf06d19d3689b375411761e47befb4c#file-smartanimator-cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	public sealed class SmartAnimator 
	{
		#region IDXR + DATA + CTOR
		public AnimatorStateInfo this[int layer] 
		{
			get { return Animator.GetCurrentAnimatorStateInfo (layer); }
		}

		public Animator Animator { get; private set; }
		public NetworkAnimator NetAnimator { get; private set; }
		private readonly bool isNetworked;
		private bool? drivenByNetwork;

		// These dictionaries store the cached values for all the parameters
		private Dictionary<string, Param<float>> floats;
		private Dictionary<string, Param<bool>> bools;
		private Dictionary<string, Param<int>> ints;
		private Dictionary<string, int> triggers;

		// This class is a wrapper for each animator parameter
		private class Param<TParam> where TParam : struct
		{
			public int hashName;
			public bool isDrivenByCurve;
//			public bool isDrivenBytNetwork;
			public TParam value;

			public static implicit operator TParam (Param<TParam> param)
			{ return param.value; }
		}

		/// <summary>
		/// Creates a wrapper around given Animator and makes a cache of all
		/// its parameters for faster checks and modifications.
		/// </summary>
		public SmartAnimator (Animator animator, bool networked = false) 
		{
			// Null check
			if (!animator)
			{
				Debug.LogError ("Provided Animator is null!");
				return;
			}

			// Initialize dictionaries
			floats = new Dictionary<string, Param<float>> ();
			bools = new Dictionary<string, Param<bool>> ();
			ints = new Dictionary<string, Param<int>> ();
			triggers = new Dictionary<string, int> ();

			Animator = animator;
			if (networked)
			{
				NetAnimator = animator.GetComponent<NetworkAnimator> ();
				isNetworked = true;
			}

			/* Loops through animator parameters:
			 * Default (inspector) values are stored in the cache,
			 * then updated with each Set function. This way
			 * we never have to call Animator.Get */
			for (int i = 0; i != animator.parameterCount; i++)
			{
				var p = animator.parameters[i];
				if (p.type == AnimatorControllerParameterType.Float)
				{
					bool drivenByCurve = animator.IsParameterControlledByCurve (p.nameHash);
					var param = new Param<float>
					{
						hashName = p.nameHash,
						value = p.defaultFloat,
						isDrivenByCurve = drivenByCurve
					};
					floats.Add (p.name, param);
				}
				else
				if (p.type == AnimatorControllerParameterType.Bool)
				{
					bool drivenByCurve = animator.IsParameterControlledByCurve (p.nameHash);
					var param = new Param<bool>
					{
						hashName = p.nameHash,
						value = p.defaultBool,
						isDrivenByCurve = drivenByCurve
					};
					bools.Add (p.name, param);
				}
				else
				if (p.type == AnimatorControllerParameterType.Int)
				{
					bool drivenByCurve = animator.IsParameterControlledByCurve (p.nameHash);
					var param = new Param<int>
					{
						hashName = p.nameHash,
						value = p.defaultInt,
						isDrivenByCurve = drivenByCurve
					};
					ints.Add (p.name, param);
				}
				else
				if (p.type == AnimatorControllerParameterType.Trigger)
					triggers.Add (p.name, p.nameHash);
			}
		}
		#endregion

		#region UTILS
		public bool IsInState (string name, bool countTransition = false, int layer = 0) 
		{
			if (Animator.IsInTransition (layer) && !countTransition) 
				return false;

			var info = Animator.GetCurrentAnimatorStateInfo (layer);
			return info.IsName (name);
		}

		private bool IsDrivenByNetwork () 
		{
			if (drivenByNetwork == null) 
			{
				if (isNetworked) 
				{
					if (NetAnimator.isServer)
						drivenByNetwork = NetAnimator.localPlayerAuthority;
					else
					if (NetAnimator.isClient)
						drivenByNetwork = ! NetAnimator.hasAuthority;

					// Otherwise Network hasn't been initialized yet
					else return false;
				}
				else drivenByNetwork = false;
			}
			return (bool) drivenByNetwork;
		}
		#endregion

		#region GETTERS

		/* Find parameter by given name and return its value.
		 * (parameters driven by curves aren't stored in cache) */

		public bool GetBool (string id) 
		{
			#if (UNITY_EDITOR || DEVELOPMENT_BUILD || CHECK_SMART_ANIMATOR) && !AVOID_SMART_ANIMATOR_CHECK
			Param<bool> cache;
			if (bools.TryGetValue (id, out cache))
			{
				if (cache.isDrivenByCurve || IsDrivenByNetwork ())
					return Animator.GetBool (cache.hashName);
				else
					return cache;
			}
			else
			{
				Debug.LogError ("Can't find parameter, returning false", Animator);
				return false;
			}
			#else
			Param<bool> cache = bools[id];
			if (cache.isDrivenByCurve || IsDrivenByNetwork ())
				return Animator.GetBool (cache.hashName);
			else
				return cache;
			#endif
		}

		public float GetFloat (string id) 
		{
			#if (UNITY_EDITOR || DEVELOPMENT_BUILD || CHECK_SMART_ANIMATOR) && !AVOID_SMART_ANIMATOR_CHECK
			Param<float> cache;
			if (floats.TryGetValue (id, out cache))
			{
				if (cache.isDrivenByCurve || IsDrivenByNetwork ())
					return Animator.GetFloat (cache.hashName);
				else
					return cache;
			}
			else
			{
				Debug.LogError ("Can't find parameter, returning -1", Animator);
				return -1f;
			}
			#else
			// wrong on porpouse, if error hits, this works!!
			Param<floatf> cache = floats[id];
			if (cache.isDrivenByCurve || IsDrivenByNetwork ())
				return Animator.GetFloat (cache.hashName);
			else
				return cache;
			#endif
		}

		public int GetInt (string id) 
		{
			#if (UNITY_EDITOR || DEVELOPMENT_BUILD || CHECK_SMART_ANIMATOR) && !AVOID_SMART_ANIMATOR_CHECK
			Param<int> cache;
			if (ints.TryGetValue (id, out cache))
			{
				if (cache.isDrivenByCurve || IsDrivenByNetwork ())
					return Animator.GetInteger (cache.hashName);
				else
					return cache;
			}
			else
			{
				Debug.LogError ("Can't find parameter, returning -1", Animator);
				return -1;
			}
			#else
			Param<int> cache = ints[id];
			if (cache.isDrivenByCurve || IsDrivenByNetwork ())
				return Animator.GetInteger (cache.hashName);
			else
				return cache;
			#endif
		}
#endregion

		#region SETTERS

		/* Find parameter by given name and set its value.
		 * Then update cache to avoid later checks. */

		public bool SetBool (string id, bool value) 
		{
			#if (UNITY_EDITOR || DEVELOPMENT_BUILD || CHECK_SMART_ANIMATOR) && !AVOID_SMART_ANIMATOR_CHECK
			Param<bool> cache;
			if (bools.TryGetValue (id, out cache))
			{
				Animator.SetBool (cache.hashName, value);
				cache.value = value;
			}
			else Debug.LogError ("Can't find parameter!", Animator);
			return cache.value;
			#else
			Param<bool> cache = bools[id];
			Animator.SetBool (cache.hashName, value);
			cache.value = value;
			#endif
		}

		public float SetFloat (string id, float value) 
		{
			#if (UNITY_EDITOR || DEVELOPMENT_BUILD || CHECK_SMART_ANIMATOR) && !AVOID_SMART_ANIMATOR_CHECK
			Param<float> cache;
			if (floats.TryGetValue (id, out cache))
			{
				Animator.SetFloat (cache.hashName, value);
				cache.value = value;
			}
			else Debug.LogError ("Can't find parameter!", Animator);
			return cache.value;
			# else
			Param<float> cache = floats[id];
			Animator.SetFloat (cache.hashName, value);
			cache.value = value;
			#endif
		}
		public float IncrementFloat (string id, float delta) 
		{
			float value = GetFloat (id);
			return SetFloat (id, value + delta);
		}

		public int SetInt (string id, int value) 
		{
			#if (UNITY_EDITOR || DEVELOPMENT_BUILD || CHECK_SMART_ANIMATOR) && !AVOID_SMART_ANIMATOR_CHECK
			Param<int> cache;
			if (ints.TryGetValue (id, out cache))
			{
				Animator.SetInteger (cache.hashName, value);
				cache.value = value;
			}
			else Debug.LogError ("Can't find parameter!", Animator);
			return cache.value;
			#else
			Param<int> cache = ints[id];
			Animator.SetInteger (cache.hashName, value);
			cache.value = value;
			#endif
		}
		public int IncrementInt (string id, int delta) 
		{
			int value = GetInt (id);
			return SetInt (id, value + delta);
		}

		public void SetTrigger (string id, bool reset = false) 
		{
			#if (UNITY_EDITOR || DEVELOPMENT_BUILD || CHECK_SMART_ANIMATOR) && !AVOID_SMART_ANIMATOR_CHECK
			int hash;
			if (triggers.TryGetValue (id, out hash))
			{
				if (reset) Animator.ResetTrigger (hash);
				else
				{
					// Apparently you need to call it twice for network :/
					if (isNetworked) NetAnimator.SetTrigger (hash);
					Animator.SetTrigger (hash);
				}
			}
			else Debug.LogError ("Can't find parameter!", Animator);
			#else
			int hash = triggers[id];
			if (reset) Animator.ResetTrigger (hash);
			else
			{
				if (isNetworked) NetAnimator.SetTrigger (hash);
				Animator.SetTrigger (hash);
			}
			#endif
		}
		#endregion
	} 
}
