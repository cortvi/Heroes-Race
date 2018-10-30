using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	public class Apisonadora : NetBehaviour 
	{
		#region DATA
		[Info] public float waitTime;
		internal SmartAnimator anim;

		public Material[] variants;
		private static int count;
		#endregion

		[ServerCallback]
		new IEnumerator Start () 
		{
			while (true) 
			{
				// Then wait given time
				float mark = Time.time + waitTime;
				while (Time.time <= mark) yield return null;

				// Repeat animation
				anim.SetTrigger ("Hit");
				yield return new WaitForSeconds (0.3f);

				// Wait until it finishes, then repeat loop
				while (!(anim.IsInState("Default") && anim[0].normalizedTime >= 0.3f))
					yield return null;
			}
		}

		protected override void OnAwake () 
		{
			if (NetworkServer.active)
			{
				waitTime = Random.Range (0.5f, 1.3f);
				anim = GetComponent<Animator> ().GoSmart (networked: true);
			}

			// Make each one look different
			if (count == 3) count = 0;
			var variant = variants[count++];
			GetComponentInChildren<Renderer> ().sharedMaterial = variant;
		}
	} 
}
