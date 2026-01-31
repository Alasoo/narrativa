using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;

public class DialogueNodeEditor : EditorWindow
{
    public List<DialogueNode> nodes = new List<DialogueNode>();
    public List<Connection> connections = new List<Connection>();
    private DialogueNode selectedNode = null;
    private ConnectionPoint selectedInPoint;
    private ConnectionPoint selectedOutPoint;
    private Vector2 offset;
    private Vector2 drag;
    private bool isPanning = false;
    private Vector2 panStart;
    private int nextNodeId = 1;
    private GUIStyle nodeStyle;
    private GUIStyle selectedNodeStyle;
    private GUIStyle inPointStyle;
    private GUIStyle outPointStyle;
    private string currentPath = "Assets/Dialogues/NewDialogue.asset";

    [MenuItem("Window/Dialogue Node Editor")]
    private static void OpenWindow()
    {
        DialogueNodeEditor window = GetWindow<DialogueNodeEditor>();
        window.titleContent = new GUIContent("Dialogue Node Editor");
        window.minSize = new Vector2(800, 600);
    }

    private void OnEnable()
    {
        nodeStyle = new GUIStyle();
        nodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
        nodeStyle.border = new RectOffset(12, 12, 12, 12);
        nodeStyle.padding = new RectOffset(20, 20, 20, 20);

        selectedNodeStyle = new GUIStyle();
        selectedNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D;
        selectedNodeStyle.border = new RectOffset(12, 12, 12, 12);
        selectedNodeStyle.padding = new RectOffset(20, 20, 20, 20);

        inPointStyle = new GUIStyle();
        inPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left.png") as Texture2D;
        inPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left on.png") as Texture2D;
        inPointStyle.border = new RectOffset(4, 4, 12, 12);

        outPointStyle = new GUIStyle();
        outPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right.png") as Texture2D;
        outPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right on.png") as Texture2D;
        outPointStyle.border = new RectOffset(4, 4, 12, 12);

        // Crear carpeta si no existe
        if (!AssetDatabase.IsValidFolder("Assets/Dialogues"))
        {
            AssetDatabase.CreateFolder("Assets", "Dialogues");
        }
    }

    private void OnGUI()
    {
        DrawGrid(20, 0.2f, Color.gray);
        DrawGrid(100, 0.4f, Color.gray);

        DrawConnections();
        DrawConnectionLine(Event.current);
        DrawNodes();

        DrawToolbar();

        ProcessNodeEvents(Event.current);
        ProcessEvents(Event.current);

        if (GUI.changed) Repaint();
    }

    private void DrawToolbar()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
        
        // Campo para la ruta
        GUILayout.Label("Ruta:", GUILayout.Width(35));
        currentPath = EditorGUILayout.TextField(currentPath, GUILayout.Width(250));
        
