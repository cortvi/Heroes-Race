using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace.Effectors 
{
	public class Slow : EffectorBase 
	{
		[Range (0f, 1f)]
		public float slowAmount;

		protected override void OnEnter (Hero hero) 
		{
			hero.SpeedMul *= 1 - slowAmount;
		}
		protected override void OnExit (Hero hero) 
		{
			hero.SpeedMul /= 1 - slowAmount;
		}
	} 
}
