using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

namespace HeroesRace 
{
	public class TowerGenerator: MonoBehaviour 
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

			if (bypassTower.Length > 0)
			{
				#region BYPASS
				// First quesito is always entracne:
				tower[0][0] = (int) Qs._01_Entrada;

				for (int f=0; f<bypassTower.Length; ++f)
				{
					for (int q=0; q<9; ++q)
					{
						if (tower[f][q] != 0) continue;

						if (bypassTower[f][q+1] == (int) Qs._11_Ascensores)
						{
							tower[f][q] = (int) Qs._11_Ascensores;
							tower[f+1][q] = - (int) Qs._11_Ascensores;
						}
						// Bypass randomizer, manually form tower:
						else tower[f][q] = bypassTower[f][q+1];
					}
				}

				#endregion
			}
			else
			{
				#region RANDOMIZER
				tower[0][0] = (int) Qs._01_Entrada;
				tower[3][5] = (int) Qs._17_THE_END;

				int lifts = 0;
				for (int f=0; f<4; ++f)
				{
					if (f < 3)
					{
						// Lifts can be in either quesito 4 or 5:
						lifts = (int) Mathf.Repeat (Random.Range (4f, 6f) + lifts, 9f);

						tower[f][lifts] = (int) Qs._11_Ascensores;
						tower[f + 1][lifts] = - (int) Qs._11_Ascensores;
					}

					for (int q=0; q<9; ++q)
					{
						if (tower[f][q] != 0) continue;
						int Q = 0, last;

						if (q == 0) last = 8;
						else last = q - 1;

						do
						{
							Q = (f == 0) ?
							   // Avoid throw-quesitos on first floor
							   Random.Range (4, 13) : // No-throw only
							   Random.Range (4, 17);  // All
							yield return null;
						}
						// Avoid repeating quesistos
						while (tower[f][last] == Q || (f > 0 && Q >= 13 && ((Qs) tower[f - 1][q]).ToString ().StartsWith ("f")));

						// Asignar quesito
						tower[f][q] = Q;
					}
				}
				#endregion
			}

			#region SPAWNINGs
			for (int f=0; f<4; ++f)
			{
				for (int q=0; q<9; ++q)
				{
					// Skip bypassed quesitos set to 'None'
					int index = Mathf.Abs (tower[f][q]);
					if (index == 0) continue;

					// Instantiate quesito & align with its floor
					var Q = Instantiate (qPrefabs[index - 1]).transform;
					Q.position = floorRoots[f].position;
					Q.rotation = Quaternion.identity;

					// Rotate it to create the circle
					Q.Rotate (Vector3.up, q * -40f);

					// If spawned quesito is an lift up
					if ((Qs) index == Qs._11_Ascensores && tower[f][q] > 0) 
					{
						// Spawn lifts & rotate next floor to align lifts
						Q.GetComponent<Q11> ().SpawnLifts (floor: f);
					}
					// Finally spawn the quesito
					NetworkServer.Spawn (Q.gameObject);
				}
			}
			#endregion
		}

		private void Awake () 
		{
			// Auto destroy itself
			if (Net.IsClient) Destroy (gameObject);
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
			public Qs q9;
		}
	}

	// 'p' means power-up
	// 'f' means "no-fall-into"
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
		f05_Apisonadora,
		f08_MiniSlimes,
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


