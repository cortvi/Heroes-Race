using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	public class NetGUI : NetworkDiscovery 
	{
		private void OnGUI () 
		{
			if (!showGUI) return;
			GUILayout.BeginArea (new Rect (10f, 10f, 200f, 100f));

			#region CONNECTION CONTROL
			if (NetworkServer.active) 
			{
				if (Net.users.Count != Net.UsersNeeded) 
				{
					GUILayout.Label ("Awaiting clients...");
					GUILayout.Label ("Clients connected: " + Net.users.Count);
				}
				else
				{
					GUILayout.Label ("All clients connected.");
					GUILayout.Label ("Clients connected: " + Net.users.Count);

					// Scene managing options
					if (GUILayout.Button ("Go to selection"))
						Net.worker.ServerChangeScene ("Selection");
					else
					if (GUILayout.Button ("Go to tower"))
						Net.worker.ServerChangeScene ("Tower");
				}
			}
			else
			if (isClient) 
			{
				if (Net.worker.client != null) 
				{
					GUILayout.Label ("Connected to server");
					GUILayout.Label ("Ready status: " + ClientScene.ready);
					if (GUILayout.Button ("[X] STOP CLIENT")) Net.worker.StopClient ();
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
				}
				else
				if (GUILayout.Button ("Start client")) 
				{
					Initialize ();
					StartAsClient ();
				}
			} 
			#endregion

			GUILayout.EndArea ();
		}

		public override void OnReceivedBroadcast (string fromAddress, string data) 
		{
            Log.Info ("Server found");
			Net.worker.networkAddress = fromAddress;
			if (Net.worker.client == null) Net.worker.StartClient ();
		}
	}
}
