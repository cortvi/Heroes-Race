using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	[NetworkSettings (channel = 2, sendInterval = 0.01f)]
	public class AnimationSync : NetBehaviour 
	{
		#region DATA
		[Range (0f, 20f)]
		public int lerpFactor;
		public string syncedAnimation;
		private Animation anim;

		[SyncVar (hook = "Sync")]
		private float syncTime; 
		#endregion

		private void Sync (float syncTime) 
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
			syncTime = a.normalizedTime;
		}

		protected override void OnStart () 
		{
			// Get the reference for the animation state to be synced
			anim = GetComponentInChildren<Animation> ();
		}
	} 
}
