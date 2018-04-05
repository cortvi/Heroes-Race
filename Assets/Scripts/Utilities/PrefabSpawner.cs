using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PrefabSpawner : MonoBehaviour 
{
	/// El Prefab a colocar
	public GameObject prefab;
	public float delay;

	IEnumerator Start () 
	{
		/// Wait Delay time
		var mark = Time.time + delay;
		while (Time.time <= mark) yield return null;

		/// Replace this object by Prefab
		Instantiate (prefab, transform.position, transform.rotation);
		Destroy (gameObject);
	}
}
