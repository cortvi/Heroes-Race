using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Internal;
using UnityEngine.Networking;

public class TowerGenerator : NetworkBehaviour
{
	public GameObject[] pjPrefabs;

	public IEnumerator GenerateTower ()
	{
		#region COMMENTED OUT
/*
		var tower = new int[4][];
		tower[0] = new int[9];
		tower[1] = new int[9];
		tower[2] = new int[9];
		tower[3] = new int[9];
		yield return null;

		#region RANDOMIZER
		for (var p=0; p!=4; p++)
		{
			// Primer quesito
			tower[p][0] = ( int ) (p==0 ? Qs.Entrada : Qs.Ascensores);

			// Ultimo quesito
			if (p!=3)
			{
				var pos = Random.Range (4, 6);
				tower[p][pos] = ( int ) Qs.Ascensores;
			}
			else tower[p][5] = ( int ) Qs.THE_END;

			// Resto de quesitos
			for (var q=1; q!=9; q++)
			{
				if ((Qs)tower[p][q] == Qs.Ascensores) continue;
				if ((Qs)tower[p][q] == Qs.THE_END) continue;

				int Q;
				do
				{
					Q =
					p==0 ?
					Random.Range (03, 12) : // No throw
					Random.Range (12, 16);  // Throw
					yield return null;
				}
				while (tower[p][q-1] == Q);

				tower[p][q] = Q;
			}
			// Yield
			yield return null;
		}
		#endregion

		#region SPAWNING
		for (var p = 0;p!=4;p++)
		{
			for (var q = 0;q!=9;q++)
			{
				var Q = Instantiate (qPrefabs[tower[p][q]]);
				Q.transform.SetParent (levelsRoot[p]);
				Q.transform.localPosition = Vector3.zero;
				Q.transform.localRotation = Quaternion.Euler (0f, q*-40f, 0f);

				if (( Qs ) tower[p][q] == Qs.Ascensores && q!=0)
				{
					Q.GetComponent<AscensorSpawner> ().enabled = true;
					levelsRoot[p+1].Rotate (Vector3.up, q*-40f);
				}
				NetworkServer.Spawn (Q);
			}
		}
		#endregion
*/
		#endregion

		#region SPAWN PLAYERS
		var players = FindObjectsOfType<Game> ();
		foreach (var p in players)
		{
			var obj = Instantiate (pjPrefabs[p.pj]);
			obj.GetComponent<Player> ().owner = p;
			obj.transform.position = Vector3.up * 1.33f;
			obj.transform.rotation = Quaternion.Euler (0f, 192.57f - p.pj, 0f);
			yield return new WaitUntil (()=> p.connectionToClient.isReady);
			NetworkServer.SpawnWithClientAuthority (obj, p.gameObject);
		}
		#endregion
	}

	private void Start () 
	{
		StartCoroutine ("GenerateTower");
	}
}


