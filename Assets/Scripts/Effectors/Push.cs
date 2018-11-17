using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeroesRace.Effectors 
{
	public class Push : CCBase 
	{
		protected override void OnEnter (Hero hero) 
		{
			hero.mods.Add ("Pushed", CCs.All, 2f, Triggers.Hit);
			StartCoroutine (PushHero (hero.driver.capsule));
		}

		private IEnumerator PushHero (CapsuleCollider capsule) 
		{
			// Move Driver capsule to make Hero fall through hole
			var center = capsule.center;
			float iZ = center.z;
			float tZ = 30.5f;

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
			// Wait until Hero is in hole (out of side)
			yield return new WaitForSeconds (0.5f);
			center.z = iZ;
			capsule.center = center;
		}
	} 
}
