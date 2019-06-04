using UnityEngine;

namespace HeroesRace 
{
	public class Q1: QBase 
	{
		[SerializeField]
		private Transform[] spawnPoints;
		public static Q1 i;

		public Vector3 GetSpawn (int position) 
		{
			return spawnPoints[position].position;
		}

		protected override void OnAwake ()
		{
			if (i != null)
			{
				Destroy (gameObject);
				return;
			}
			else
			{
				i = this;
				Player.SetPawns ();
			}
			base.OnAwake ();
		}
	} 
}
