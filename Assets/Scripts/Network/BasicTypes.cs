using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

namespace HeroesRace 
{
	public class Rpc 
	{
		#region DATA
		public const short MsgType = (short)Codes.Courtain;
		private static Dictionary<int, Action> handlers;

		private static int ID;
		private readonly int id; 
		#endregion

		public Rpc () 
		{
			id = ID++;
			if (Net.IsClient && handlers == null)
				handlers = new Dictionary<int, Action> ();
		}

		public void Register (Action handler) 
		{
			// Register using ID (should be the same on both Client & Server
			if (Net.IsClient) handlers.Add (id, handler);
		}

		public void SendToAll () 
		{
			// Send from Server to all Clients
			if (Net.IsServer) NetworkServer.SendToAll (MsgType, new IntegerMessage (id));
		}

		public static void Recieve (NetworkMessage msg) 
		{
			// Call handler
			int id = msg.ReadMessage<IntegerMessage> ().value;
			handlers[id] ();
		}
	}

	public enum Codes : short 
	{
		// Server -> Client
		Courtain = 150,
		SimpleRPC = 151,
		Slime10 = 152
	}
}
