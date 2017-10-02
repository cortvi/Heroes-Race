using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Internal;

public class TowerGenerator : MonoBehaviour
{
	public GameObject[] levelsRoot;				// Los transforms de cada nivel
	public GameObject[] prefabs;				// Los prefabs de los quesitos
	// 18->quesito final
	// 17->entrada 2 (recepcion ascensores)

	public IEnumerator GenerateTower ()
	{
		var tower = new int[3][];
		for (var lvl=0; lvl!=3; lvl++)
		{
			int[] level;
			do
			{
				level = GenerateLevel (lvl);
				yield return null;
			}
			while (!ValidateLevel (level));
		}
	}

	int[] GenerateLevel ( int lvl )
	{
		var level = new int?[9];
		// El primer quesito del primer piso es la entrada
		if (lvl==0) level[0] = 1;
		else
		{
			// El quesito inicial en el resto son los ascensores
			level[0] = 11;
			// El ultimo quesito del ultimo piso es el final
			if (lvl==2) level[5] = 18;
		}
		for (var q=0; q!=9; q++)
		{
			if (level[q]!=null) continue;

		}
		throw new Exception ();
	}

	bool ValidateLevel ( int[] level )
	{
		throw new Exception ();
	}
}
