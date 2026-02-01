using System.Collections.Generic;
using MyStateMachine;
using UnityEngine;

public class EndGameManager : MonoBehaviour
{

    [SerializeField] private Animator anim;
    [Space]
    [SerializeField] private DialogueNode dialogue91;
    [SerializeField] private DialogueNode dialogue92;
    [SerializeField] private DialogueNode dialogue93;
    [SerializeField] private DialogueNode dialogue94;


    public List<DialogueNode> dialoguesAvailable = new();

    private DialogueNode dialogueSelected = null;

    public static EndGameManager Instance;

    private List<DialogueNode> dialogues = new();

    void Awake()
    {
        Instance = this;
    }


    void Start()
    {
        DialogoSystem.Instance.OnReset += OnReset;
        DialogoSystem.Instance.OnComplete += OnComplete;
        dialogues.Add(dialogue91);
        dialogues.Add(dialogue92);
        dialogues.Add(dialogue93);
        dialogues.Add(dialogue94);
    }

    void OnDisable()
    {
        DialogoSystem.Instance.OnReset -= OnReset;
        DialogoSystem.Instance.OnComplete -= OnComplete;
    }

    private void OnReset()
    {
        anim.SetBool("go", false);
        anim.SetBool("do1", false);
        anim.SetBool("do2", false);
        anim.SetBool("do3", false);
        anim.SetBool("do4", false);
    }

    private void OnComplete(DialogueNode nodeComplete)
    {
        if (dialoguesAvailable.Contains(nodeComplete))
        {
            dialoguesAvailable.Remove(nodeComplete);
            //
            Debug.Log("FIN DEL JUEGO?");
        }

        foreach (var dialogue in dialogues)
        {
            if (dialogue == null) continue;
            if (dialogue == nodeComplete) continue;
            if (dialogue.isCompleted) continue;
            if (dialogue.isRefused) continue;

            if (dialogue.idCompletedRequired.Count > 0)
            {
                foreach (var node in dialogue.idCompletedRequired)
                {
                    if (DialogoSystem.Instance.lastMissionCompleted == node)
                        dialoguesAvailable.Add(dialogue);
                }
            }

            if (dialogue.idCompletedRequired_and.Count > 0)
            {
                bool allCompleted = true;
                foreach (var node in dialogue.idCompletedRequired_and)
                {
                    if (node.isCompleted) continue;
                    allCompleted = false;
                    break;
                }
                if (allCompleted)
                    dialoguesAvailable.Add(dialogue);
            }
        }
    }

    public void PlayAnimation(DialogueNode dialogueSelected)
    {
        this.dialogueSelected = dialogueSelected;
        anim.SetBool("go", true);
    }


    public void DoorOpened()
    {
        if (dialogueSelected == dialogue91)
        {
            anim.SetBool("do1", true);
        }
        else if (dialogueSelected == dialogue92)
        {
            anim.SetBool("do2", true);
        }
        else if (dialogueSelected == dialogue93)
        {
            anim.SetBool("do3", true);
        }
        else if (dialogueSelected == dialogue94)
        {
            anim.SetBool("do4", true);
        }
    }

    public void EnterCharacter()
    {
        if (dialogues.Count <= 0) return;
        Debug.Log($"Ejecutamos diálogo: {dialogues[0]}");
        DialogoSystem.Instance.StartDialogue(dialogues[0]);
    }



    void OnTriggerEnter2D(Collider2D collision)
    {
        if (CharacterStateMachine.Instance.gameObject != collision.gameObject) return;
        if (!dialogues.Contains(DialogoSystem.Instance.lastMissionCompleted)) return;

        PlayAnimation(DialogoSystem.Instance.lastMissionCompleted);
    }


    void OnTriggerExit2D(Collider2D collision)
    {
    }


}
