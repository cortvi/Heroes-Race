using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeroesRace 
{
	public class NetCourtain : NetBehaviour 
	{
		public void Open (bool state) 
		{
			// Set courtain open state
			Net.worker.courtain.SetBool ("Open", state);
		}

		protected override void OnStart () 
		{
			// Once active on Net, set reference to itself
			Courtain.net = this;
		}

		protected override void OnAwake () 
		{
			// Preserve during whole session
			DontDestroyOnLoad (gameObject);
		}
	} 
}
