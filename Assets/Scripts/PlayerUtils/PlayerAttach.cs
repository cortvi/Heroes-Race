using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttach : MonoBehaviour
{
	private void OnTriggerEnter( Collider other )
	{
		if (other.tag!="Player") return;
		other.transform.SetParent (transform);
	}
	private void OnTriggerExit( Collider other )
	{
		if (other.tag!="Player") return;
		other.transform.SetParent (null);
	}
}
