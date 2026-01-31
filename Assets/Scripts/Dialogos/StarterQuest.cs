using System.Collections.Generic;
using UnityEngine;

public class StarterQuest : MonoBehaviour
{
    [SerializeField] private List<DialogueNode> dialogues = new();

    public DialogueNode Quest()
    {
        foreach (var dialogue in dialogues)
        {
            if(dialogue == null) continue;
            if (DialogoSystem.Instance.lastMissionCompleted == dialogue.idCompletedRequired)
                return dialogue;
        }
        return null;
    }
}
