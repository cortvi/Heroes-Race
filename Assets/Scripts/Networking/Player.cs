using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Networking
{
	public class Player : MonoBehaviour
	{
		private void Update()
		{
			/// Movement
			var movement = Vector3.zero;
			movement += Input.GetAxis ("Horizontal") * transform.right;
			movement += Input.GetAxis ("Vertical") * transform.forward;
			me.SimpleMove (movement);

			/// Rotation
			transform.Rotate (transform.up, Input.GetAxis ("Mouse X") * 120f * Time.deltaTime);
		}

		CharacterController me;
		private void Awake() { me = GetComponent<CharacterController> (); }
	} 
}
