using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public static class Extensions
{
	public static IEnumerator WaitAnimator( this Animator a, string animation, int layer = 0 )
	{
		while (a.GetCurrentAnimatorStateInfo (layer).IsName (animation))
			yield return null;
	}
}
