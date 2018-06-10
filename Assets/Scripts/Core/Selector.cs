using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Selector : NetBehaviour 
{
	[ClientCallback]
	private void Update () 
	{
		if (!hasAuthority) return;

		if (Input.GetKeyDown (KeyCode.A)) print (name);
	}
}
