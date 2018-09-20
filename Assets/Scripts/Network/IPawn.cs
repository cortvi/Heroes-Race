using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeroesRace 
{
	public interface IPawn
	{
		string[] GetInput ();
		void ProcessInput (string[] input);
	} 
}