        // Botón Save
        if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(50)))
        {
            SaveDialogue();
        }
        
        // Botón Save As
        if (GUILayout.Button("Save As", EditorStyles.toolbarButton, GUILayout.Width(60)))
        {
            SaveDialogueAs();
        }
        
        // Botón Load
        if (GUILayout.Button("Load", EditorStyles.toolbarButton, GUILayout.Width(50)))
        {
            LoadDialogue();
        }
        
        GUILayout.FlexibleSpace();
        
        // Botón para crear nuevo diálogo
        if (GUILayout.Button("New", EditorStyles.toolbarButton, GUILayout.Width(50)))
        {
            if (EditorUtility.DisplayDialog("Nuevo Diálogo", 
                "¿Crear nuevo diálogo? Se perderán los cambios no guardados.", 
                "Sí", "No"))
            {
                nodes.Clear();
                connections.Clear();
                nextNodeId = 1;
                currentPath = "Assets/Dialogues/NewDialogue.asset";
                GUI.changed = true;
            }
        }
        
        GUILayout.EndHorizontal();
    }

    private void SaveDialogue()
    {
        if (string.IsNullOrEmpty(currentPath))
        {
            SaveDialogueAs();
            return;
        }

        try
        {
            // Crear o cargar el ScriptableObject existente
            DialogueGraphData dialogueData = null;
            
            if (File.Exists(currentPath))
            {
                dialogueData = AssetDatabase.LoadAssetAtPath<DialogueGraphData>(currentPath);
            }
            
            if (dialogueData == null)
            {
                dialogueData = ScriptableObject.CreateInstance<DialogueGraphData>();
                AssetDatabase.CreateAsset(dialogueData, currentPath);
            }
            
            // Limpiar datos existentes
            dialogueData.graph.nodes.Clear();
            dialogueData.graph.connections.Clear();
            
            // Guardar nodos
            foreach (var node in nodes)
            {
                DialogueNodeData nodeData = new DialogueNodeData
                {
                    id = node.id,
                    position = node.rect.position,
                    characterId = node.characterId,
                    dialogueText = node.dialogueText,
                    nextNodeId = -1
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
            
            // Guardar conexiones y actualizar referencias
            foreach (var connection in connections)
            {
                ConnectionData connectionData = new ConnectionData
                {
                    inPointNodeId = connection.inPoint.node.id,
                    outPointNodeId = connection.outPoint.node.id
                };
                
                // Determinar si la conexión viene de una respuesta
                bool isFromAnswer = false;
                int answerIndex = -1;
                
                // Buscar si esta conexión viene de alguna respuesta
                for (int i = 0; i < connection.outPoint.node.answers.Count; i++)
                {
                    var answer = connection.outPoint.node.answers[i];
                    if (answer.outPoint == connection.outPoint)
                    {
                        isFromAnswer = true;
                        answerIndex = i;
                        break;
                    }
                }
                
                connectionData.isFromAnswer = isFromAnswer;
                connectionData.answerIndex = answerIndex;
                
                // Actualizar referencias en los nodos
                var outNodeData = dialogueData.GetNode(connection.outPoint.node.id);
                var inNodeData = dialogueData.GetNode(connection.inPoint.node.id);
                
                if (outNodeData != null && inNodeData != null)
                {
                    if (isFromAnswer && answerIndex >= 0 && answerIndex < outNodeData.answers.Count)
                    {
                        // Es una conexión desde una respuesta
                        outNodeData.answers[answerIndex].targetNodeId = connection.inPoint.node.id;
                    }
                    else
                    {
                        // Es la conexión principal del nodo
                        outNodeData.nextNodeId = connection.inPoint.node.id;
                    }
                }
                
                dialogueData.graph.connections.Add(connectionData);
            }
            
            // Establecer nodo inicial (el primero creado o el de menor ID)
            if (dialogueData.graph.nodes.Count > 0)
            {
                dialogueData.graph.startNodeId = dialogueData.graph.nodes[0].id;
            }
            
            // Guardar
            EditorUtility.SetDirty(dialogueData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"Diálogo guardado en: {currentPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error al guardar diálogo: {e.Message}");
        }
    }

    private void SaveDialogueAs()
    {
        string newPath = EditorUtility.SaveFilePanelInProject(
            "Guardar Diálogo",
            "NewDialogue",
            "asset",
            "Guardar diálogo como...",
            "Assets/Dialogues");
        
        if (!string.IsNullOrEmpty(newPath))
        {
            currentPath = newPath;
            SaveDialogue();
        }
    }

    private void LoadDialogue()
    {
        string loadPath = EditorUtility.OpenFilePanel(
            "Cargar Diálogo",
            "Assets/Dialogues",
            "asset");
        
        if (!string.IsNullOrEmpty(loadPath))
        {
            // Convertir a ruta relativa del proyecto
            if (loadPath.StartsWith(Application.dataPath))
            {
                currentPath = "Assets" + loadPath.Substring(Application.dataPath.Length);
                LoadDialogueFromPath(currentPath);
            }
        }
    }

    private void LoadDialogueFromPath(string path)
    {
        DialogueGraphData dialogueData = AssetDatabase.LoadAssetAtPath<DialogueGraphData>(path);
        
        if (dialogueData == null)
        {
            Debug.LogError($"No se encontró el diálogo en: {path}");
            return;
        }
        
        // Limpiar editor actual
        nodes.Clear();
        connections.Clear();
        nextNodeId = 1;
        
        // Primero crear todos los nodos
        foreach (var nodeData in dialogueData.graph.nodes)
        {
            CreateNodeFromData(nodeData);
            nextNodeId = Mathf.Max(nextNodeId, nodeData.id + 1);
        }
        
        // Luego crear las conexiones
        foreach (var connectionData in dialogueData.graph.connections)
        {
            CreateConnectionFromData(connectionData);
        }
        
        Debug.Log($"Diálogo cargado desde: {path}");
        GUI.changed = true;
    }

    private void CreateNodeFromData(DialogueNodeData nodeData)
    {
        DialogueNode node = new DialogueNode(nodeData.id, nodeData.position, 400, 200, nodeStyle, selectedNodeStyle, 
            inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, OnNodeSelected,
            OnClickRemoveConnection);
        
        node.characterId = nodeData.characterId;
        node.dialogueText = nodeData.dialogueText;
        
        // Crear respuestas
        foreach (var answerData in nodeData.answers)
        {
            node.AddAnswerFromData(answerData);
        }
        
        nodes.Add(node);
    }

    private void CreateConnectionFromData(ConnectionData connectionData)
    {
        DialogueNode outNode = GetNodeById(connectionData.outPointNodeId);
        DialogueNode inNode = GetNodeById(connectionData.inPointNodeId);
        
        if (outNode != null && inNode != null)
        {
            ConnectionPoint outPoint = null;
            
            if (connectionData.isFromAnswer && connectionData.answerIndex >= 0 && 
                connectionData.answerIndex < outNode.answers.Count)
            {
                // Conexión desde una respuesta
                outPoint = outNode.answers[connectionData.answerIndex].outPoint;
            }
            else
            {
                // Conexión desde el nodo principal
                outPoint = outNode.outPoint;
            }
            
            if (outPoint != null)
            {
                Connection connection = new Connection(inNode.inPoint, outPoint, OnClickRemoveConnection);
                connections.Add(connection);
            }
        }
    }

    private DialogueNode GetNodeById(int id)
    {
        foreach (var node in nodes)
        {
            if (node.id == id) return node;
        }
        return null;
    }

    private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
    {
        int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
        int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

        Handles.BeginGUI();
        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        offset += drag * 0.5f;
        Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);

        for (int i = 0; i < widthDivs; i++)
        {
            Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset,
                new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
        }

        for (int j = 0; j < heightDivs; j++)
        {
            Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset,
                new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
        }

        Handles.color = Color.white;
        Handles.EndGUI();
    }

    private void DrawNodes()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            nodes[i].Draw();
        }
    }

    private void DrawConnections()
    {
        for (int i = 0; i < connections.Count; i++)
        {
            connections[i].Draw();
        }
    }

    private void DrawConnectionLine(Event e)
    {
        if (selectedInPoint != null && selectedOutPoint == null)
        {
            Handles.DrawBezier(
                selectedInPoint.rect.center,
                e.mousePosition,
                selectedInPoint.rect.center + Vector2.left * 50f,
                e.mousePosition - Vector2.left * 50f,
                Color.white,
                null,
                2f
            );

            GUI.changed = true;
        }

        if (selectedOutPoint != null && selectedInPoint == null)
        {
            Handles.DrawBezier(
                selectedOutPoint.rect.center,
                e.mousePosition,
                selectedOutPoint.rect.center - Vector2.left * 50f,
                e.mousePosition + Vector2.left * 50f,
                Color.white,
                null,
                2f
            );

            GUI.changed = true;
        }
    }

    private void ProcessEvents(Event e)
    {
        drag = Vector2.zero;

        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 2) // Rueda del ratón para pan
                {
                    isPanning = true;
                    panStart = e.mousePosition;
                    e.Use();
                }

                if (e.button == 1) // Click derecho para menú contextual
                {
                    ProcessContextMenu(e.mousePosition);
                    e.Use();
                }
                break;

            case EventType.MouseUp:
                if (e.button == 2)
                {
                    isPanning = false;
                    e.Use();
                }
                break;

            case EventType.MouseDrag:
                if (isPanning && e.button == 2) // Pan con rueda del ratón
                {
                    Vector2 delta = e.mousePosition - panStart;
                    panStart = e.mousePosition;
                    OnDrag(delta);
                    e.Use();
                }
                break;

            case EventType.KeyDown:
                if (e.keyCode == KeyCode.Delete && selectedNode != null)
                {
                    DeleteNode(selectedNode);
                    e.Use();
                }
                break;
        }
    }

    private void ProcessNodeEvents(Event e)
    {
        for (int i = nodes.Count - 1; i >= 0; i--)
        {
            bool guiChanged = nodes[i].ProcessEvents(e);

            if (guiChanged)
            {
                GUI.changed = true;
            }
        }
    }

    private void ProcessContextMenu(Vector2 mousePosition)
    {
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("Create Node"), false, () => OnClickCreateNode(mousePosition));
        genericMenu.ShowAsContext();
    }

    private void OnDrag(Vector2 delta)
    {
        drag = delta;

        for (int i = 0; i < nodes.Count; i++)
        {
            nodes[i].Drag(delta);
        }

        GUI.changed = true;
    }

    private void OnClickCreateNode(Vector2 position)
    {
        CreateNode(position);
    }

    private void CreateNode(Vector2 position)
    {
        DialogueNode node = new DialogueNode(nextNodeId++, position, 400, 200, nodeStyle, selectedNodeStyle, 
            inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, OnNodeSelected,
            OnClickRemoveConnection);
        nodes.Add(node);
        GUI.changed = true;
    }

    private void DeleteNode(DialogueNode node)
    {
        // Remove connections related to this node
        List<Connection> connectionsToRemove = new List<Connection>();

        for (int i = 0; i < connections.Count; i++)
        {
            if (connections[i].inPoint.node == node || 
                connections[i].outPoint.node == node)
            {
                connectionsToRemove.Add(connections[i]);
            }
            
            // Also check connections from node's answers
            foreach (var answer in node.answers)
            {
                if (connections[i].outPoint == answer.outPoint)
                {
                    connectionsToRemove.Add(connections[i]);
                }
            }
        }

        for (int i = 0; i < connectionsToRemove.Count; i++)
        {
            connections.Remove(connectionsToRemove[i]);
        }

        nodes.Remove(node);
        if (selectedNode == node) selectedNode = null;
        GUI.changed = true;
    }

    private void OnNodeSelected(DialogueNode node)
    {
        selectedNode = node;
    }

    private void OnClickInPoint(ConnectionPoint inPoint)
    {
        selectedInPoint = inPoint;

        if (selectedOutPoint != null)
        {
            if (selectedOutPoint.node != selectedInPoint.node)
            {
                CreateConnection();
                ClearConnectionSelection();
            }
            else
            {
                ClearConnectionSelection();
            }
        }
    }

    private void OnClickOutPoint(ConnectionPoint outPoint)
    {
        selectedOutPoint = outPoint;

        if (selectedInPoint != null)
        {
            if (selectedOutPoint.node != selectedInPoint.node)
            {
                CreateConnection();
                ClearConnectionSelection();
            }
            else
            {
                ClearConnectionSelection();
            }
        }
    }

    private void CreateConnection()
    {
        connections.Add(new Connection(selectedInPoint, selectedOutPoint, OnClickRemoveConnection));
        GUI.changed = true;
    }

    private void OnClickRemoveConnection(Connection connection)
    {
        connections.Remove(connection);
        GUI.changed = true;
    }

    private void ClearConnectionSelection()
    {
        selectedInPoint = null;
        selectedOutPoint = null;
    }

    private void OnClickRemoveNode(DialogueNode node)
    {
        DeleteNode(node);
    }
}

