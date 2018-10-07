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
			// Throw pieces away
			yield return new WaitForSeconds (0.5f);
			Rpc_Throw ();
			Throw ();

			// Destroy pieces over Net after 2s
			yield return new WaitForSeconds (2f);
			foreach (var p in pieces) NetworkServer.Destroy (p.gameObject);

			// Spawn next platform over net
			var next = Instantiate (gameObject);
			NetworkServer.Spawn (next);
			NetworkServer.Destroy (gameObject);
		}

		private void Throw () 
		{
			foreach (var p in pieces)
			{
				p.isKinematic = false;
				p.AddExplosionForce (5f, transform.position, 1f, 2f);
			}
		}
		[ClientRpc] private void Rpc_Throw () 
		{
			Throw ();
		}

		#region CALLBACKS
		protected override void OnAwake () 
		{
			pieces = GetComponentsInChildren<Rigidbody> (true);
		}

		private void OnTriggerEnter (Collider other) 
		{
			if (other.tag == "Player")
				StartCoroutine (Break ());
		} 
		#endregion
	} 
}
