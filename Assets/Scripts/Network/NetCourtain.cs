using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HeroesRace 
{
	public class NetCourtain : NetBehaviour 
	{
		private Courtain courtain;
		private Animator animator;
		private Text text;

		#region UTILS
		public void SetText (string newText) 
		{
			// Change display text
			text.text = newText.Replace (" ", "    ");
		}

		public void Open (bool state) 
		{
			// Set courtain open state
			animator.SetBool ("Open", state);
		} 
		#endregion

		#region CALLBACKS
		protected override void OnStart () 
		{
			courtain = FindObjectOfType<Courtain> ();
			animator = courtain.GetComponent<Animator> ();
			text = courtain.GetComponentInChildren<Text> ();
			Courtain.net = this;
		}

		protected override void OnAwake () 
		{
			// Preserve during whole session
			DontDestroyOnLoad (gameObject);
		} 
		#endregion
	} 
}