public class DialogueNode
{
    public int id;
    public Rect rect;
    public string characterId = "";
    public string dialogueText = "";
    public List<Answer> answers = new List<Answer>();
    public bool isSelected = false;

    public ConnectionPoint inPoint;
    public ConnectionPoint outPoint;

    private GUIStyle style;
    private GUIStyle selectedStyle;
    private GUIStyle inPointStyle;
    private GUIStyle outPointStyle;

    private Action<DialogueNode> OnRemoveNode;
    private Action<DialogueNode> OnSelectNode;
    private Action<ConnectionPoint> OnClickInPoint;
    private Action<ConnectionPoint> OnClickOutPoint;
    private Action<Connection> OnClickRemoveConnection;

    private float baseHeight = 200f;
    private float answerHeight = 120f;
    private Vector2 scrollPosition;

    public DialogueNode(int id, Vector2 position, float width, float height, GUIStyle nodeStyle, 
        GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle,
        Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint,
        Action<DialogueNode> OnRemoveNode, Action<DialogueNode> OnSelectNode,
        Action<Connection> OnClickRemoveConnection)
    {
        this.id = id;
        this.rect = new Rect(position.x, position.y, width, height);
        this.style = nodeStyle;
        this.selectedStyle = selectedStyle;
        this.inPointStyle = inPointStyle;
        this.outPointStyle = outPointStyle;
        this.OnClickInPoint = OnClickInPoint;
        this.OnClickOutPoint = OnClickOutPoint;
        this.OnRemoveNode = OnRemoveNode;
        this.OnSelectNode = OnSelectNode;
        this.OnClickRemoveConnection = OnClickRemoveConnection;

        inPoint = new ConnectionPoint(this, ConnectionPointType.In, inPointStyle, OnClickInPoint);
        outPoint = new ConnectionPoint(this, ConnectionPointType.Out, outPointStyle, OnClickOutPoint);
    }

