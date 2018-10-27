using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeroesRace 
{
	public class Rotator : MonoBehaviour 
	{
		public Vector3 axis;
		public float speed;
		private float dir;

		void LateUpdate () 
		{
			// Rotate around
			float vel = speed * dir * Time.deltaTime;
			transform.Rotate (axis, vel, Space.Self);
		}

		public void SwitchDir (float dir) 
		{
			// Change moving direction
			this.dir = dir;
		}
	} 
}
