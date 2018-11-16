﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	public class NetAnchor : NetBehaviour 
	{
		// Where Heroes will be attached
		public Transform anchor;

		protected void Attach (Hero hero, bool useDriver = false) 
		{
			var target = hero.transform;
			if (Net.IsServer) 
			{
				Rpc_Attaching (true, hero.gameObject);
				if (useDriver) target = hero.driver.transform;
			}
			target.SetParent (anchor, true);
		}

		protected void Dettach (Hero hero, bool useDriver = false) 
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

			if (attach) Attach (hero);
			else Dettach (hero);
		}
	} 
}
