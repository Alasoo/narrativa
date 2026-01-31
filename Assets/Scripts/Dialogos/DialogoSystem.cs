using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogoSystem : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public List<Button> buttons = new();
    [Space]
    public GameObject helpPanel;
    public TMP_Text helpText;

    [Header("Start dialogue")]
    public DialogueNode startDialogue;

    public int lastMissionCompleted { get; private set; } = -1;



    public DialogueNode currentNode;
    private Dictionary<Button, TMP_Text> buttonDict = new();

    public static DialogoSystem Instance;

    public event Action<int> OnComplete;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        foreach (var btn in buttons)
        {
            TMP_Text txt = btn.GetComponentInChildren<TMP_Text>();
            buttonDict.Add(btn, txt);
        }

        if (startDialogue != null)
            StartDialogue(startDialogue);
    }


    // Llama a esto para iniciar una conversación
    public void StartDialogue(DialogueNode startingNode)
    {
        helpPanel.SetActive(false);

        DisplayNode(startingNode);
        dialoguePanel.SetActive(true);
    }

    private void DisplayNode(DialogueNode node)
    {
        Debug.Log($"Display Node");
        currentNode = node;

        // 1. Actualizar UI del NPC
        dialogueText.text = node.dialogueText;

        // 2. Limpiar botones anteriores
        foreach (var btn in buttons)
        {
            btn.onClick.RemoveAllListeners();
            btn.gameObject.SetActive(false);
        }

        // 3. Crear botones para las respuestas del Player
        for (int i = 0; i < node.options.Count; i++)
        {
            var currentOption = node.options[i];
            Button btn = buttons[i];
            TMP_Text txt = buttonDict[btn];

            buttonDict.ElementAt(i).Value.text = node.options[i].responseText;
            txt.text = currentOption.responseText;

            Debug.Log($"Configurando botón {i} con opción: {currentOption.responseText}");
            // Usamos 'currentOption' en lugar de 'node.options[i]'
            btn.onClick.AddListener(() => OnOptionSelected(currentOption));
            btn.gameObject.SetActive(true);
        }

        if (node.options.Count == 0)
        {
            buttons[0].onClick.AddListener(() => EndDialogue());

            // Acceder al texto del primer botón
            if (buttonDict.ContainsKey(buttons[0]))
                buttonDict[buttons[0]].text = "Continuar";

            buttons[0].gameObject.SetActive(true);
        }
    }

    private void OnOptionSelected(DialogueOption option)
    {
        Debug.Log($"Click");
        if (option == null)
        {
            Debug.Log($"option es nulo");
            return;
        }

        if (option.nextNode == null)
        {
            Debug.Log($"nextNode es nulo");

            EndDialogue();
        }
        else
        {
            DisplayNode(option.nextNode);
        }
    }


    private void EndDialogue()
    {
        Debug.Log("Fin de la conversación");

        if (currentNode == null)
        {
            Debug.Log($"currentNode es nulo");
            return;
        }

        dialoguePanel.SetActive(false);
        helpText.text = currentNode.helpText;
        helpPanel.SetActive(true);
        if (currentNode.endAndComplete)
            Complete();
    }

    public void Complete()
    {
        lastMissionCompleted = currentNode.id;
        OnComplete?.Invoke(currentNode.id);
        currentNode = null;
    }
}
