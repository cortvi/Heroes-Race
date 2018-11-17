using System;
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

		private uint id;
		#endregion

		#region CALLBACKS
		void LateUpdate () 
		{
			// Rotate around
			float vel = speed * dir * Time.deltaTime;
			transform.Rotate (axis, vel, Space.Self);
		}

		private void Start () 
		{
			id = GetComponentInParent<NetworkIdentity> ().netId.Value;
			Rpc.Register ("GoRight" + id, () => SwitchDir (1f));
			Rpc.Register ("GoLeft" + id, () => SwitchDir (-1f));
		}

		public void SwitchDir (float dir) 
		{
			// Notify all Clients
			if (Net.IsServer)
			{
				if (dir == 1f) Rpc.SendToAll ("GoRight" + id);
				else
				if (dir == -1f) Rpc.SendToAll ("GoLeft" + id);
			}
			// Change moving direction
			this.dir = dir;
		}
		#endregion
	} 
}
