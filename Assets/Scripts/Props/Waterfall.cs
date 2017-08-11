using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Gestiona el movimiento de los
/// blend-shapes de la torre de agua
public class Waterfall : MonoBehaviour
{
	/// La velocidad de cada Key
	[Range(-1f, 1f)] public float[] keys;

	public float rotationSpeed;
	SkinnedMeshRenderer mesh;
	float[] values;

	private void Update()
	{
		for ( var k=0; k!=keys.Length; k++ )
		{
			values[k] += keys[k] * 100f * Time.deltaTime;
			mesh.SetBlendShapeWeight (k, values[k]);

			// Invertir velocidad si se alcanzan minimos/maximos
			if (values[k] <= 0 || values[k] >= 100)
			{
				keys[k] *= -1;
				values[k] = Mathf.Clamp (values[k], 0f, 100f);
			}
		}

		transform.Rotate (Vector3.up, rotationSpeed * Time.deltaTime);
	}

	private void Awake() 
	{
		Camera.main.depthTextureMode = DepthTextureMode.Depth;
		mesh = GetComponent<SkinnedMeshRenderer> ();
		values = new float[keys.Length];
	}
}
