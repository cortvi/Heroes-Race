using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
	public ParticleSystem ps;

	private void OnTriggerEnter( Collider other )
	{
		if (other.tag!="Player" || !ps) return;

		Destroy (gameObject);
	}
}

public enum PU 
{
	SpeedUp,
	Shield,
	Bomb
}
