using System;
using UnityEngine;
using UnityEngine.InputSystem;


namespace MyInputSystem
{
    public class InputReader : MonoBehaviour, InputSystem_Actions.IPlayerActions
    {
        public Vector2 movementValue { get; private set; }
        public Vector2 lookValue { get; private set; }
        public Vector2 scrollValue { get; private set; }
        public bool sprint { get; private set; }
        public bool attacking { get; private set; }

        public event Action OnAttackEvent;
        public event Action OnInteractEvent;
        public event Action OnCancelMenuEvent;
        public event Action OnCancelGameEvent;




        private InputSystem_Actions controls;


        private void Start()
        {
            controls = new();
            controls.Player.SetCallbacks(this);


            controls.Player.Enable();
        }


        private void OnDestroy()
        {
            controls.Player.Disable();
        }




        public void OnLook(InputAction.CallbackContext context)
        {
            lookValue = context.ReadValue<Vector2>();
        }
        public void OnMove(InputAction.CallbackContext context)
        {
            movementValue = context.ReadValue<Vector2>();
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                attacking = true;
                OnAttackEvent?.Invoke();
            }
            else if (context.canceled)
            {
                attacking = false;
                OnAttackEvent?.Invoke();
            }
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            if (context.started)
                sprint = true;
            else if (context.canceled)
                sprint = false;
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                OnInteractEvent?.Invoke();
            }
        }
        public void OnCancelGame(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            OnCancelGameEvent?.Invoke();
        }
        public void OnCancelMenu(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            OnCancelMenuEvent?.Invoke();
        }

        public void OnScroll(InputAction.CallbackContext context)
        {
            scrollValue = context.ReadValue<Vector2>();
        }


        public void OnNavigate(InputAction.CallbackContext context)
        {
            Debug.Log($"Navigate");
        }
        public void OnSubmit(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                Debug.Log($"OnSubmit");
            }
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
        }

        public void OnJump(InputAction.CallbackContext context)
        {
        }

        public void OnPrevious(InputAction.CallbackContext context)
        {
        }

        public void OnNext(InputAction.CallbackContext context)
        {
        }
    }
}
