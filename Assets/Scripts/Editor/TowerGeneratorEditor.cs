using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HeroesRace 
{
	[CustomEditor (typeof(TowerGenerator))]
	public class TowerGeneratorEditor : Editor 
	{
		private bool info;

		public override void OnInspectorGUI () 
		{
			base.OnInspectorGUI ();
			EditorGUILayout.LabelField ("", GUI.skin.horizontalSlider);

			// Print all quesitos names in the Enum order
			if (info = EditorGUILayout.Foldout (info, "Info quesitos", true)) 
			{
				foreach (var q in Enum.GetNames (typeof(Qs)))
				{
					if (q == "None") continue;

					string name = q.Remove (0, 1);
					var style = q[0] == 'p' ? EditorStyles.boldLabel : EditorStyles.label;
					EditorGUILayout.LabelField (name, style);
				}
			}
		}
	} 
}
