using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	[NetworkSettings (channel = 2)]
	public class MiniSlime : NetBehaviour 
	{
		[SyncVar] private float syncTime;
		private AnimationState anim;

		[ServerCallback]
		private void LateUpdate () 
		{
			// Send sync time for any player that connects
			syncTime = anim.normalizedTime;
		}

		protected override void OnStart () 
		{
			// Get some common references
			anim = GetComponent<Animation> ()["Slime_08"];
			if (NetworkClient.active) anim.normalizedTime = syncTime;
		}
	} 
}
