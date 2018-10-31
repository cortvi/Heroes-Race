﻿using System.Collections;
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
			anim = slime.GetComponent<Animation> ()["Slime_10"];
			if (Net.isClient) anim.normalizedTime = syncTime;
		}
	} 
}
