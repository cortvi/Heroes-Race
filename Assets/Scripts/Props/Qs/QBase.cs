using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	public class QBase :  NetBehaviour 
	{
		// Due to possible server-client ID mismatch,
		// cache position to restore it after net spawn
		private Vector3 posCache;

		protected override void OnClientStart () 
		{
			transform.position = posCache;
			// This scripts should not exist on Clients
			Destroy (this);
		}

		protected override void OnClientAwake () 
		{
			posCache = transform.position;
			// Register self & delete all colliders!
			ClientScene.RegisterPrefab (gameObject);
		}
	}
}
