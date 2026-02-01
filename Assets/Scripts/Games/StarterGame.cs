using System.Collections.Generic;
using MyStateMachine;
using UnityEngine;

public class StarterGame : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject canvas;
    [Space]
    [SerializeField] private Game game;
    [SerializeField] private List<DialogueNode> completeRequired = new();
    [SerializeField] private DialogueNode dialogue;

    private bool available = false;
    private bool gameEnd = false;


    void Start()
    {
        DialogoSystem.Instance.OnComplete += OnComplete;
        game.onEndGame += OnEndGame;
        DialogoSystem.Instance.OnReset += OnReset;
        canvas.SetActive(false);
        available = false;
    }

    void OnDestroy()
    {
        DialogoSystem.Instance.OnComplete -= OnComplete;
        game.onEndGame -= OnEndGame;
        DialogoSystem.Instance.OnReset -= OnReset;
        OnReset();
    }

    private void OnEndGame()
    {
        game.onEndGame -= OnEndGame;
        gameEnd = true;
        available = false;
        DialogoSystem.Instance.StartDialogue(dialogue);
    }

    private void OnReset()
    {
        game.onEndGame += OnEndGame;
        if (game != null && game.gameObject != null)
            game.gameObject.SetActive(false);
        dialogue.isCompleted = false;
        dialogue.isRefused = false;
        gameEnd = false;
        available = false;
    }

    private void OnComplete(DialogueNode nodeComplete)
    {
        if (gameEnd) return;

        foreach (var dialogue in completeRequired)
        {
            if (dialogue == null) continue;
            if (dialogue == nodeComplete)
            {
                available = true;
                return;
            }
        }
    }







    void OnTriggerEnter2D(Collider2D collision)
    {
        if (CharacterStateMachine.Instance.gameObject != collision.gameObject) return;
        if (!available) return;
        if (gameEnd) return;
        canvas.SetActive(true);
        CharacterStateMachine.Instance.inputReader.OnInteractEvent += OnInteract;
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (CharacterStateMachine.Instance.gameObject != collision.gameObject) return;
        if (!available) return;
        if (gameEnd) return;
        canvas.SetActive(false);
        CharacterStateMachine.Instance.inputReader.OnInteractEvent -= OnInteract;
    }

    private void OnInteract()
    {
        CharacterStateMachine.Instance.inputReader.OnInteractEvent -= OnInteract;
        canvas.SetActive(false);
        game.gameObject.SetActive(true);
    }

}
