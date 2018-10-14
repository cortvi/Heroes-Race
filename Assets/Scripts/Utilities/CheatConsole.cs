using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CheatConsole : MonoBehaviour 
{
	#region DATA
	public KeyCode toggleKey;
	private string input;
	private bool show;

	private const int margin = 50;
	Rect titleBarRect = new Rect (0, 0, 350, 20);
	Rect windowRect = new Rect (margin, Screen.height - 80, 350, 50);
	#endregion

	void ConsoleWindow (int windowID) 
	{
		GUI.SetNextControlName ("input-box");
		input = GUILayout.TextField (input);
		
		// Read code commands from console
		if (Event.current.keyCode == KeyCode.Return) 
		{
			if (!string.IsNullOrEmpty (input))
			{
				// Convert input to an array passed through reflection
				var commands = input.Split (' ').ToList ();
				string method = commands[0].CapitalizeFirst ();
				object[] param = null;
				if (commands.Count > 1)
				{
					commands.RemoveAt (0);
					param = new object[1];
					param[0] = commands.ToArray ();
				}
				// Use reflection to execute input
				var cheats = typeof (HeroesRace.Cheats);
				cheats.GetMethod (method).Invoke (null, param); 
			}
			// Hide console
			show = false;
		}

		// Allow the window to be dragged by its title bar.
		GUI.DragWindow (titleBarRect);
	}

	#region CALLBACKS
	void OnGUI () 
	{
		if (!show) return;
		windowRect = GUILayout.Window (123456, windowRect, ConsoleWindow, "Cheat Console");
		GUI.FocusControl ("input-box");
	}

	void Update () 
	{
		if (Input.GetKeyDown (toggleKey))
			show = true;
	}
	#endregion
}
