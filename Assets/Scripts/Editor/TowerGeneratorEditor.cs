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
			if (info = EditorGUILayout.Foldout (info, "Info quesitos")) 
			{
				// Print all quesitos names in the Enum order
				foreach (var q in Enum.GetNames (typeof(Qs)))
				{
					if (q[0] == '_')
					{
						// Mark power-up quesitos
						string name = q.Replace ("_", "");
						EditorGUILayout.LabelField (name, EditorStyles.boldLabel);
					}
					else EditorGUILayout.LabelField (q);
				}
			}
		}
	} 
}
