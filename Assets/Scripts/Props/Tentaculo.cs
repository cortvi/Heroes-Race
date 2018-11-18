using HeroesRace.Effectors;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeroesRace 
{
	public class Tentaculo : NetAnchor 
	{
		private TentaculoGrab grab;

		private void Release () 
		{
			var hero = grab.grabbed;
			var driver = hero.driver.transform;

			// Set Hero free & re-enable Driver
			Dettach (hero, useDriver: false);
			hero.driver.gameObject.SetActive (true);

			// Align Driver with new Hero location
			var pos = driver.position;
			pos.y = hero.transform.position.y;
			driver.position = pos;
			driver.rotation = Quaternion.LookRotation (hero.transform.position);

			// Apply CC to Hero
			hero.mods.Add ("Raped", CCs.All, 1f);
		}

		protected override void OnAwake () 
		{
			if (Net.IsServer) grab = GetComponentInChildren<TentaculoGrab> ();
		}
	} 
}
