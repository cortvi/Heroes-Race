using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Selector : NetBehaviour 
{
	#region DATA
	private Vector3 position;
	#endregion

	#region CALLBACKS
	private void Start () 
	{
		// Correct position
		transform.position = position;
	}

	protected override void OnAwake () 
	{
		// Cache position because it'll move
		// when connected to server
		position = transform.position;
	}
	#endregion
}
