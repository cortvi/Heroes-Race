using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	public class NetGUI : NetworkDiscovery 
	{
		public override void OnReceivedBroadcast (string fromAddress, string data) 
		{
			fromAddress = fromAddress.Replace (":", "").Replace ("f", "");
			Net.worker.networkAddress = fromAddress;
			var c = Net.worker.StartClient ();
			StopBroadcast ();
			print (fromAddress);
			print (c.isConnected);
		}

		private void OnGUI () 
		{
			GUILayout.BeginArea (new Rect (10f, 10f, 200f, 100f));
			// If not chosen a net-role yet
			if (!Net.isClient && !Net.isServer)
			{
				if (isClient) GUILayout.Label ("Searching server...");
				else
				if (isServer)
				{
					GUILayout.Label ("Awaiting clients...");
					GUILayout.Label ("Clients connected: " + Net.users.Count);
				}
				#region START BROADCASTING
				else
				{
					if (GUILayout.Button ("Start server"))
					{
						Net.users = new List<User> (3);
						Net.worker.StartServer ();

						Initialize ();
						StartAsServer ();
					}
					else
					if (GUILayout.Button ("Start client"))
					{
						Initialize ();
						StartAsClient ();
					}
				} 
				#endregion
			}
			else // Once network is initialized
			{
				GUILayout.Label ("Holly molly");
			}
			GUILayout.EndArea ();
		}
	} 
}
