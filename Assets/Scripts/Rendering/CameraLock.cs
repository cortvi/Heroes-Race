using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeroesRace 
{
	public class CameraLock : MonoBehaviour
	{
		public Vector3 offset;

		private void OnTriggerStay (Collider other) 
		{
			if (other.tag != "Player") return;
			var hero = other.GetComponent<Driver> ().owner;
			hero.cam.@override = offset;
		}
	} 
}
