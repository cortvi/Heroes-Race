using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeroesRace 
{
	public class Namer : NetBehaviour 
	{
		protected override void OnStart () 
		{
			// This script just names the object
			Destroy (this);
		}
	} 
}
