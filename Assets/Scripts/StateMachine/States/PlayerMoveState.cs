using System;
using MyInputSystem;
using UnityEngine;


namespace MyStateMachine
{
    public class PlayerMoveState : MachineBaseState
    {
        public PlayerMoveState(CharacterStateMachine stateMachine) : base(stateMachine) { }

        private readonly int idleDownHash = Animator.StringToHash("IdleDown_Anim");
        private readonly int idleLeftHash = Animator.StringToHash("IdleLeft_Anim");
        private readonly int idleRightHash = Animator.StringToHash("IdleRight_Anim");
        private readonly int idleUpHash = Animator.StringToHash("IdleUp_Anim");

        private readonly int runDownHash = Animator.StringToHash("RunDown_Anim");
        private readonly int runLeftHash = Animator.StringToHash("RunLeft_Anim");
        private readonly int runRightHash = Animator.StringToHash("RunRight_Anim");
        private readonly int runUpHash = Animator.StringToHash("RunUp_Anim");

        Vector2 movement;
        private Direction direction = Direction.None;

        private enum Direction
        {
            None,
            Up,
            Down,
            Left,
            Right
        }



        public override void Enter()
        {
            stateMachine.anim.Play(idleDownHash);
        }

        public override void Tick(float deltaTime)
        {
            if (DialogoSystem.Instance.dialoguePanel.activeSelf) return;
            if (Game.Instance != null && Game.Instance.gameObject.activeSelf) return;

            movement = stateMachine.inputReader.movementValue;
            if (movement.magnitude == 0)
            {
                if (direction == Direction.Down)
                    stateMachine.anim.Play(idleDownHash);
                else if (direction == Direction.Up)
                    stateMachine.anim.Play(idleUpHash);
                else if (direction == Direction.Left)
                    stateMachine.anim.Play(idleLeftHash);
                else if (direction == Direction.Right)
                    stateMachine.anim.Play(idleRightHash);

                direction = Direction.None;
                return;
            }

            //prioridad hacia los lados
            if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
            {
                if (movement.x > 0)
                {
                    stateMachine.anim.Play(runRightHash);
                    direction = Direction.Right;
                }
                else
                {
                    direction = Direction.Left;
                    stateMachine.anim.Play(runLeftHash);
                }
            }
            else
            {
                if (movement.y > 0)
                {
                    direction = Direction.Up;
                    stateMachine.anim.Play(runUpHash);
                }
                else
                {
                    direction = Direction.Down;
                    stateMachine.anim.Play(runDownHash);
                }
            }
        }

        public override void FixedTick(float fixedDeltatime)
        {
            if (DialogoSystem.Instance.dialoguePanel.activeSelf) return;
            if (Game.Instance != null && Game.Instance.gameObject.activeSelf) return;

            Move(movement * stateMachine.moveSpeedBase, fixedDeltatime);
        }

        public override void Exit()
        {
        }
    }
}
