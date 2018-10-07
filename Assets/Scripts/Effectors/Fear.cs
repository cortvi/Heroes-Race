using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace.Effectors 
{
	public class Fear : MonoBehaviour 
	{
		public float stunTime;

		private void OnTriggerEnter (Collider other) 
		{
			if (other.tag != "Player") return;
			var h = other.GetComponent<Hero> ();
			h.blocks.Add ("Fear", CCs.All, stunTime);
			h.anim.SetTrigger ("Fear");
		}

		private void Awake () 
		{
			// Only present on Server
			if (NetworkClient.active)
				Destroy (this);
		}
	} 
}
