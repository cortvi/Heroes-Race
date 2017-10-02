using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AscensorSpawner : MonoBehaviour
{
	public Transform[] lifts;
	public GameObject goodPrefab;
	public GameObject badPrefab;

	private void Start()
	{
		GenerateLifts (Random.Range (0, 3));
	}

	public void GenerateLifts ( int chosen )
	{
		for (var i=0;i!=3;i++)
		{
			if (i==chosen)
			{
				var a = Instantiate (goodPrefab);
				a.transform.SetParent (lifts[chosen]);
				a.transform.localPosition = Vector3.forward * 0.07f;
				a.transform.localRotation = Quaternion.identity;
			}
			else
			{
				var a = Instantiate (badPrefab);
				a.transform.SetParent (lifts[i]);
				a.transform.localPosition = Vector3.forward * 0.07f;
				a.transform.localRotation = Quaternion.identity;
			}
		}
	}
}
