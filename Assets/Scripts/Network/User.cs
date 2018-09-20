using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/* This class holds persistent data for each player
 * when they connect for the first time. This way when
 * they disconnect and re-connect, authority will be
 * restored for all owned objects. */
namespace HeroesRace 
{
	public class /* SERVER-ONLY */ User
	{
		#region DATA + CTOR
		public readonly int ID;
		public readonly string IP;
		public NetworkConnection Conn;
		public Heroes playingAs;

		public User (NetworkConnection fromConn) 
		{
			Conn = fromConn;
			IP = Conn.address;
			ID = Conn.connectionId;
			playingAs = Heroes.NONE;

			Log.LowDebug (string.Format ("Added User {0} from {1}", ID, IP));
		}
		#endregion
	} 
}
