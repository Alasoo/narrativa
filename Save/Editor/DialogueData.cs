using System;
using System.Collections.Generic;
using UnityEngine;




[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue System/Dialogue")]
public class DialogueData : ScriptableObject
{
    public DialogueGraph graph = new DialogueGraph();
    
    public void SaveFromEditor(List<DialogueNode> nodes, List<Connection> connections)
    {
        graph.nodes.Clear();
        graph.connections.Clear();
        
        // Guardar nodos
        foreach (var node in nodes)
        {
            DialogueNodeData nodeData = new DialogueNodeData
            {
                id = node.id,
                position = node.rect.position,
                characterId = node.characterId,
                dialogueText = node.dialogueText
            };
            
            // Guardar respuestas
            foreach (var answer in node.answers)
            {
                AnswerData answerData = new AnswerData
                {
                    text = answer.text
                    // targetNodeId se establecería basándose en las conexiones
                };
                nodeData.answers.Add(answerData);
            }
            
            graph.nodes.Add(nodeData);
        }
        
        // Guardar conexiones (necesitarías más lógica para vincular respuestas con nodos)
        // Esta parte depende de cómo gestiones las conexiones específicas de respuestas
    }
}