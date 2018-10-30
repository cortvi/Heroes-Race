using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeroesRace 
{
	public class PlataformaRota : NetBehaviour 
	{
		private Rigidbody[] pieces;

		private IEnumerator Break () 
		{
			yield return new WaitForSeconds (0.5f);
			GetComponents<BoxCollider> ().ToList ().ForEach (b=> b.enabled = false);
			Rpc_Throw ();
			Throw ();

			yield return new WaitForSeconds (1.7f);
			var next = Instantiate (Resources.Load ("Prefabs/Plataforma_rota") as GameObject);
			next.transform.position = transform.position;
			NetworkServer.Spawn (next);

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
