using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyStateMachine
{
    public class NPCStateMachine : StateMachine
    {
        [field: SerializeField] public GameObject canvas { get; private set; }


        private void Start()
        {
            canvas.SetActive(false);
        }




        private void OnEnable()
        {

        }




        private void OnDisable()
        {
        }

        void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject != CharacterStateMachine.Instance.gameObject) return;
            canvas.SetActive(true);
            CharacterStateMachine.Instance.inputReader.OnInteractEvent += OnInteract;
        }

        void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject != CharacterStateMachine.Instance.gameObject) return;
            canvas.SetActive(false);
            CharacterStateMachine.Instance.inputReader.OnInteractEvent -= OnInteract;
        }

        private void OnInteract()
        {
            CharacterStateMachine.Instance.inputReader.OnInteractEvent -= OnInteract;
            Debug.Log($"Interact!");
            //DialogueSystem.Instance.StartDialogue();
        }
    }
}



