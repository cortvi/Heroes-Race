using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HeroesRace 
{
	[ExecuteInEditMode]
	public class Courtain : MonoBehaviour 
	{
		public static NetCourtain net;

		[Range (0f, 1f)] public float alpha;
		[Range (0f, 1f)] public float fade;
		private Color fColor;
		private Material mat;

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
			#if UNITY_EDITOR
			// Don't call dont-destroy on inspector !
			if (!UnityEditor.EditorApplication.isPlaying) return;
			#endif

			// Do this only once
			DontDestroyOnLoad (gameObject);
		}
	} 
}
