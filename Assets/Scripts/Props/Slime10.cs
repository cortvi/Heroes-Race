using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

namespace HeroesRace 
{
	public class Slime10 : MonoBehaviour 
	{
		#region DATA
		public Vector3 axis;
		public float speed;
		private float dir;

		private Rpc GoRight = new Rpc ();
		private Rpc GoLeft = new Rpc ();
		#endregion

		#region CALLBACKS
		void LateUpdate () 
		{
			// Rotate around
			float vel = speed * dir * Time.deltaTime;
			transform.Rotate (axis, vel, Space.Self);
		}

		private void Awake () 
		{
			GoRight.Register (() => SwitchDir (1f));
			GoLeft.Register (() => SwitchDir (-1f));
		}

		public void SwitchDir (float dir) 
		{
			// Notify all Clients
			if (Net.IsServer)
			{
				if (dir == 1f) GoRight.SendToAll ();
				else
				if (dir == -1f) GoLeft.SendToAll ();
			}
			// Change moving direction
			this.dir = dir;
		}
		#endregion
	} 
}
