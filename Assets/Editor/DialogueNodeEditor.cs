using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

[CustomEditor(typeof(DialogueNodeSO))]
public class DialogueNodeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Sync / Auto-Create Choices"))
            SyncChoices();

        GUILayout.Space(20);

        DialogueNodeSO node = (DialogueNodeSO)target;

        EditorGUILayout.LabelField("Dialogue Tree Preview", EditorStyles.boldLabel);

        DrawNodeTree(node);

        GUILayout.Space(10);

        
    }
    void DrawNodeTree(DialogueNodeSO node, int indent = 0, HashSet<DialogueNodeSO> visited = null)
    {
        if (node == null)
            return;

        if (visited == null)
            visited = new HashSet<DialogueNodeSO>();

        if (visited.Contains(node))
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(indent * 20);
            EditorGUILayout.LabelField("(Loop detected)");
            EditorGUILayout.EndHorizontal();
            return;
        }

        visited.Add(node);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(indent * 20);

        if (GUILayout.Button(node.mainLine))
        {
            Selection.activeObject = node;
        }

        EditorGUILayout.EndHorizontal();

        if (node.dialogueChoices == null)
            return;

        for (int i = 0; i < node.dialogueChoices.Count; i++)
        {
            var choice = node.dialogueChoices[i];

            // draw divider only for MAIN choices
            if (indent == 0 && i > 0)
            {
                EditorGUILayout.Space(6);
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                EditorGUILayout.Space(6);
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space((indent + 1) * 20);
            EditorGUILayout.LabelField($"[{i}] {choice.playersReply}");
            EditorGUILayout.EndHorizontal();

            if (choice.nextNode != null)
                DrawNodeTree(choice.nextNode, indent + 2, visited);
        }
    }

    private void SyncChoices()
    {
        DialogueNodeSO node = (DialogueNodeSO)target;

        string nodePath = AssetDatabase.GetAssetPath(node);
        string folderPath = Path.GetDirectoryName(nodePath);

        string conversationName = Path.GetFileNameWithoutExtension(nodePath);

        int newNodeIndex = FindHighestNodeIndex(folderPath) + 1;

        for (int i = 0; i < node.dialogueChoices.Count; i++)
        {
            var choice = node.dialogueChoices[i];

            string line = choice.nextNodeMainLine;
            DialogueNodeSO targetNode = choice.nextNode;

            if (targetNode == null)
            {
                string nodeName = $"{conversationName}_N{newNodeIndex:00}";

                targetNode = ScriptableObject.CreateInstance<DialogueNodeSO>();
                targetNode.nodeID = System.Guid.NewGuid().ToString();

                string newPath = $"{folderPath}/{nodeName}.asset";
                AssetDatabase.CreateAsset(targetNode, newPath);

                choice.nextNode = targetNode;

                newNodeIndex++; // increment ONLY when creating
            }

            targetNode.mainLine = line;
            targetNode.actionType = choice.actionType;

            EditorUtility.SetDirty(targetNode);
        }

        EditorUtility.SetDirty(node);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Dialogue nodes synced and named.");
    }


    private int ExtractNodeIndex(string nodeName)
    {
        if (string.IsNullOrEmpty(nodeName))
            return -1;

        int indexStart = nodeName.IndexOf("_N");

        if (indexStart == -1)
            return -1;

        indexStart += 2;

        int indexEnd = indexStart;

        while (indexEnd < nodeName.Length && char.IsDigit(nodeName[indexEnd]))
            indexEnd++;

        string number = nodeName.Substring(indexStart, indexEnd - indexStart);

        if (int.TryParse(number, out int result))
            return result;

        return -1;
    }

    int FindHighestNodeIndex(string folder)
    {
        int max = -1;

        string[] guids = AssetDatabase.FindAssets("t:DialogueNodeSO", new[] { folder });

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string name = Path.GetFileNameWithoutExtension(path);

            int index = ExtractNodeIndex(name);

            if (index > max)
                max = index;
        }

        return max;
    }
}