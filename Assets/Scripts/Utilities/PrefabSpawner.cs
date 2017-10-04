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
		var o = Instantiate (prefab, transform.position, transform.rotation);
		if ( o.GetComponent<NetworkIdentity> () != null )
			if (GetComponentInParent<NetworkIdentity> ().isServer)
				NetworkServer.Spawn (o);

		Destroy (gameObject);
	}

	public void Awake() 
	{
		Invoke ("Create", delay);
	}
}
