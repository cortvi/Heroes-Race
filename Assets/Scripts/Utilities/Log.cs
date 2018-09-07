using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

internal static class Log 
{
	public enum LogType 
	{
		None,
		Info,
		Debug,
		DeepDebug
	}
	public static LogType logLevel;

	[Conditional ("UNITY_EDITOR")]
	public static void Info (object msg, Object o = null) 
	{
		if (logLevel >= LogType.Info)
			UnityEngine.Debug.Log (msg, o);
	}

	[Conditional ("UNITY_EDITOR")]
	public static void Debug (object msg, Object o = null) 
	{
		if (logLevel >= LogType.Debug)
			UnityEngine.Debug.Log (msg, o);
	}

	[Conditional ("UNITY_EDITOR")]
	public static void LowDebug (object msg, Object o = null) 
	{
		if (logLevel >= LogType.DeepDebug)
			UnityEngine.Debug.Log (msg, o);
	}
}
