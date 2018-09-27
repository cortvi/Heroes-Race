using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.AttributeUsage (System.AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class InfoAttribute : PropertyAttribute { }

[CustomPropertyDrawer (typeof(InfoAttribute))]
public sealed class InfoDrawer : PropertyDrawer 
{
	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
	{
		// Cache value to not break other extensions
		bool enabled = GUI.enabled;

		GUI.enabled = false;
		EditorGUI.PropertyField (position, property, label, true);
		GUI.enabled = enabled;
	}
}
