using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MyStateMachine
{
    public abstract class State
    {
        public abstract void Enter();
        public abstract void Tick(float deltaTime);
        public virtual void FixedTick(float deltaTime) { }
        public abstract void Exit();
        public virtual void OnDisable() { }
        public virtual void OnEnable() { }
        public virtual void OnDestroy() { }
        public virtual void OnBecameInvisible() { }
        public virtual void OnBecameVisible() { }
        

        protected float GetNormalizedTime(Animator animator, string tag, int layerIndex = 0)
        {
            AnimatorStateInfo currentInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);
            AnimatorStateInfo nextInfo = animator.GetNextAnimatorStateInfo(layerIndex);

            if (animator.IsInTransition(layerIndex) && nextInfo.IsTag(tag))
            {
                return nextInfo.normalizedTime;
            }
            else if (!animator.IsInTransition(layerIndex) && currentInfo.IsTag(tag))
            {
                return currentInfo.normalizedTime;
            }
            else
            {
                return 0f;
            }
        }
    }
}