    public void Drag(Vector2 delta)
    {
        rect.position += delta;
        
        // Actualizar también la posición de los puntos de conexión
        inPoint.UpdatePosition();
        outPoint.UpdatePosition();
        
        // Actualizar posición de las respuestas
        foreach (var answer in answers)
        {
            answer.UpdateConnectionPointPosition();
        }
    }

    public void Draw()
    {
        // Calcular altura dinámica basada en el número de respuestas
        float calculatedHeight = baseHeight + (answers.Count * answerHeight);
        rect.height = calculatedHeight;

        // Dibujar puntos de conexión principales
        inPoint.Draw();
        outPoint.Draw();

        // Dibujar estilo del nodo
        GUIStyle currentStyle = isSelected ? selectedStyle : style;
        GUI.Box(rect, "", currentStyle);

        // Área de contenido del nodo con ScrollView
        Rect contentRect = new Rect(rect.x + 10, rect.y + 10, rect.width - 20, rect.height - 20);
        GUILayout.BeginArea(contentRect);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(contentRect.width), GUILayout.Height(contentRect.height));
        
        // Header con node ID
        EditorGUILayout.LabelField($"Nodo #{id}", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        // Character ID field
        EditorGUILayout.LabelField("Personaje que habla:");
        characterId = EditorGUILayout.TextField(characterId, GUILayout.Height(20));
        EditorGUILayout.Space(5);

        // Dialogue text field
        EditorGUILayout.LabelField("Diálogo:");
        dialogueText = EditorGUILayout.TextArea(dialogueText, GUILayout.Height(60));
        EditorGUILayout.Space(10);

        // Answers section
        EditorGUILayout.LabelField("Respuestas:", EditorStyles.boldLabel);
        
        // Lista de respuestas existentes
        for (int i = 0; i < answers.Count; i++)
        {
            answers[i].Draw(i);
        }
        
        // Botón para añadir nueva respuesta
        if (GUILayout.Button("+ Añadir Respuesta", GUILayout.Height(25)))
        {
            AddAnswer();
        }

        EditorGUILayout.EndScrollView();
        GUILayout.EndArea();
        
        // Dibujar puntos de conexión de las respuestas
        foreach (var answer in answers)
        {
            answer.DrawConnectionPoint();
        }
    }

