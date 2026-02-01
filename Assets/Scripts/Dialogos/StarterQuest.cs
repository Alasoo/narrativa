using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class StarterQuest : MonoBehaviour
{
    [SerializeField] private List<DialogueNode> dialogues = new();

    public List<DialogueNode> dialoguesAvailable = new();


    void Start()
    {
        DialogoSystem.Instance.OnComplete += OnComplete;
        DialogoSystem.Instance.OnReset += OnReset;
    }

    void OnDestroy()
    {
        DialogoSystem.Instance.OnComplete -= OnComplete;
        DialogoSystem.Instance.OnReset -= OnReset;
        OnReset();
    }

    private void OnReset()
    {
        if (dialogues.Count == 0) return;
        foreach (var dialogue in dialogues)
        {
            if (dialogue == null) continue;
            dialogue.isCompleted = false;
            dialogue.isRefused = false;
        }

        dialoguesAvailable.Clear();
    }

    public bool HaveDialogueAvailable()
    {
        return dialoguesAvailable.Count > 0;
    }

    public DialogueNode TakeDialogue()
    {
        if (dialoguesAvailable.Count > 0)
            return dialoguesAvailable[0];

        return null;
    }


    private void OnComplete(DialogueNode nodeComplete)
    {
        if (dialoguesAvailable.Contains(nodeComplete))
            dialoguesAvailable.Remove(nodeComplete);

        foreach (var nodeToComplete in nodeComplete.nodesToComplete)
        {
            if (dialoguesAvailable.Contains(nodeToComplete))
                dialoguesAvailable.Remove(nodeToComplete);
        }

        foreach (var nodeToRefuse in nodeComplete.nodesToRefuse)
        {
            if (dialoguesAvailable.Contains(nodeToRefuse))
                dialoguesAvailable.Remove(nodeToRefuse);
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
}
