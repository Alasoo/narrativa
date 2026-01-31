using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyStateMachine
{
    public abstract class MachineBaseState : State
    {
        protected CharacterStateMachine stateMachine;

        public MachineBaseState(CharacterStateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
        }


        protected const float AnimatorDampTime = .1f;

        protected void Move(Vector2 movement, float fixedDeltatime)
        {
            stateMachine.rb.MovePosition(stateMachine.rb.position + movement * fixedDeltatime);
        }

    }
}



