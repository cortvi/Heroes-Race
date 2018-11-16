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
		private Animation anim;

		[ClientRpc (channel = 2)]
		private void Rpc_Sync (float syncTime) 
		{
			if (!anim) return;
			var a = anim[syncedAnimation];

			// Sync with Server
			if (lerpFactor != 0) 
			{
				float lerp = Mathf.Lerp (a.normalizedTime, syncTime, Time.deltaTime * lerpFactor);
				a.normalizedTime = lerp;
			}
			else if (a) a.normalizedTime = syncTime;
		}

		[ServerCallback]
		private void LateUpdate () 
		{
			var a = anim[syncedAnimation];
			if (sendTimer > sendRate)
			{
				Rpc_Sync (a.normalizedTime);
				sendTimer = 0f;
			}
			else sendTimer += Time.deltaTime;
		}

		protected override void OnStart () 
		{
			// Get the reference for the animation state to be synced
			anim = GetComponentInChildren<Animation> ();
		}
	} 
}
