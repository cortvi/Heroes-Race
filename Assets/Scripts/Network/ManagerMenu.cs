using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[DefaultExecutionOrder(-100)]
public class ManagerMenu : NetworkBehaviour 
{
	#region DATA
	public Selector[] selectors;
	#endregion

	[ServerCallback]
	private void Awake () 
	{
		// Assign authority to selector
		foreach (var p in Networker.players)
		{
			int id = p.connectionToClient.connectionId - 1;
			selectors[id].id.AssignClientAuthority (p.connectionToClient);
			selectors[id].SetName ("Selector");
		}
	}
}
