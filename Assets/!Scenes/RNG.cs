using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class RNG : MonoBehaviour 
{
	public int loop = 10000;
	readonly Stopwatch watch = new Stopwatch ();

	void OnEnable () 
	{
		// Test
		TimeSpan unity = UnityMethod ();
		watch.Reset ();
		TimeSpan custom = CustomMethod ();

		// Print results
		UnityEngine.Debug.Log ("Unity method:  " + unity, gameObject);
		print ("Custom method: " + custom);
	}

	TimeSpan CustomMethod () 
	{
		watch.Start ();
		var rng = new StopWatchRNG ();
		for (int i = 0; i != loop; i++)
		{
			rng.Get ();
		}
		watch.Stop ();
		return watch.Elapsed;
	}

	TimeSpan UnityMethod () 
	{
		float value;

		watch.Start ();
		for (int i = 0; i != loop; i++)
		{
			value = UnityEngine.Random.value;
		}
		watch.Stop ();
		return watch.Elapsed;
	}
}

public class StopWatchRNG 
{
	readonly Stopwatch watch = new Stopwatch ();

	public StopWatchRNG () { watch.Start (); }
	~StopWatchRNG () { watch.Stop (); }

	public int Get (int range = 10) 
	{
		return (int) watch.ElapsedTicks % range;
	}
}
