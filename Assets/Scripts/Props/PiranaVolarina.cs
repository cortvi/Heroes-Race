using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	public class PiranaVolarina : MonoBehaviour  
	{
		#region DATA
		[Info] public float spawnRate;
		private float spawnTimer;
		private bool done;

		private Transform wrapper;
		private static readonly Quaternion correction = Quaternion.Euler (0f, 0f, 90f);
		#endregion

		[ServerCallback]
		private void Update () 
		{
			// Spawn over net, passing info
			if (!done && spawnTimer >= spawnRate)
			{
				var next = Instantiate (this);
				NetworkServer.Spawn (next.gameObject);
				next.name = GetComponent<NetBehaviour> ().SharedName;
				next.spawnRate = spawnRate;

				// Just once
				done = true;
			}
			else spawnTimer += Time.deltaTime;

		}

		private void LateUpdate () 
		{
			// Correct animator rotation for all
			wrapper.rotation *= correction;
		}

		private void Start () 
		{
			if (Net.IsServer && spawnRate == 0f) 
			{
				// Calculted for the first Pirana, then it's passed on
				spawnRate = Random.Range (0.7f, 1.2f);
			}
			wrapper = transform.GetChild (0);
		}

		private void Destroy () 
		{
			// End of animation call
			NetworkServer.Destroy (gameObject);
		}
	}
}
