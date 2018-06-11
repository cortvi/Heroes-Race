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
		(transform as RectTransform).localPosition = position;
		Debug.Log ("Position restored", this);
	}

	protected override void OnAwake () 
	{
		// Cache position because it'll move
		// when connected to server
		position = (transform as RectTransform).localPosition;
		Debug.Log ("Cached: " + position, this);
	}
	#endregion
}
