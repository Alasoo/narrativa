using UnityEngine;
using System;
using System.Collections.Generic;

[System.Serializable]
public class DialogueNodeData
{
    public int id;
    public Vector2 position;
    public string characterId;
    public string dialogueText;
    public List<AnswerData> answers = new List<AnswerData>();
    
    // Para conexión directa del nodo (sin respuesta)
    public int nextNodeId = -1;
}

[System.Serializable]
public class AnswerData
{
    public string text;
    public string characterId;
    public int targetNodeId = -1; // ID del nodo al que conecta esta respuesta
}

[System.Serializable]
public class ConnectionData
{
    public int inPointNodeId;
    public int outPointNodeId;
    public bool isFromAnswer; // Indica si la conexión viene de una respuesta
    public int answerIndex; // Índice de la respuesta si isFromAnswer es true
}

[System.Serializable]
public class DialogueGraph
{
    public List<DialogueNodeData> nodes = new List<DialogueNodeData>();
    public List<ConnectionData> connections = new List<ConnectionData>();
    public int startNodeId = 1; // ID del nodo inicial
}

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue System/Dialogue")]
public class DialogueGraphData : ScriptableObject
{
    public int id;
    public DialogueGraph graph = new DialogueGraph();
    
    // Método para encontrar un nodo por ID
    public DialogueNodeData GetNode(int id)
    {
        foreach (var node in graph.nodes)
        {
            if (node.id == id) return node;
        }
        return null;
    }
    
    // Método para encontrar el nodo inicial
    public DialogueNodeData GetStartNode()
    {
        return GetNode(graph.startNodeId);
    }
}