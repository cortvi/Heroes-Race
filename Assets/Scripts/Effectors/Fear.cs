using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace.Effectors 
{
	public class Fear : EffectorBase 
	{
		public float stunTime;

		protected override void OnEnter (Hero hero) 
		{
			hero.blocks.Add ("Fear", CCs.All, stunTime);
			hero.anim.SetTrigger ("Fear");

			// Stop Hero movement in-situ
			hero.driver.body.velocity = Vector3.zero;
			hero.driver.body.angularVelocity = Vector3.zero;
		}
	} 
}
