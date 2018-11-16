using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

namespace HeroesRace 
{
	// Simple RPC call
	public class Rpc 
	{
		private readonly short msgType;
		private readonly Action handler;

		public Rpc (Action handler) 
		{
			msgType = Msg.Count++;
			NetworkServer.RegisterHandler (msgType, Recieve);
			this.handler = handler;
		}

		public void SendToAll () 
		{
			// Send from Server to all Clients
			NetworkServer.SendToAll (msgType, new EmptyMessage ());
		}

		private void Recieve (NetworkMessage msg) 
		{
			// Call handler
			handler ();
		}
	}

	// All net-msg codes
	public static class Msg 
	{
		public static short Count = 500;

		public const short Courtain = 150;
	}
}
