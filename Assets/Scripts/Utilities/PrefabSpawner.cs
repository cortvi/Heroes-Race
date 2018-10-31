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

		// At Server
		IEnumerator Start () 
		{
			if (!Net.isServer) yield break;

			// Wait Delay time
			var mark = Time.time + delay;
			while (Time.time <= mark) yield return null;

			// Replace this object by Prefab
			var obj = Instantiate (prefab, transform.position, transform.rotation);
			NetworkServer.Spawn (obj);

			// Destroy it afterwards
			Destroy (gameObject);
		}

		// At Client
		private void Awake () 
		{
			// Make it invisible
			GetComponent<MeshRenderer> ().enabled = false;

			if (!Net.isClient) return;
			// On clients, register the prefab
			// for when it's spawned over the Net
			ClientScene.RegisterPrefab (prefab);

			// Destroy it afterwards
			Destroy (gameObject);
		}
	} 
}
