using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	public partial class /* COMMON */ Player : NetBehaviour 
	{
		IPawn pawn;
	}

	public partial class /* SERVER */ Player 
	{
		[Command]
		public void Cmd_Input (string[] input) 
		{
			pawn.ProcessInput (input);
		}
	}

	public partial class /* CLIENT */ Player 
	{
		[ClientCallback]
		private void Update () 
		{
			if (isLocalPlayer) return;
			string[] input = pawn.GetInput ();
			Cmd_Input (input);
		}
	}
}

