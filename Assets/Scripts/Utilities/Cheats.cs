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

		public static void Grant (params string[] args) 
		{
			var power = args[0].CapitalizeFirst ().EnumParse<PowerUp> ();
			if (Net.isClient)
			{
				// Grants Client said power
				Net.me.Cmd_GrantPower (power);
			}
			else
			if (Net.isServer)
			{
				// Grants said Client said power
				int player = int.Parse (args[1]);
				(Net.players[player-1].pawn as Hero).power = power;
			}
		}
	} 
}
