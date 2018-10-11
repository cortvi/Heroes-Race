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
		private GameObject next;

		private IEnumerator Break () 
		{
			yield return new WaitForSeconds (0.5f);
			GetComponents<BoxCollider> ().ToList ().ForEach (b=> b.enabled = false);
			Rpc_Throw ();
			Throw ();

			yield return new WaitForSeconds (3f);
			NetworkServer.Destroy (gameObject);
			NetworkServer.Spawn (next);
			next.SetActive (true);
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
		[ClientRpc] private void Rpc_Throw () 
		{
			Throw ();
		}

		#region CALLBACKS
		[ServerCallback]
		private void OnTriggerEnter (Collider other) 
		{
			if (other.tag == "Player")
				StartCoroutine (Break ());
		}

		protected override void OnAwake () 
		{
			// Spawn next before pieces get thrown away
			var next = Instantiate (gameObject);
			next.SetActive (false);

			pieces = GetComponentsInChildren<Rigidbody> (true);
		}
		#endregion
	} 
}
