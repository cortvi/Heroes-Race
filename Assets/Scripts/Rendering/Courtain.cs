using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace HeroesRace 
{
	public class Courtain : MonoBehaviour 
	{
		#region DATA
		public static Courtain i;

		[Range (0f, 1f)] public float alpha;
		[Range (0f, 1f)] public float fade;
		private Color fColor;
		private Material mat;

		private Animator anim;
		private Text text;
		#endregion

		public static void Open (bool state, bool overNet = false) 
		{
			// Open / Close courtain
			i.anim.SetBool ("Open", true);
			if (overNet && Net.IsServer) 
			{
				if (state) Rpc.SendToAll ("Open");
				else Rpc.SendToAll ("Close");
			}
		}

		public static void SetText (string text) 
		{
			// Change display text
			i.text.text = text;
		}

		#region CALLBACKS
		private void Update () 
		{
			if (!mat) Start ();

			fColor.a = alpha;
			mat.SetColor ("_FColor", fColor);
			mat.SetFloat ("_Fade", fade);
		}

		private void Start () 
		{
			// Do once, but re-invokable
			mat = GetComponentInChildren<Image> ().materialForRendering;
			fColor = mat.GetColor ("_FColor");
		}

		private void Awake () 
		{
			// Do this only once
			DontDestroyOnLoad (gameObject);
			anim = GetComponent<Animator> ();
			text = GetComponentInChildren<Text> ();
			i = this;
		} 
		#endregion
	} 
}
