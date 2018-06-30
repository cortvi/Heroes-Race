using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	public class NetGUI : NetworkDiscovery 
	{
		private bool ready;
		private void OnGUI () 
		{
			if (!showGUI) return;
			GUILayout.BeginArea (new Rect (10f, 10f, 200f, 100f));

			#region CONNECTION CONTROL
			if (isServer)
			{
				if (!ready)
				{
					GUILayout.Label ("Awaiting clients...");
					if (Net.users.Count == Net.UsersNeeded) ready = true;
				}
				else
				{
					GUILayout.Label ("All clients connected.");
					GUILayout.Label ("Clients connected: " + Net.users.Count);
					if (GUILayout.Button ("Go to selection"))
					{
						Net.worker.ServerChangeScene ("Menu");
					}
					else
					if (GUILayout.Button ("Go to tower"))
					{

					}
				}
			}
			else
			if (isClient)
			{
				if (Net.worker.client != null)
				{
					GUILayout.Label ("Connected to server");
					GUILayout.Label ("Status: " + Net.worker.client.isConnected);
					GUILayout.Label ("Server IP: " + Net.worker.client.serverIp);
				}
				else GUILayout.Label ("Searching server...");
			} 
			#endregion

			#region START BROADCASTING
			else
			{
				if (GUILayout.Button ("Start server"))
				{
					Initialize ();
					StartAsServer ();

					Net.worker.StartServer ();
					Net.isServer = true;
				}
				else
				if (GUILayout.Button ("Start client"))
				{
					Initialize ();
					StartAsClient ();
					Net.isClient = true;
				}
			} 
			#endregion

			GUILayout.EndArea ();
		}

		public override void OnReceivedBroadcast (string fromAddress, string data) 
		{
			Net.worker.networkAddress = fromAddress;
			if (Net.worker.client == null) Net.worker.StartClient ();
		}
	}
}
