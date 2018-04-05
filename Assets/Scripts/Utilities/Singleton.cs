using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Singleton 
{
	private static object thisLock = new object ();
	public static T Spawn <T> (string prefabToLoad) where T : Behaviour
	{
		/// Keep it thread-safe
		lock (thisLock)
		{
			/// Be sure there isn't any other on scene
			var intruder = Object.FindObjectOfType<T> ();
			if (intruder) throw new UnityException ("Requested type is already spawned on scene!");

			/// Locate prefab
			var prefab = Resources.Load<T> (prefabToLoad);
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
}
