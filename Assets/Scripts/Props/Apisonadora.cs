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

		new IEnumerator Start () 
		{
			UpdateName ();
			while (true) 
			{
				// Then wait given time
				float mark = Time.time + waitTime;
				while (Time.time <= mark) yield return null;

				// Repeat animation
				anim.SetTrigger ("Hit");
				yield return new WaitForSeconds (0.2f);

				// Wait until it finished to repeat loop
				while (!(anim.IsInState("Default") && anim[0].normalizedTime < 1f))
					yield return null;
			}
		}

		protected override void OnAwake () 
		{
			// Make each one look different
			if (count == 3) count = 0;
			var variant = variants[count++];
			GetComponentInChildren<Renderer> ().sharedMaterial = variant;

			if (NetworkServer.active) 
			{
				anim = GetComponent<Animator> ().GoSmart (networked: true);
				waitTime = Random.Range (0.35f, 1.3f);
			}
		}
	} 
}
