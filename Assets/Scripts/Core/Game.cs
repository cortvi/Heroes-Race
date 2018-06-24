using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace HeroesRace 
{
	// This is the "Local players" class
	public class Game : NetBehaviour
	{
		#region DATA
		public override string SharedName 
		{
			get { return "Player"; }
		}
		[SyncVar] internal Heroes playingAs;
		#endregion

		#region HELPERS
		public enum Heroes 
		{
			NONE = -1,

			Espectador,
			Indiana,
			Harley,
			Harry,

			Count
		}

		[Server] public void SpawnHero (Heroes hero) 
		{
			playingAs = hero;
			// Instantiate Hero object & propagate over the Net
			var go = Instantiate (Resources.Load<Character> ("Prefabs/Heroes/" + hero.ToString ()));
			NetworkServer.SpawnWithClientAuthority (go.gameObject, connectionToClient);
		}
		#endregion
	} 
}
