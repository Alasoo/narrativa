using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


public class Game : MonoBehaviour
{
    public event Action onEndGame;

    public static Game Instance;


    void Awake()
    {
        Instance = this;
    }

    protected virtual void EndGame()
    {
        onEndGame?.Invoke();
    }


#if UNITY_EDITOR
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
        {
            gameObject.SetActive(false);
            onEndGame?.Invoke();
        }
    }
#endif

}
