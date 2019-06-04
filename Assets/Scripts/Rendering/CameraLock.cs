using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeroesRace 
{
	public class CameraLock : MonoBehaviour 
	{
		public static CameraLock i;
		public readonly Vector3 offset = new Vector3 (0f, 3.06f, 7.88f);

		private static void SetCam (bool locked, HeroCam cam) 
		{
			// On Clients, must search only Cam
			if (!cam) cam = FindObjectOfType<HeroCam> ();
			cam.locked = locked;
		}

		private void OnTriggerEnter (Collider other) 
		{
			if (Net.IsClient || other.tag != "Player") return;
			var hero = other.GetComponent<Driver> ().owner;

			SetCam (true, hero.cam);
			Rpc.SendTo ("LockCam", hero.owner.Conn);
		}
		private void OnTriggerExit (Collider other) 
		{
			if (Net.IsClient || other.tag != "Player") return;
			var hero = other.GetComponent<Driver> ().owner;

			SetCam (false, hero.cam);
			Rpc.SendTo ("UnlockCam", hero.owner.Conn);
		}

		private void Awake () 
		{
			if (i == null) i = this;
			else
			{
				Destroy (this);
				return;
			}

			// Register RPC calls
			Rpc.Register ("LockCam",  () => SetCam (true, null));
			Rpc.Register ("UnlockCam",() => SetCam (false,null));
		}
	} 
}
