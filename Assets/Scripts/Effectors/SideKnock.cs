using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace.Effectors 
{
	public class SideKnock : EffectorBase 
	{
		public float kickForce;
		public float upForce;

		protected override void OnEnter (Hero hero) 
		{
			// Stop Hero before knocking
			hero.driver.body.velocity = Vector3.zero;
			hero.driver.body.angularVelocity = Vector3.zero;

			// Apply computed force
			var force = KnockForce (hero.driver.transform);
			hero.driver.body.AddForceAtPosition (force, transform.position, ForceMode.VelocityChange);

			// Apply CC to Hero
			hero.mods.AddCC ("Knocked ", CCs.All, 1.5f, unique: false);
			hero.anim.SetTrigger ("Hit");
		}

		private void OnDrawGizmos () 
		{
			if (Net.players[0] && Net.players[0].pawn) 
			{
				var driver = (Net.players[0].pawn as Hero).driver.transform;
				var from = transform.position;
				var to = from + KnockForce (driver);
				Gizmos.DrawLine (from, to);
			}
		}

		private Vector3 KnockForce (Transform heroDriver) 
		{
			// Compare against hero position to get the force direction
			var transPos = heroDriver.InverseTransformPoint (transform.position);
			float sign = -Mathf.Sign (transPos.x);

			// Get matrix right and comput final force
			var kickDir = heroDriver.right * sign * kickForce;
			kickDir += Vector3.up * upForce;

			return kickDir;
		}
	} 
}