    public bool ProcessEvents(Event e)
    {
        bool guiChanged = false;

        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0) // Click izquierdo para seleccionar/mover nodo
                {
                    if (rect.Contains(e.mousePosition))
                    {
                        isSelected = true;
                        OnSelectNode?.Invoke(this);
                        guiChanged = true;
                        e.Use();
                    }
                    else
                    {
                        isSelected = false;
                    }
                }

                if (e.button == 1 && rect.Contains(e.mousePosition)) // Click derecho para menú
                {
                    ProcessContextMenu();
                    e.Use();
                }
                break;

            case EventType.MouseDrag:
                if (e.button == 0 && isSelected) // Mover nodo con click izquierdo
                {
                    Drag(e.delta);
                    guiChanged = true;
                    e.Use();
                }
                break;

            case EventType.MouseUp:
                if (e.button == 0)
                {
                    guiChanged = true;
                }
                break;
        }

        // Procesar eventos de las respuestas
        foreach (var answer in answers)
        {
            if (answer.ProcessEvents(e))
            {
                guiChanged = true;
            }
        }

        return guiChanged;
    }

    private void ProcessContextMenu()
    {
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("Remove Node"), false, () => OnRemoveNode?.Invoke(this));
        genericMenu.ShowAsContext();
    }

    public void AddAnswer()
    {
        answers.Add(new Answer(this, outPointStyle, OnClickOutPoint, OnRemoveAnswer, OnClickRemoveConnection));
    }

    public void AddAnswerFromData(AnswerData answerData)
    {
        var answer = new Answer(this, outPointStyle, OnClickOutPoint, OnRemoveAnswer, OnClickRemoveConnection);
        answer.text = answerData.text;
        answer.characterId = answerData.characterId;
        answers.Add(answer);
    }

    private void OnRemoveAnswer(Answer answer)
    {
        answers.Remove(answer);
    }

    public void RemoveAnswer(Answer answer)
    {
        OnRemoveAnswer(answer);
    }
}

