using MyStateMachine;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject canvas;
    [SerializeField] private StarterQuest starterQuest;


    void Start()
    {
        canvas.SetActive(false);
    }


    void OnTriggerEnter2D(Collider2D collision)
    {
        if (CharacterStateMachine.Instance.gameObject != collision.gameObject) return;
        if (!starterQuest.HaveDialogueAvailable()) return;
        canvas.SetActive(true);
        CharacterStateMachine.Instance.inputReader.OnInteractEvent += OnInteract;
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (CharacterStateMachine.Instance.gameObject != collision.gameObject) return;
        if (!starterQuest.HaveDialogueAvailable()) return;
        canvas.SetActive(false);
        CharacterStateMachine.Instance.inputReader.OnInteractEvent -= OnInteract;
    }

    private void OnInteract()
    {
        CharacterStateMachine.Instance.inputReader.OnInteractEvent -= OnInteract;
        canvas.SetActive(false);
        DialogoSystem.Instance.StartDialogue(starterQuest.TakeDialogue());
    }
}
