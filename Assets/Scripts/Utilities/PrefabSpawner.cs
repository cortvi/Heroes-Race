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
		Instantiate (prefab, transform.position, transform.rotation);
		Destroy (gameObject);
	}

	public void Awake() 
	{
		Invoke ("Create", delay);
	}
}
