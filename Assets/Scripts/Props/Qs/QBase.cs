using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	public class QBase :  NetBehaviour 
	{
		protected override void OnClientAwake () 
		{
			// Useless on Client
			Destroy (this);
		}
	}
}
