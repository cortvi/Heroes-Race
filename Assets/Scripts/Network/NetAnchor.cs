using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	public class NetAnchor : NetBehaviour 
	{
		// Where Heroes will be attached
		public Transform anchor;

		public void Attach (Hero hero, bool useDriver) 
		{
			var target = hero.transform;
			if (Net.IsServer) 
			{
				Rpc_Attaching (true, hero.gameObject);
				if (useDriver) target = hero.driver.transform;
			}
			target.SetParent (anchor, true);
		}

		public void Dettach (Hero hero, bool useDriver) 
		{
			var target = hero.transform;
			if (Net.IsServer) 
			{
				Rpc_Attaching (false, hero.gameObject);
				if (useDriver) target = hero.driver.transform;
			}
			target.SetParent (null, true);
		}

		[ClientRpc]
		private void Rpc_Attaching (bool attach, GameObject heroGO) 
		{
			var hero = heroGO.GetComponent<Hero> ();

			// Always grab Hero on Client
			if (attach) Attach (hero, useDriver: false);
			else Dettach (hero, useDriver: false);
		}
	} 
}
