using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

namespace HeroesRace 
{
	public class Rotator : MonoBehaviour 
	{
		#region DATA
		public Vector3 axis;
		public float speed;
		private float dir;

		private short msgTpe; 
		#endregion

		void LateUpdate () 
		{
			// Rotate around
			float vel = speed * dir * Time.deltaTime;
			transform.Rotate (axis, vel, Space.Self);
		}

		public void SwitchDir (float dir) 
		{
			if (Net.IsServer)
			{
				// Change moving direction & notify Clients
				NetworkServer.SendToAll (msgTpe, new IntegerMessage ((int) dir));
				this.dir = dir;
			}
		}
		private void RPC_SwitchDir (NetworkMessage msg) 
		{
			// Recieve new rotation value & apply it
			dir = msg.ReadMessage<IntegerMessage> ().value;
		}

		private void Awake () 
		{
			msgTpe = Msg.Count++;
			Net.worker.client.RegisterHandler (msgTpe, RPC_SwitchDir);
		}
	} 
}