public class Answer
{
    public string text = "";
    public string characterId = "";
    public ConnectionPoint outPoint;
    private DialogueNode parentNode;
    private GUIStyle outPointStyle;
    private Action<ConnectionPoint> OnClickOutPoint;
    private Action<Answer> OnRemoveAnswer;
    private Action<Connection> OnClickRemoveConnection;
    private Rect connectionPointRect;
    private Rect titleRect;
    private int targetNodeId = -1;
    private int answerIndex;

    public Answer(DialogueNode parentNode, GUIStyle outPointStyle, Action<ConnectionPoint> OnClickOutPoint, 
        Action<Answer> OnRemoveAnswer, Action<Connection> OnClickRemoveConnection)
    {
        this.parentNode = parentNode;
        this.outPointStyle = outPointStyle;
        this.OnClickOutPoint = OnClickOutPoint;
        this.OnRemoveAnswer = OnRemoveAnswer;
        this.OnClickRemoveConnection = OnClickRemoveConnection;
        
        // Obtener el índice de esta respuesta
        this.answerIndex = parentNode.answers.Count;
        
        // Crear punto de conexión para esta respuesta
        this.outPoint = new ConnectionPoint(parentNode, ConnectionPointType.Out, outPointStyle, OnClickOutPoint);
    }

    public void Draw(int index)
    {
        this.answerIndex = index;
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        // Título de la respuesta en horizontal con el punto de conexión
        EditorGUILayout.BeginHorizontal();
        
        // Título
        EditorGUILayout.LabelField($"Respuesta {index + 1}", EditorStyles.miniBoldLabel, GUILayout.ExpandWidth(true));
        
        // Guardar posición del título para el punto de conexión
        if (Event.current.type == EventType.Repaint)
        {
            titleRect = GUILayoutUtility.GetLastRect();
            titleRect.y += parentNode.rect.y + 10; // Ajustar para posición global
            titleRect.x += parentNode.rect.x + 10; // Ajustar para posición global
            titleRect.height = 20;
            
            // Calcular posición del punto de conexión
            UpdateConnectionPointPosition();
        }
        
        EditorGUILayout.EndHorizontal();
        
        // Texto de la respuesta
        EditorGUILayout.LabelField("Texto:", EditorStyles.miniLabel);
        text = EditorGUILayout.TextField(text, GUILayout.Height(40));
        
        // Character que responde
        EditorGUILayout.LabelField("Personaje que responde:", EditorStyles.miniLabel);
        characterId = EditorGUILayout.TextField(characterId);
        
        // Información de conexión
        if (targetNodeId > 0)
        {
            EditorGUILayout.LabelField($"→ Conectado a Nodo #{targetNodeId}", EditorStyles.miniLabel);
        }
        else
        {
            EditorGUILayout.LabelField("→ Sin conexión", EditorStyles.miniLabel);
        }
        
        // Botón para eliminar
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("X Eliminar", GUILayout.Width(80)))
        {
            OnRemoveAnswer?.Invoke(this);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }

    public void DrawConnectionPoint()
    {
        // Solo dibujar si tenemos un rectángulo válido
        if (connectionPointRect.width > 0 && connectionPointRect.height > 0)
        {
            // Actualizar posición del punto de conexión
            outPoint.UpdatePosition(connectionPointRect.center);
            
            // Dibujar el punto de conexión
            if (GUI.Button(connectionPointRect, "", outPointStyle))
            {
                OnClickOutPoint?.Invoke(outPoint);
            }
        }
    }

    public void UpdateConnectionPointPosition()
    {
        if (titleRect.width > 0 && titleRect.height > 0)
        {
            // Posicionar el punto de conexión justo después del título
            // En el lado derecho del nodo, alineado verticalmente con el título
            connectionPointRect = new Rect(
                parentNode.rect.x + parentNode.rect.width - 25, // 25px desde el borde derecho
                titleRect.y + (titleRect.height / 2) - 10,      // Centro vertical del título
                20,
                20
            );
        }
        else
        {
            // Fallback: calcular posición basada en el índice
            float baseY = parentNode.rect.y + 180; // Posición base debajo del diálogo
            float yOffset = answerIndex * 120; // 120px por cada respuesta
            
            connectionPointRect = new Rect(
                parentNode.rect.x + parentNode.rect.width - 25,
                baseY + yOffset + 20, // +20 para alinear con el título
                20,
                20
            );
        }
    }

