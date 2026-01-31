using System.Collections;
using System.Collections.Generic;
using MyInputSystem;
using UnityEngine;

namespace MyStateMachine
{
    public class CharacterStateMachine : StateMachine
    {
        [field: SerializeField] public Animator anim { get; private set; }
        [field: SerializeField] public Rigidbody2D rb { get; private set; }
        [field: SerializeField] public InputReader inputReader { get; private set; }
        [field: Space]
        [field: SerializeField] public float moveSpeedBase { get; private set; } = 4f;

        public static CharacterStateMachine Instance;


        void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            SwitchState(new PlayerMoveState(this));
        }




        private void OnEnable()
        {

        }




        private void OnDisable()
        {
        }

    }
}



