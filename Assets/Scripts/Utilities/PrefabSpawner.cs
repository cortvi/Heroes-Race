using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	public class PrefabSpawner : MonoBehaviour 
	{
		public GameObject prefab;
		public float delay;

		IEnumerator Start () 
		{
			if (!Net.IsServer) yield break;

			// Wait Delay time
			var mark = Time.time + delay;
			while (Time.time <= mark) yield return null;

			// Replace this object by Prefab
			var obj = Instantiate (prefab, transform.position, transform.rotation);
			NetworkServer.Spawn (obj);

			// Destroy it afterwards
			Destroy (gameObject);
		}

		private void Awake () 
		{
			// Make it invisible
			GetComponent<MeshRenderer> ().enabled = false;

			// Useless on Clients
			if (Net.IsClient) Destroy (gameObject);
		}
	} 
}