    public bool ProcessEvents(Event e)
    {
        // Verificar si se hizo click en el punto de conexión
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            if (connectionPointRect.Contains(e.mousePosition))
            {
                OnClickOutPoint?.Invoke(outPoint);
                e.Use();
                return true;
            }
        }
        return false;
    }

    public void SetTargetNodeId(int nodeId)
    {
        targetNodeId = nodeId;
    }

    public void ClearTargetNode()
    {
        targetNodeId = -1;
    }
}

public enum ConnectionPointType { In, Out }

public class ConnectionPoint
{
    public Rect rect;
    public ConnectionPointType type;
    public DialogueNode node;
    private GUIStyle style;
    private Action<ConnectionPoint> OnClick;

    public ConnectionPoint(DialogueNode node, ConnectionPointType type, GUIStyle style, Action<ConnectionPoint> OnClick)
    {
        this.node = node;
        this.type = type;
        this.style = style;
        this.OnClick = OnClick;
        UpdatePosition();
    }

    public void UpdatePosition()
    {
        if (type == ConnectionPointType.In)
        {
            rect = new Rect(
                node.rect.x - 10,
                node.rect.y + node.rect.height / 2 - 10,
                20,
                20
            );
        }
        else
        {
            rect = new Rect(
                node.rect.x + node.rect.width - 10,
                node.rect.y + node.rect.height / 2 - 10,
                20,
                20
            );
        }
    }

    public void UpdatePosition(Vector2 position)
    {
        rect = new Rect(position.x - 10, position.y - 10, 20, 20);
    }

    public void Draw()
    {
        if (GUI.Button(rect, "", style))
        {
            OnClick?.Invoke(this);
        }
    }
}

public class Connection
{
    public ConnectionPoint inPoint;
    public ConnectionPoint outPoint;
    private Action<Connection> OnClickRemoveConnection;

    public Connection(ConnectionPoint inPoint, ConnectionPoint outPoint, Action<Connection> OnClickRemoveConnection)
    {
        this.inPoint = inPoint;
        this.outPoint = outPoint;
        this.OnClickRemoveConnection = OnClickRemoveConnection;
        
        // Actualizar información de nodo objetivo si es una conexión de respuesta
        if (outPoint.type == ConnectionPointType.Out && outPoint.node != null)
        {
            // Buscar la respuesta correspondiente y actualizar su targetNodeId
            if (outPoint.node is DialogueNode dialogueNode)
            {
                foreach (var answer in dialogueNode.answers)
                {
                    if (answer.outPoint == outPoint && inPoint.node != null)
                    {
                        answer.SetTargetNodeId(inPoint.node.id);
                        break;
                    }
                }
            }
        }
    }

    public void Draw()
    {
        if (inPoint == null || outPoint == null) return;

        Handles.DrawBezier(
            inPoint.rect.center,
            outPoint.rect.center,
            inPoint.rect.center + Vector2.left * 50f,
            outPoint.rect.center - Vector2.left * 50f,
            Color.white,
            null,
            2f
        );

        // Draw a button in the middle to remove the connection
        Vector2 centerPoint = (inPoint.rect.center + outPoint.rect.center) * 0.5f;
        Rect centerRect = new Rect(centerPoint.x - 12, centerPoint.y - 12, 24, 24);

        if (GUI.Button(centerRect, "X"))
        {
            // Limpiar targetNodeId si es una conexión de respuesta
            if (outPoint.type == ConnectionPointType.Out && outPoint.node is DialogueNode dialogueNode)
            {
                foreach (var answer in dialogueNode.answers)
                {
                    if (answer.outPoint == outPoint)
                    {
                        answer.ClearTargetNode();
                        break;
                    }
                }
            }
            
            OnClickRemoveConnection?.Invoke(this);
        }
    }
}

// ScriptableObject para guardar diálogos (Debe estar en un archivo separado: DialogueGraphData.cs)
// Pero lo incluyo aquí como referencia


[System.Serializable]
public class DialogueGraph
{
    public List<DialogueNodeData> nodes = new List<DialogueNodeData>();
    public List<ConnectionData> connections = new List<ConnectionData>();
    public int startNodeId = 1;
}

