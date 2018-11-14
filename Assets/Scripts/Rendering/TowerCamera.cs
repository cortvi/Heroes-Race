using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeroesRace 
{
	public class /* SERVER-ONLY */ TowerCamera : MonoBehaviour 
	{
		internal static TowerCamera i;
		internal HeroCamera tracking;

		private void Update () 
		{
			// ——— Allow switching Hero with keyboard ———
			if (Input.GetKeyDown (KeyCode.Alpha1) &&
				Net.players[0]) tracking.target = Net.players[0].pawn as Hero;

			if (Input.GetKeyDown (KeyCode.Alpha2) &&
				Net.players[1]) tracking.target = Net.players[1].pawn as Hero;

			if (Input.GetKeyDown (KeyCode.Alpha3) &&
				Net.players[2]) tracking.target = Net.players[2].pawn as Hero;
		}

		private void Awake () 
		{
			if (Net.isServer) 
			{
				tracking = Camera.main.gameObject.AddComponent<HeroCamera> ();
				i = this;
				print ("set up");
			}
		}
	} 
}
