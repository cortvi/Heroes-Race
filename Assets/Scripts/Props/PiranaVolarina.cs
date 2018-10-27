using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	[NetworkSettings (channel = 2)]
	public class PiranaVolarina : NetBehaviour 
	{
		#region DATA
		[Info] public float spawnTime;
		private float spawnMark;
		private bool done;

		[SyncVar] private float syncTime;
		private AnimationState anim;

		private Transform wrapper;
		private Quaternion correction; 
		#endregion

		private void LateUpdate () 
		{
			// Correct animator rotation for all
			wrapper.rotation *= correction;

			if (NetworkServer.active) 
			{
				syncTime = anim.normalizedTime;
				if (!done && Time.time >= spawnMark)
				{
					// Spawn over net, passing spawn value
					var next = Instantiate (this);
					NetworkServer.Spawn (next.gameObject);
					next.SharedName = SharedName;
					next.spawnTime = spawnTime;

					// Just once
					done = true;
				}
			}
		}

		protected override void OnStart () 
		{
			// Get some common references
			anim = GetComponent<Animation> ()["Fly"];
			correction = Quaternion.Euler (0f, 0f, 90f);
			wrapper = transform.GetChild (0);

			if (NetworkServer.active) 
			{
				// Calculted for the first Pirana, then it's passed on
				if (spawnTime == 0f) spawnTime = Random.Range (0.8f, 1.3f);
				spawnMark = Time.time + spawnTime;
			}
			else anim.normalizedTime = syncTime;
		}

		private void Destroy () 
		{
			// End of animation call
			NetworkServer.Destroy (gameObject);
		}
	}
}
