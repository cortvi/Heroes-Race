using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TowerLoader : MonoBehaviour
{
	public static PJs[] pjSelected;
	public GameObject[] pjPrefabs;
	public RenderTexture[] targets;

	private void Start()
	{
		if (Game.manager!=null) return;
		StartCoroutine (LoadGame ());
	}

	IEnumerator LoadGame ()
	{
		yield return new WaitUntil (() => Input.GetKeyDown (KeyCode.P));
		for (var i=0; i!=3; i++)
		{
			var obj = Instantiate (pjPrefabs[( int ) pjSelected[i]] );
			NetworkServer.SpawnWithClientAuthority (obj, Networker.conns[i]);

			/// Setup camera
			var cam = obj.GetComponentInChildren<Camera> ().gameObject;
			cam.GetComponent<Camera> ().targetTexture = targets[i];
			cam.GetComponent<AudioListener> ().enabled = false;
			cam.GetComponent<FlareLayer> ().enabled = false;
			cam.GetComponent<GUILayer> ().enabled = false;
		}
	}
}
