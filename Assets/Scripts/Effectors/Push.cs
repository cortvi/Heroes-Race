using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeroesRace.Effectors 
{
	public class Push : CCBase 
	{
		protected override void OnEnter (Hero hero) 
		{
			// Add CC & suppress speed
			hero.mods.Add ("Pushed", CCs.All, 1.15f, Triggers.Hit);
			hero.driver.body.angularVelocity = Vector3.zero;
			hero.driver.body.velocity = Vector3.zero;

			// Throw through hole
			StartCoroutine (PushHero (hero.driver.capsule));
		}

		private IEnumerator PushHero (CapsuleCollider capsule) 
		{
			// Move Driver capsule to make Hero fall through hole
			var center = capsule.center;
			float iZ = center.z;
			// Variant-2 holes are further away
			float tZ = name.Contains ("2") ? 30.75f : 30.5f;

			float step = 0f;
			float duration = 0.75f;
			while (step <= 1f) 
			{
				float pow = Mathf.Pow (step, 0.6f);
				float lerp = Mathf.Lerp (iZ, tZ, pow);

				center.z = lerp;
				capsule.center = center;

				yield return null;
				step += Time.deltaTime / duration;
			}
			yield return new WaitForSeconds (0.15f);
			// Restore original Driver pos
			center.z = iZ;
			capsule.center = center;
		}
	} 
}
