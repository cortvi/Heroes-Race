using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	internal static class Cheats 
	{
		public static void Print (params string[] words) 
		{
			var builder = new StringBuilder ();
			foreach (var w in words)
			{
				builder.Append (w);
				builder.Append (' ');
			}
			Log.Debug (builder.ToString ());
		}

		public static void Power (params string[] args) 
		{
			if (NetworkClient.active)
			{
				// Grants Client said power
				var power = args[0].EnumParse<PowerUp> ();
				Net.me.Cmd_GrantPower (power);
			}
			else
			if (NetworkServer.active)
			{
				// Grants said Client said power
				var power = args[0].EnumParse<PowerUp> ();
				int player = int.Parse (args[1]);

				(Net.players[player].pawn as Hero).power = power;
			}
		}
	} 
}
