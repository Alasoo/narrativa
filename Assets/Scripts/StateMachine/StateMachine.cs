using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MyStateMachine
{
    public abstract class StateMachine : MonoBehaviour
    {
        private State currentState = null;


#if UNITY_EDITOR
        private void OnValidate()
        {

        }
#endif

        public void SwitchState(State newState)
        {
            currentState?.Exit();
            currentState = newState;
            currentState?.Enter();
        }


        private void Update()
        {
            currentState?.Tick(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            currentState?.FixedTick(Time.fixedDeltaTime);
        }

        private void OnDestroy()
        {
            currentState?.OnDestroy();
        }

        private void OnDisable()
        {
            currentState?.OnDisable();
        }
        private void OnEnable()
        {
            currentState?.OnEnable();
        }

        private void OnBecameInvisible()
        {
            currentState?.OnBecameInvisible();
        }

        private void OnBecameVisible()
        {
            currentState?.OnBecameVisible();
        }

    }
}



