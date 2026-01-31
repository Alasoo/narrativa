using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class DialogueSaveSystem
{
    private DialogueNodeEditor editorWindow;
    
    public DialogueSaveSystem(DialogueNodeEditor window)
    {
        editorWindow = window;
    }
    
    public void SaveDialogue(string path)
    {
        // Crear o cargar el ScriptableObject existente
        DialogueGraphData dialogueData = null;
        
        if (File.Exists(path))
        {
            dialogueData = AssetDatabase.LoadAssetAtPath<DialogueGraphData>(path);
        }
        
        if (dialogueData == null)
        {
            dialogueData = ScriptableObject.CreateInstance<DialogueGraphData>();
            AssetDatabase.CreateAsset(dialogueData, path);
        }
        
        // Limpiar datos existentes
        dialogueData.graph.nodes.Clear();
        dialogueData.graph.connections.Clear();
        
        // Guardar nodos
        foreach (var node in editorWindow.nodes)
        {
            DialogueNodeData nodeData = new DialogueNodeData
            {
                id = node.id,
                position = node.rect.position,
                characterId = node.characterId,
                dialogueText = node.dialogueText
            };
            
            // Guardar respuestas
            for (int i = 0; i < node.answers.Count; i++)
            {
                var answer = node.answers[i];
                AnswerData answerData = new AnswerData
                {
                    text = answer.text,
                    characterId = answer.characterId,
                    targetNodeId = -1 // Se llenará con las conexiones
                };
                nodeData.answers.Add(answerData);
            }
            
            dialogueData.graph.nodes.Add(nodeData);
        }
        
        // Guardar conexiones
        foreach (var connection in editorWindow.connections)
        {
            ConnectionData connectionData = new ConnectionData
            {
                inPointNodeId = connection.inPoint.node.id,
                outPointNodeId = connection.outPoint.node.id
            };
            
            // Determinar si la conexión viene de una respuesta
            if (connection.outPoint.type == ConnectionPointType.Out)
            {
                // Buscar si esta conexión viene de alguna respuesta
                bool foundInAnswer = false;
                for (int i = 0; i < connection.outPoint.node.answers.Count; i++)
                {
                    var answer = connection.outPoint.node.answers[i];
                    if (answer.outPoint == connection.outPoint)
                    {
                        connectionData.isFromAnswer = true;
                        connectionData.answerIndex = i;
                        foundInAnswer = true;
                        
                        // Actualizar targetNodeId en la respuesta
                        var nodeData = dialogueData.GetNode(connection.outPoint.node.id);
                        if (nodeData != null && i < nodeData.answers.Count)
                        {
                            nodeData.answers[i].targetNodeId = connection.inPoint.node.id;
                        }
                        break;
                    }
                }
                
                // Si no viene de una respuesta, es la conexión principal del nodo
                if (!foundInAnswer)
                {
                    connectionData.isFromAnswer = false;
                    connectionData.answerIndex = -1;
                    
                    // Actualizar nextNodeId del nodo de origen
                    var nodeData = dialogueData.GetNode(connection.outPoint.node.id);
                    if (nodeData != null)
                    {
                        nodeData.nextNodeId = connection.inPoint.node.id;
                    }
                }
            }
            
            dialogueData.graph.connections.Add(connectionData);
        }
        
        // Guardar
        EditorUtility.SetDirty(dialogueData);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"Diálogo guardado en: {path}");
    }
    
    public void LoadDialogue(string path)
    {
        DialogueGraphData dialogueData = AssetDatabase.LoadAssetAtPath<DialogueGraphData>(path);
        
        if (dialogueData == null)
        {
            Debug.LogError($"No se encontró el diálogo en: {path}");
            return;
        }
        
        // Limpiar editor actual
        editorWindow.nodes.Clear();
        editorWindow.connections.Clear();
        
        // Cargar nodos
        foreach (var nodeData in dialogueData.graph.nodes)
        {
            // Crear nodo en el editor
            // (Necesitaríamos exponer el método CreateNode del editor)
        }
        
        // Cargar conexiones
        // (Implementación similar para reconstruir las conexiones)
        
        Debug.Log($"Diálogo cargado desde: {path}");
    }
}