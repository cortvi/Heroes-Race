using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeroesRace 
{
	public class Driver : MonoBehaviour
	{
		internal Action<Collision> logic;
		private void OnCollisionEnter (Collision collision)
		{
			logic (collision);
		}
	} 
}
