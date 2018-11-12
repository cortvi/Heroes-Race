using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace.Effectors 
{
	public class Slow : CCBase 
	{
		[Range (0f, 1f)]
		public float amount;

		protected override void OnEnter (Hero hero) 
		{
			// Apply slow to Hero
			hero.mods.SpeedDown (amount);
		}
		protected override void OnExit (Hero hero) 
		{
			// Remove slow from Hero
			hero.mods.SpeedDown (0f);
		}
	} 
}
