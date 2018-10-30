using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	public class QBase :  NetBehaviour 
	{
		protected override void OnClientStart () 
		{
			// This scripts should not exist on Clients
			Destroy (this);
		}

		protected override void OnClientAwake () 
		{
			// Register self & delete all colliders!
			ClientScene.RegisterPrefab (gameObject);
		}
	}
}
