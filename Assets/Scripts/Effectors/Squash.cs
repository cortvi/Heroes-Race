using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeroesRace.Effectors 
{
	public class Squash : EffectorBase 
	{
		protected override void OnEnter (Hero hero) 
		{
			hero.anim.SetTrigger ("Squash");
			hero.blocks.Add ("Squashed", CCs.All, 1.5f);
		}
	} 
}
