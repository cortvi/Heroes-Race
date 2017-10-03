using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
	public PU forcePU;
	public ParticleSystem ps;

	private void OnTriggerEnter( Collider other )
	{
//		if (!ps) return;
		if (other.tag!="Player") return;
		var p = other.GetComponent<Player> ();
		if (p.powerUp!=PU.NONE) return;

		var pu = (forcePU==PU.NONE ? (PU) Random.Range (1, 4) : forcePU);
		p.powerUp = pu;
		ShowPU (( int ) pu);
//		transform.DetachChildren ();
//		ps.Play ();
		Destroy (gameObject);
	}

	public static void ShowPU ( int pu, bool hideInstead=false )
	{
		var hud = GameObject.Find ("HUD_PU").transform;
		for (var i = 0;i!=3;i++) hud.GetChild (i).gameObject.SetActive (false);
		if (!hideInstead) hud.GetChild (pu-1).gameObject.SetActive (true);
	}
}

public enum PU 
{
	NONE,
	SpeedUp,
	Shield,
	Bomb
}
