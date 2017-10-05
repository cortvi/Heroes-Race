using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ClientOnly : NetworkBehaviour
{
	private void Start()
	{
		if (!isClient)
			gameObject.SetActive (false);
	}
}
