using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using TMPro;

public class DialogueSystem : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    //public TMP_Text characterNameText;
    public TMP_Text dialogueText;
    //public Transform answersPanel;
    //public GameObject answerButtonPrefab;

    public List<Button> buttons = new();

    [Header("Dialogue Data")]
    public DialogueGraphData dialogueData;

    private DialogueNodeData currentNode;
    private Dictionary<int, DialogueNodeData> nodesDictionary = new Dictionary<int, DialogueNodeData>();

    public static DialogueSystem Instance;


    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {

        // Inicializar diccionario de nodos
        if (dialogueData != null)
        {
            foreach (var node in dialogueData.graph.nodes)
            {
                nodesDictionary[node.id] = node;
            }

            Debug.Log($"dialogue ID: {dialogueData.id}");
            // Comenzar con el nodo inicial
            //StartDialogue();
        }

    }

    public void StartDialogue()
    {
        Debug.Log($"StartDialogue");

        if (dialogueData == null)
        {
            Debug.LogError("No hay diálogo asignado!");
            return;
        }


        currentNode = dialogueData.GetStartNode();
        if (currentNode == null)
        {
            Debug.LogError("No se encontró el nodo inicial!");
            return;
        }

        ShowDialoguePanel(true);
        DisplayCurrentNode();
    }

    private void DisplayCurrentNode()
    {
        // Mostrar nombre del personaje

        foreach (var btn in buttons)
        {
            btn.gameObject.SetActive(false);
            btn.onClick.RemoveAllListeners();
        }

        // Mostrar texto del diálogo
        dialogueText.text = currentNode.dialogueText;


        // Si hay respuestas, mostrarlas
        if (currentNode.answers != null && currentNode.answers.Count > 0)
        {
            for (int i = 0; i < currentNode.answers.Count; i++)
            {
                var answer = currentNode.answers[i];
                CreateAnswerButton(answer, i);
            }
        }
        else
        {
            // Si no hay respuestas, crear botón para continuar
            CreateContinueButton();
        }
    }

    private void CreateAnswerButton(AnswerData answer, int index)
    {
        buttons[index].gameObject.SetActive(true);

        //GameObject buttonObj = Instantiate(answerButtonPrefab, answersPanel);
        //Button button = buttonObj.GetComponent<Button>();
        TMP_Text buttonText = buttons[index].GetComponentInChildren<TMP_Text>();

        // Formato: [Personaje] Respuesta
        string buttonLabel = $"{answer.text}";      //[{answer.characterId}] 
        buttonText.text = buttonLabel;

        // Asignar acción al botón
        int targetNodeId = answer.targetNodeId;
        buttons[index].onClick.AddListener(() => OnAnswerSelected(targetNodeId));
    }

    private void CreateContinueButton()
    {
        buttons[0].gameObject.SetActive(true);

        //GameObject buttonObj = Instantiate(answerButtonPrefab, answersPanel);
        //Button button = buttonObj.GetComponent<Button>();
        TMP_Text buttonText = buttons[0].GetComponentInChildren<TMP_Text>();

        buttonText.text = "Continuar...";

        // Si hay siguiente nodo, ir a él
        if (currentNode.nextNodeId > 0)
        {
            int nextNodeId = currentNode.nextNodeId;
            buttons[0].onClick.AddListener(() => OnContinue(nextNodeId));
        }
        else
        {
            // Si no hay siguiente nodo, terminar diálogo
            buttons[0].onClick.AddListener(EndDialogue);
        }
    }

    private void OnAnswerSelected(int targetNodeId)
    {
        if (targetNodeId <= 0)
        {
            Debug.LogWarning("Respuesta sin destino asignado!");
            return;
        }

        // Ir al nodo destino
        GoToNode(targetNodeId);
    }

    private void OnContinue(int nextNodeId)
    {
        GoToNode(nextNodeId);
    }

    private void GoToNode(int nodeId)
    {
        if (nodesDictionary.TryGetValue(nodeId, out DialogueNodeData nextNode))
        {
            currentNode = nextNode;
            DisplayCurrentNode();
        }
        else
        {
            Debug.LogError($"No se encontró el nodo con ID: {nodeId}");
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        ShowDialoguePanel(false);
        Debug.Log("Diálogo terminado!");
    }

    private void ShowDialoguePanel(bool show)
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(show);
        }
    }

    // Método público para cargar un nuevo diálogo
    public void LoadDialogue(DialogueGraphData newDialogue)
    {
        dialogueData = newDialogue;
        nodesDictionary.Clear();

        foreach (var node in dialogueData.graph.nodes)
        {
            nodesDictionary[node.id] = node;
        }

        StartDialogue();
    }
}