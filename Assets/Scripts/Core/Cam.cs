using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Cam : MonoBehaviour
{
	public RenderTexture[] targets;

	[Range(-1f,1f)]
	public float upDown;

	public int currentLevel;
	private const float levelHeigth = 5.2f;

	private void LateUpdate()
	{
		var h = levelHeigth*currentLevel + levelHeigth*upDown + 2.65f;
		var newPos = transform.parent.InverseTransformPoint (Vector3.up*h);
		newPos.x = transform.localPosition.x;
		newPos.z = transform.localPosition.z;
		transform.localPosition = newPos;
	}

	public void MoveLevel ( int dir ) 
	{ currentLevel += dir; }

	private void Start()
	{
		var nId = transform.parent.GetComponent<NetworkIdentity> ();
		if (nId.isServer)
		{
			var id = nId.GetComponent<Player> ().owner.pj;
			GetComponent<Camera> ().targetTexture = targets[id];
			this.enabled = false;
		}
		else 
		if (!nId.hasAuthority) gameObject.SetActive (false);
	}
}