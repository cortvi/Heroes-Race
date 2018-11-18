using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TentaculoFix 
{
	[MenuItem ("Fix/Tentaculo")]
	public static void Optimize ()  
	{
		string guid = AssetDatabase.FindAssets ("Tentaculo t:Mesh")[0];
		string path = AssetDatabase.GUIDToAssetPath (guid);
		var model = AssetImporter.GetAtPath (path) as ModelImporter;

		model.optimizeGameObjects = true;
		model.extraExposedTransformPaths = new string[] { "CATRigTail22" };
		
		AssetDatabase.WriteImportSettingsIfDirty (path);
		AssetDatabase.Refresh ();
	}
}
