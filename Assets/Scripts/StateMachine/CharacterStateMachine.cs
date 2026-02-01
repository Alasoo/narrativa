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

        private Vector2 startPos;

        void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            startPos = transform.position;
            SwitchState(new PlayerMoveState(this));
            DialogoSystem.Instance.OnReset += OnReset;

        }

        void OnDestroy()
        {
            DialogoSystem.Instance.OnReset -= OnReset;
        }


        private void OnReset()
        {
            transform.position = startPos;
        }




        private void OnDisable()
        {
        }

    }
}



