using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSlow : MonoBehaviour
{
	[Range(0f, 1f)]
	public float slowAmount;

	private void OnTriggerEnter( Collider other )
	{
		if (other.tag!="Player") return;
		var p = other.GetComponent<Character> ();
		p.charSpeed *= slowAmount;
		p.SpeedMul *= slowAmount;
	}

	private void OnTriggerExit( Collider other )
	{
		if (other.tag!="Player") return;
		var p = other.GetComponent<Character> ();
		p.charSpeed /= slowAmount;
		p.SpeedMul /= slowAmount;
	}
}
