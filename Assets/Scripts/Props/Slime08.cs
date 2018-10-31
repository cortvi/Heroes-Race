using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	[NetworkSettings (channel = 2, sendInterval = 0.01f)]
	public class Slime08 : NetBehaviour 
	{
		[SyncVar] private float syncTime;
		private AnimationState anim;

		private void LateUpdate () 
		{
			if (Net.isClient) 
			{
				float lerp = Mathf.Lerp (anim.normalizedTime, syncTime, Time.deltaTime * 10f);
				anim.normalizedTime = lerp;
			}
			// Send sync time for any player that connects
			else if (Net.isServer) syncTime = anim.normalizedTime;
		}

		protected override void OnStart () 
		{
			// Get some common references
			anim = GetComponent<Animation> ()["Slime_08"];
			if (Net.isClient) anim.normalizedTime = syncTime;
		}
	} 
}
