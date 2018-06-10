using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public static class Extensions 
{
	#region BEHAVIOUR
	private static object thisLock = new object ();
	public static T SpawnSingleton<T> () where T : Behaviour
	{
		// Keep it thread-safe
		lock (thisLock)
		{
			// Be sure there isn't any other on scene
			var intruder = Object.FindObjectOfType<T> ();
			if (intruder) throw new UnityException ("Requested type is already spawned on scene!");

			// Locate prefab
			var prefab = Resources.Load<T> ("Prefabs/" + typeof (T).Name);
			if (prefab != null)
			{
				var go = Object.Instantiate (prefab);
				go.name = "[Singleton] " + typeof (T).Name;
				Object.DontDestroyOnLoad (go);

				return go;
			}
			else throw new UnityException ("Prefab asset not found!");
		}
	}
	#endregion

	#region pfff
	public static IEnumerator WaitAnimator (this Animator a, string animation, int layer = 0)
	{
		while (a.GetCurrentAnimatorStateInfo (layer).IsName (animation))
			yield return null;
	} 
	#endregion
}
