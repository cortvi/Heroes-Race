using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeroesRace 
{
	public abstract class SMBController<TBridge> : StateMachineBehaviour where TBridge : Component
	{
		public abstract class Logic 
		{
			public virtual void OnEnter (AnimatorStateInfo info) { }
			public virtual void OnUpdate (AnimatorStateInfo info) { }
			public virtual void OnExit (AnimatorStateInfo info) { }
		}

		#region DATA
		public bool enabled;
		public bool networked;
		private bool ready;

		protected SmartAnimator anim;
		protected TBridge bridge;

		private Dictionary<int, Logic> callbacks;
		#endregion

		#region UTILS
		public void SetReady (Animator animator) 
		{
			// Get references
			anim = new SmartAnimator (animator, networked);
			bridge = animator.GetComponent<TBridge> ();

			// Initialize dictionaries
			callbacks = new Dictionary<int, Logic> ();
			SubscribeCallbacks ();

			// Finalize initialization
			ready = true;
		}
		protected abstract void SubscribeCallbacks ();
		#endregion

		#region CALLBACKS
		public sealed override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
		{
			if (!enabled || !ready) return;

		}
		public sealed override void OnStateUpdate (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
		{
			if (!enabled || !ready) return;

		}
		public sealed override void OnStateExit (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
		{
			if (!enabled || !ready) return;

		}
		#endregion
	}
}
