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

	#region CALLBACKS
	[ClientCallback]
	private void Awake ()
	{
		// Ask for selector assignation
		Cmd_AskForSelector ();
	}

	[Command]
	private void Cmd_AskForSelector ()
	{
		int id = connectionToClient.connectionId - 1;
		selectors[id].id.AssignClientAuthority (connectionToClient);
		selectors[id].SetName ("Selector");
	} 
	#endregion
}
