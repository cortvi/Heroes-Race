using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace.Effectors 
{
	public class Fear : CCBase 
	{
		public float stunTime;

		protected override void OnEnter (Hero hero) 
		{
			// Stop Hero movement in-situ && apply CC
			hero.driver.body.velocity = Vector3.zero;
			hero.driver.body.angularVelocity = Vector3.zero;

			hero.mods.Add ("Fear", CCs.Locomotion, stunTime, Triggers.Scared);
		}
	} 
}
