using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeroesRace 
{
	public class Driver : MonoBehaviour 
	{
		internal Rigidbody body;
		internal CapsuleCollider capsule;
		internal Action<Collision> onCollisionEnter;

		private void OnCollisionEnter (Collision collision) 
		{
			// Collision logic happens in Server's Hero
			onCollisionEnter (collision);
		}

		private void Awake () 
		{
			capsule = GetComponent<CapsuleCollider> ();
			body = GetComponent<Rigidbody> ();
			body.centerOfMass = Vector3.zero;
		}
	} 
}
