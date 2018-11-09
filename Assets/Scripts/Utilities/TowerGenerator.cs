using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

namespace HeroesRace 
{
	public class TowerGenerator : MonoBehaviour 
	{
		public Floor[] bypassTower;
		public Transform[] floorRoots;
		public GameObject[] qPrefabs;

		private IEnumerator Start () 
		{
			var tower = new int[4][];
			tower[0] = new int[9];
			tower[1] = new int[9];
			tower[2] = new int[9];
			tower[3] = new int[9];

			#region BYPASS
			if (bypassTower.Length != 0)
			{
				for (int f=0; f!=bypassTower.Length; ++f)
				{
					// Primer quesito, ascensores o spawn-platform
					tower[f][0] = (int)(f == 0? Qs._01_Entrada : Qs._11_Ascensores);
					for (int q=1; q!=9; ++q)
					{
						// Bypass randomizer, manually form tower
						tower[f][q] = bypassTower[f][q];
					}
				}
			}
			#endregion

			#region RANDOMIZER
			else
			{
				for (int f=0; f!=4; ++f)
				{
					// Primer quesito, ascensores o spawn-platform
					tower[f][0] = (int) (f==0? Qs._01_Entrada : Qs._11_Ascensores);

					// Ultimo quesito de cada piso
					if (f != 3)
					{
						// Los ascensores pueden estar en
						// las posiciones 4, 5, o 6 de cada piso
						int pos = Random.Range (4, 6);
						tower[f][pos] = (int) Qs._11_Ascensores;
					}
					// Ultimo quesito de toda la torre
					else tower[f][5] = (int) Qs._17_THE_END;

					// Resto de quesitos
					for (int q=1; q!=9; ++q)
					{
						if ((Qs) tower[f][q] == Qs._11_Ascensores) continue;
						if ((Qs) tower[f][q] == Qs._17_THE_END) continue;

						int Q; do
						{Q =
							f == 0?
							// Avoid throw-quesitos on first floor
							Random.Range (04, 13) : // No-throw only
							Random.Range (04, 17);  // All

							yield return null;
						}
						// Avoid repeating quesistos
						while (tower[f][q - 1] == Q);

						// Asignar quesito
						tower[f][q] = Q;
					}
				}
			}
			#endregion

			#region SPAWNING
			for (var f=0; f!=4; ++f)
			{
				for (var q=0; q!=9; ++q)
				{
					// Skip bypassed quesitos set to 'None'
					int index = tower[f][q];
					if (index == 0) continue;

					// Instantiate quesito & align with its floor
					var Q = Instantiate (qPrefabs[index - 1]).transform;
					Q.position = floorRoots[f].position;
					Q.rotation = floorRoots[f].rotation;

					// Rotate it to create the circle
					Q.Rotate (Vector3.up, q * -40f);

					// If spawned quesito is a not-entry-lift
					if ((Qs) index == Qs._11_Ascensores && q != 0) 
					{
						// Spawn lifts & rotate next floor to align lifts
						Q.GetComponent<Q11> ().SpawnLifts (floor: f);
						floorRoots[f + 1].Rotate (Vector3.up, q * -40f);
					}
					// Finally spawn the quesito
					NetworkServer.Spawn (Q.gameObject);
				}
			}
			#endregion
		}

		private void Awake () 
		{
			if (Net.isClient)
			{
				// Register all quesitos & destroy itself
				foreach (var p in qPrefabs) ClientScene.RegisterPrefab (p);
				Destroy (gameObject);
			}
		}

		[System.Serializable]
		public struct Floor 
		{
			List<FieldInfo> fields;

			public int this[int index] 
			{
				get 
				{
					if (fields == null)
					{
						// Get reflected info about the quesito fields
						fields = GetType ().GetFields ().ToList ();
					}

					// Find quesito by name
					var field = fields.Find (f => (f.Name == "q" + index));
					return (int) field.GetValue (this);
				}
			}

			public Qs q1;
			public Qs q2;
			public Qs q3;
			public Qs q4;
			public Qs q5;
			public Qs q6;
			public Qs q7;
			public Qs q8;
		}
	}

	// 'p' means power-up
	public enum Qs 
	{
		None,
		// Especiales
		_01_Entrada,
		_11_Ascensores,
		_17_THE_END,
		// No-throw
		p02_PiranasSaltarinas,
		_06_PiranasVolarinas,
		p04_Cueva,
		_05_Apisonadora,
		_08_MiniSlimes,
		_10_Slime,
		p09_Pinchus,
		p12_Medusa,
		_15_Perla,
		// Throw
		p03_Pared,
		p07_Hueco,
		p13_Tentaculo,
		_14_Paredes
	} 
}


