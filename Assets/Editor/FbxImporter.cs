using System;
using UnityEngine;
using UnityEditor;

public class FBXScaleOverride : AssetPostprocessor
{
	void OnPreprocessModel ()
	{
		if (assetImporter.userData.Contains ("DONE")) return;

		var importer = assetImporter as ModelImporter;
		var name = importer.assetPath.ToLower ();
		if (name.Substring (name.Length - 4, 4)==".fbx")
		{
			importer.isReadable = false;
			importer.importBlendShapes = false;
			importer.weldVertices = false;
			importer.importMaterials = false;
			importer.materialName = ModelImporterMaterialName.BasedOnMaterialName;
			importer.materialSearch = ModelImporterMaterialSearch.RecursiveUp;
		}

		assetImporter.userData = "DONE";
	}
}