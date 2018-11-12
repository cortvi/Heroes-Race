using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	public class PlataformaRota : NetBehaviour 
	{
		public BoxCollider trigger;
		private Rigidbody[] pieces;
		private static Collider[] hits = new Collider[3];

		private IEnumerator Break () 
		{
			// Wait until breaking in both Client & Server
			yield return new WaitForSeconds (0.5f);
			GetComponents<BoxCollider> ().ToList ().ForEach (b=> b.enabled = false);
			Rpc_Throw ();
			Throw ();

			// Overlap box to get all players laying on platform
			var pos = transform.position; var rot = transform.rotation;
			var ctr = trigger.center; var ext = trigger.size;
			int n = Physics.OverlapBoxNonAlloc (pos + ctr, ext, hits, rot, 1 << 8);
			for (int i=0; i!=n; ++i)
			{
				// Thorw all found players on air
				var driver = hits[i].GetComponent<Driver> ();
				if (driver)
				{
					driver.body.AddForce (Vector3.up * 3.5f, ForceMode.VelocityChange);
					driver.SwitchFriction (false);
					driver.owner.OnAir = true;
				}
			}

			// Spawn new platform after X time
			yield return new WaitForSeconds (1.7f);
			var next = Instantiate (Resources.Load ("Prefabs/Plataforma_rota") as GameObject);
			next.transform.position = transform.position;
			next.transform.rotation = transform.rotation;
			NetworkServer.Spawn (next);

			// Finally destroy this onw
			NetworkServer.Destroy (gameObject);
		}

		private void Throw () 
		{
			foreach (var p in pieces)
			{
				p.isKinematic = false;
				float force = Random.Range (3f, 5f);
				float upForce = Random.Range (0.5f, 1.5f);
				p.AddExplosionForce (force, transform.position, 1.5f, upForce, ForceMode.VelocityChange);
			}
		}
		[ClientRpc]
		private void Rpc_Throw () { Throw (); }

		#region CALLBACKS
		[ServerCallback]
		private void OnTriggerEnter (Collider other) 
		{
			if (other.tag == "Player")
				StartCoroutine (Break ());
		}

		protected override void OnAwake () 
		{
			pieces = GetComponentsInChildren<Rigidbody> (true);
		}
		#endregion
	} 
}
