using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlataformaRota : MonoBehaviour
{
	public Rigidbody[] pieces;
	GameObject next;

	private void Start()
	{
		next = Instantiate (gameObject);
		next.SetActive (false);
	}

	private void OnTriggerEnter( Collider other )
	{
		if (other.tag=="Player") StartCoroutine ("Break");
	}

	IEnumerator Break ()
	{
		yield return new WaitForSeconds (0.3f);
		foreach (var p in pieces)
		{
			p.isKinematic = false;
			Destroy (p.gameObject, 2f);
		}
		yield return new WaitForSeconds (2f);
		next.SetActive (true);
		Destroy (gameObject);
	} 
}
