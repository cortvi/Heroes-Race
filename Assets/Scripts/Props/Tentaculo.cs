using HeroesRace.Effectors;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	public class Tentaculo : NetAnchor 
	{
		private TentaculoGrab grab;

		[ServerCallback]
		private void Release () 
		{
			if (!grab.grabbed) return;
			var hero = grab.grabbed;
			var driver = hero.driver;

			// Set Hero free & re-enable Driver
			Dettach (hero, useDriver: false);
			driver.enabled = true;
			grab.grabbed = null;

			// Align Driver with new Hero location
			driver.transform.position = new Vector3 (0f, hero.transform.position.y, 0f);
			// Align Driver rotation to projected Hero position
			var hPos = hero.transform.position; /* */ hPos.y = 0f;
			driver.transform.rotation = Quaternion.LookRotation (hPos);

			// Set Hero on Air
			hero.driver.SwitchFriction (false);
			hero.OnAir = true;

			// Apply CC to Hero
			hero.mods.Add ("Raped", ~CCs.Rotating, 1f);
		}

		protected override void OnAwake () 
		{
			if (Net.IsServer) grab = GetComponentInChildren<TentaculoGrab> ();
		}
	} 
}
