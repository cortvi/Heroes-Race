/// Adapted from: https://gist.github.com/marsh12th/fdf06d19d3689b375411761e47befb4c#file-smartanimator-cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SmartAnimator : Object
{
	#region DATA
	public Animator Animator { get; private set; }
	public NetworkAnimator NetAnimator { get; private set; }
	private bool networked;

	private Dictionary<string, Param<float>> floats;
	private Dictionary<string, Param<bool>> bools;
	private Dictionary<string, Param<int>> ints;
	private Dictionary<string, int> triggers;

	// This class is a wrapper for each animator parameter
	private class Param<T> where T : struct
	{
		public T value;
		public int hash;
		public bool isDrivenByCurve;

		public static implicit operator T (Param<T> param)
		{ return param.value; }
	}
	#endregion

	/// <summary>
	/// Creates a wrapper around given Animator and makes a cache of all
	/// its parameters for faster checks and modifications.
	/// </summary>
	public SmartAnimator (Animator animator, bool networked = false) 
	{
		// Null check
		if (!animator)
		{
			Debug.LogError ("Provided Animator is null!", this);
			return;
		}
		// Initialize dictionaries
		floats = new Dictionary<string, Param<float>> ();
		bools = new Dictionary<string, Param<bool>> ();
		ints = new Dictionary<string, Param<int>> ();
		triggers = new Dictionary<string, int> ();

		Animator = animator;
		NetAnimator = animator.GetComponent<NetworkAnimator> ();
		this.networked = networked;

		// Loops through animator parameters
		// Default (inspector) values are stored in the cache
		// now and then updated with each Set function.
		// This way we never have to call Animator.Get
		foreach (var p in animator.parameters) 
		{
			if (p.type == AnimatorControllerParameterType.Float)
			{
				var drivenByCurve = animator.IsParameterControlledByCurve (p.nameHash);
				var param = new Param<float> { hash = p.nameHash, value = p.defaultFloat, isDrivenByCurve = drivenByCurve };
				floats.Add (p.name, param);
			}
			else
			if (p.type == AnimatorControllerParameterType.Bool)
			{
				var drivenByCurve = animator.IsParameterControlledByCurve (p.nameHash);
				var param = new Param<bool> { hash = p.nameHash, value = p.defaultBool, isDrivenByCurve = drivenByCurve };
				bools.Add (p.name, param);
			}
			else
			if (p.type == AnimatorControllerParameterType.Int)
			{
				var drivenByCurve = animator.IsParameterControlledByCurve (p.nameHash);
				var param = new Param<int> { hash = p.nameHash, value = p.defaultInt, isDrivenByCurve = drivenByCurve };
				ints.Add (p.name, param);
			}
			else
			if (p.type == AnimatorControllerParameterType.Trigger) triggers.Add (p.name, p.nameHash);
		}
	}

	public bool IsInState (string name, bool trueInTransition = false, int layer = 0) 
	{
		if (Animator.IsInTransition (layer) && !trueInTransition)
			return false;

		var info = Animator.GetCurrentAnimatorStateInfo (layer);
		return info.IsName (name);
	}

	#region GETTERS
	/* Find parameter by given name and return its value.
	 * (parameters driven by curves aren't stored in cache) */

	public float GetFloat (string id) 
	{
		Param<float> cache;
		if (floats.TryGetValue (id, out cache))
		{
			if (cache.isDrivenByCurve) return Animator.GetFloat (cache.hash);
			else return cache;
		}
		else
		{
			Debug.LogError ("Can't find parameter, returning -1", this);
			return -1f;
		}
	}
	public bool GetBool (string id) 
	{
		Param<bool> cache;
		if (bools.TryGetValue (id, out cache))
		{
			if (cache.isDrivenByCurve) return Animator.GetBool (cache.hash);
			else return cache;
		}
		else
		{
			Debug.LogError ("Can't find parameter, returning false", this);
			return false;
		}
	}
	public int GetInt (string id) 
	{
		Param<int> cache;
		if (ints.TryGetValue (id, out cache))
		{
			if (cache.isDrivenByCurve) return Animator.GetInteger (cache.hash);
			else return cache;
		}
		else
		{
			Debug.LogError ("Can't find parameter, returning -1", this);
			return -1;
		}
	}
	#endregion

	#region SETTERS
	/* Find parameter by given name and set its value.
	 * Then update cache to avoid later checks. */

	public void SetFloat (string id, float value) 
	{
		Param<float> cache;
		if (floats.TryGetValue (id, out cache))
		{
			if (cache != value)
			{
				Animator.SetFloat (cache.hash, value);
				cache.value = value;
			}
		}
		else Debug.LogError ("Can't find parameter!", this);
	}
	public void SetBool (string id, bool value) 
	{
		Param<bool> cache;
		if (bools.TryGetValue (id, out cache))
		{
			if (cache != value)
			{
				Animator.SetBool (cache.hash, value);
				cache.value = value;
			}
		}
		else Debug.LogError ("Can't find parameter!", this);
	}
	public void SetInt (string id, int value) 
	{
		Param<int> cache;
		if (ints.TryGetValue (id, out cache))
		{
			if (cache != value)
			{
				Animator.SetInteger (cache.hash, value);
				cache.value = value;
			}
		}
		else Debug.LogError ("Can't find parameter!", this);
	}

	public void SetTrigger (string id, bool reset = false) 
	{
		int hash;
		if (triggers.TryGetValue (id, out hash))
		{
			if (reset) Animator.ResetTrigger (hash);
			else
			{
				if (networked)  NetAnimator.SetTrigger (hash);
				else			Animator.SetTrigger (hash);
			}
		}
		else Debug.LogError ("Can't find parameter!", this);
	}
	#endregion
}
