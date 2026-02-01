using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
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
    [Space]
    public TMP_Text logText;

    [Header("Start dialogue")]
    public DialogueNode startDialogue;

    public DialogueNode lastMissionCompleted { get; private set; } = null;



    private DialogueNode currentNode;
    private Dictionary<Button, TMP_Text> buttonDict = new();

    public static DialogoSystem Instance;

    public event Action<DialogueNode> OnComplete;
    public event Action OnReset;


    private List<DialogueNode> nodosUsados = new();

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

#if UNITY_EDITOR
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            OnReset?.Invoke();

            foreach (var nodo in nodosUsados)
            {
                nodo.isCompleted = false;
                nodo.isRefused = false;
            }
            nodosUsados.Clear();

            if (startDialogue != null)
                StartDialogue(startDialogue);
        }
    }
#endif

    void OnDestroy()
    {
        startDialogue.isCompleted = false;
        startDialogue.isRefused = false;
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
        currentNode = node;

        if (!nodosUsados.Contains(node))
            nodosUsados.Add(node);

        // 1. Actualizar UI del NPC
        dialogueText.text = node.dialogueText;
        logText.text = node.name;

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
        if (option == null) return;

        if (option.nextNode == null)
        {
            Debug.Log($"nextNode es nulo");
            EndDialogue();
        }
        else
        {
            if (currentNode.endAndComplete)
                Complete();
            DisplayNode(option.nextNode);
        }
    }


    private void EndDialogue()
    {
        if (currentNode == null) return;

        dialoguePanel.SetActive(false);
        helpText.text = currentNode.helpText;
        helpPanel.SetActive(true);
        if (currentNode.endAndComplete)
            Complete();
    }

    public void Complete()
    {
        lastMissionCompleted = currentNode;
        Debug.Log($"Completada misión: {currentNode.name} - {lastMissionCompleted}");
        currentNode.isCompleted = true;
        foreach (var nodeToComplete in currentNode.nodesToComplete)
            nodeToComplete.isCompleted = true;
        foreach (var nodeToRefuse in currentNode.nodesToRefuse)
            nodeToRefuse.isRefused = true;

        OnComplete?.Invoke(currentNode);
        currentNode = null;
    }
}
