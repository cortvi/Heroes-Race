using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

[CustomPropertyDrawer (typeof (InfoAttribute))]
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
#endif

[System.AttributeUsage (AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class InfoAttribute : PropertyAttribute { }
