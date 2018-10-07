using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace.Effectors 
{
	public class Slow : MonoBehaviour 
	{
		[Range (0f, 1f)]
		public float slowAmount;

		private void OnTriggerEnter (Collider other) 
		{
			if (other.tag != "Player") return;
			var h = other.GetComponent<Hero> ();
			h.SpeedMul *= 1 - slowAmount;
		}

		private void OnTriggerExit (Collider other) 
		{
			if (other.tag != "Player") return;
			var h = other.GetComponent<Hero> ();
			h.SpeedMul /= 1 - slowAmount;
		}

		private void Awake () 
		{
			// Only present on Server
			if (NetworkClient.active)
				Destroy (this);
		}
	} 
}
