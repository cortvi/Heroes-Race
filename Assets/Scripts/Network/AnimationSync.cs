using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	public class AnimationSync : NetBehaviour 
	{
		[Range (0.001f, 0.5f)]
		public float sendRate;
		[Range (0f, 20f)]
		public int lerpFactor;
		public string syncedAnimation;

		private float sendTimer;
		private AnimationState anim;

		[ClientRpc (channel = 2)]
		private void Rpc_Sync (float syncTime) 
		{
			// Sync with Server
			if (lerpFactor != 0)
			{
				float lerp = Mathf.Lerp (anim.normalizedTime, syncTime, Time.deltaTime * lerpFactor);
				anim.normalizedTime = lerp;
			}
			else anim.normalizedTime = syncTime;
		}

		private void LateUpdate () 
		{
			if (sendTimer > sendRate)
			{
				Rpc_Sync (anim.normalizedTime);
				sendTimer = 0f;
			}
			else sendTimer += Time.deltaTime;
		}

		protected override void OnStart () 
		{
			// Get the reference for the animation state to be synced
			anim = GetComponentInChildren<Animation> ()[syncedAnimation];
		}
	} 
}
