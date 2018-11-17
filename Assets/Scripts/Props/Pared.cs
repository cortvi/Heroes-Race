using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeroesRace 
{
	public class Pared : NetBehaviour 
	{
		private SmartAnimator anim;

		private void OnTriggerEnter (Collider other) 
		{
			if (other.tag != "Player") return;
			if (anim.Animator.IsInTransition (0)) return;

			anim.SetTrigger ("Push");
		}

		protected override void OnAwake () 
		{
			if (Net.IsServer)
			{
				anim = GetComponent<Animator> ().GoSmart (networked: true);
				anim.NetAnimator.SetParameterAutoSend (0, true);
			}
			else
			// Useless in Clients
			if (Net.IsClient) Destroy (this);
		}
	} 
}
