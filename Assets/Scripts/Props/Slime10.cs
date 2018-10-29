using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	[NetworkSettings (channel = 2, sendInterval = 0.1f)]
	public class Slime10 : NetBehaviour 
	{
		public GameObject slime;
		private AnimationState anim;
		[SyncVar] private float syncTime;

		[ServerCallback]
		private void LateUpdate () 
		{
			// Send sync time for any Player that connects
			syncTime = anim.normalizedTime;
		}

		protected override void OnStart () 
		{
			// Get some common references
			anim = slime.GetComponent<Animation> ()["Slime_10"];
			if (NetworkClient.active) anim.normalizedTime = syncTime;
		}
	} 
}
