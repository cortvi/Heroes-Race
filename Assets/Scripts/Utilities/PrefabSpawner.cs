using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	public class PrefabSpawner : MonoBehaviour 
	{
		// El Prefab a colocar
		public GameObject prefab;
		public float delay;

		IEnumerator Start () 
		{
			// Make it invisible
			GetComponent<MeshRenderer> ().enabled = false;

			if (NetworkServer.active)
			{
				// Wait Delay time
				var mark = Time.time + delay;
				while (Time.time <= mark) yield return null;

				// Replace this object by Prefab
				var obj = Instantiate (prefab, transform.position, transform.rotation);
				NetworkServer.Spawn (obj);
			}
			else
			if (NetworkClient.active)
			{
				// On clients, register the prefab
				// for when it's spawned over the Net
				ClientScene.RegisterPrefab (prefab);
			}

			// Destroy it afterwards
			Destroy (gameObject);
		}
	} 
}
