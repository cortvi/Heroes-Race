using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class UIAlwaysActive : MonoBehaviour
{
	bool alreadyChangeToPlay;

	private void Update()
	{
		if (!alreadyChangeToPlay && EditorApplication.isPlayingOrWillChangePlaymode)
		{
			alreadyChangeToPlay = true;

			var ui = GameObject.Find ("Canvas").transform;
			for (var c=0; c!=ui.childCount; c++)
			{
				ui.GetChild (c).gameObject.SetActive (true);
			}
		}
		else
		if (alreadyChangeToPlay && !EditorApplication.isPlaying)
			alreadyChangeToPlay = false;
	}
}
