using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

namespace HeroesRace 
{
	public static class Rpc 
	{
		#region DATA
		public const short MsgType = 150;
		private static Dictionary<string, Action> handlers;
		#endregion

		public static void Register (string name, Action handler) 
		{
			// Register call
			if (Net.IsClient) handlers.Add (name, handler);
		}

		public static void SendToAll (string name) 
		{
			// Send from Server to all Clients
			if (Net.IsServer)
			{
				var msg = new StringMessage (name);
				NetworkServer.SendToAll (MsgType, msg);
			}
		}

		public static void Recieve (NetworkMessage msg) 
		{
			// Call handler
			string name = msg.ReadMessage<StringMessage> ().value;
			handlers[name] ();
		}
	}
}
