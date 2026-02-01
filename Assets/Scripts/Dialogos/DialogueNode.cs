using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "NuevoNodo", menuName = "Dialogo/Nodo")]
public class DialogueNode : ScriptableObject
{
    public string id;
    [Header("Completed Required (OR)")]
    public List<DialogueNode> idCompletedRequired = new();
    [Header("Completed Required (AND)")]
    public List<DialogueNode> idCompletedRequired_and = new();
    public List<DialogueNode> nodesToComplete = new();
    public List<DialogueNode> nodesToRefuse = new();

    [Header("Info del Hablante")]
    public CharacterProfile speaker; // Quién habla
    [TextArea(3, 10)]
    public string dialogueText; // Lo que dice
    [TextArea(3, 10)]
    public string helpText; // Lo que dice

    public bool endAndComplete = true;

    [Header("Opciones del Player")]
    public List<DialogueOption> options; // Lista de respuestas posibles




    [Header("NO TOCAR!!!!")]
    public bool isCompleted = false;
    public bool isRefused = false;

    void OnDisable()
    {
        isCompleted = false;
        isRefused = false;
    }
}

// Clase auxiliar para definir las respuestas
[System.Serializable]
public class DialogueOption
{
    public string responseText; // Texto del botón (ej: "Sí, acepto")
    public DialogueNode nextNode; // A qué nodo lleva esta respuesta
}


public enum CharacterProfile
{
    Narrador,
    Player,
    Npc1,
    Npc2,
    Npc3,
    Npc4,
    Npc5,
    Npc6,
    Npc7,
    Npc8,
    Npc9,
}

