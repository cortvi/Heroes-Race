using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PrefabSpawner : MonoBehaviour
{
	/// El Prefab que colocar
	public GameObject prefab;
	public float delay;

	void Create ()
	{
		if ( prefab.GetComponent<NetworkIdentity> () != null )
		{
			var o = Instantiate (prefab, transform.position, transform.rotation);
			if (GetComponentInParent<NetworkIdentity> ().isServer)
				NetworkServer.Spawn (o);
		}
		else Instantiate (prefab, transform.position, transform.rotation);

		Destroy (gameObject);
	}

	public void Start () 
	{
		Invoke ("Create", delay);
	}
}
