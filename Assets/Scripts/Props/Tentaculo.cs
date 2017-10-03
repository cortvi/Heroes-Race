using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tentaculo : MonoBehaviour
{
	Animator anim;
	public Transform hook;
	public float awakeTime;
	public float sleepTime;

	List<Player> playersIn;
	bool doneWaiting;

	IEnumerator Work ()
	{
		while (true)
		{
			yield return new WaitForSeconds (sleepTime);
			anim.SetTrigger ("Wake");
			yield return new WaitForSeconds (1.3f);
			StartCoroutine ("WaitUntilSleep");
			StartCoroutine ("WaitUntilPrey");

			while (!doneWaiting) yield return null;
			StopCoroutine ("WaitUntilPrey");
			StopCoroutine ("WaitUntilSleep");
			doneWaiting = false;

			if (playersIn.Count>0)
			{
				anim.SetTrigger ("Hook");
				yield return new WaitForSeconds (1.3f);
			}
			else
			{
				anim.SetTrigger ("Sleep");
				yield return new WaitForSeconds (1f);
			}
		}
	}
	IEnumerator WaitUntilSleep () 
	{
		yield return new WaitForSeconds (awakeTime);
		doneWaiting = true;
	}
	IEnumerator WaitUntilPrey () 
	{
		while (playersIn.Count==0) yield return null;
		doneWaiting = true;
	}

	void Hook ()
	{
		if (playersIn.Count==0) return;

		var chosen = playersIn[Random.Range (0, playersIn.Count)];
		if (chosen.shielded)
		{
			chosen.shielded=false;
			playersIn.Remove (chosen);
			return;
		}

		chosen.StartCoroutine (chosen.BlockPlayer (2f));
		chosen.StartCoroutine (chosen.Tentaculo (hook));
		playersIn.Remove (chosen);
	}

	private void OnTriggerEnter( Collider other ) 
	{
		if (other.tag=="Player")
			playersIn.Add (other.GetComponent<Player> ());

	}
	private void OnTriggerExit( Collider other )  
	{
		if (other.tag=="Player")
			playersIn.Remove (other.GetComponent<Player> ());
	}

	private void Start () 
	{
		playersIn = new List<Player> (3);
		anim = GetComponent<Animator> ();
		StartCoroutine ("Work");
	}
}