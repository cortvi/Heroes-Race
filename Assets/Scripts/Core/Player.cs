using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.SceneManagement;

namespace HeroesRace 
{
	public class /* CLIENT-ONLY */ Player : NetBehaviour 
	{
		#region CALLBACKS
		protected override void OnClientAuthority () 
		{
			Net.me = this;
			DontDestroyOnLoad (this);
		}
		#endregion
	}
}
