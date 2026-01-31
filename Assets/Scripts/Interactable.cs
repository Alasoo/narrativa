using MyStateMachine;
using UnityEngine;

[RequireComponent(typeof(StarterQuest))]
public class Interactable : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject canvas;
    [SerializeField] private StarterQuest starterQuest;

    public bool canInteract { get; private set; } = true;

    void Start()
    {
        canvas.SetActive(false);
    }


    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!canInteract) return;
        if (starterQuest.Quest() == null) return;
        if (CharacterStateMachine.Instance.gameObject != collision.gameObject) return;
        canvas.SetActive(true);
        CharacterStateMachine.Instance.inputReader.OnInteractEvent += OnInteract;
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (!canInteract) return;
        if (starterQuest.Quest() == null) return;
        if (CharacterStateMachine.Instance.gameObject != collision.gameObject) return;
        canvas.SetActive(false);
        CharacterStateMachine.Instance.inputReader.OnInteractEvent -= OnInteract;
    }

    private void OnInteract()
    {
        DialogoSystem.Instance.StartDialogue(starterQuest.Quest());
    }
}
