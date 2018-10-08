using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HeroesRace 
{
	public class HeroHUD : MonoBehaviour 
	{
		public Sprite[] sprites;
		public Image powerUp;

		public void UpdatePower (PowerUp newPower) 
		{
			if (newPower != PowerUp.None)
			{
				int idx = (int)newPower;
				powerUp.sprite = sprites[idx];
				powerUp.enabled = true;
			}
			else powerUp.enabled = false;
			#warning En el futuro, quizá hacer esto más dinámico
		}
	} 
}
