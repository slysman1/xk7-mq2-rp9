using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Dialogue Data/New Dialogue Node", fileName = "Node - ")]

public class DialogueNodeSO : ScriptableObject
{
    public DialogueNodeSO followingDialogue;
    public string nodeID;
    public string localizationKey;
    public DialogueActionType actionType;

    [TextArea] public string mainLine;
    public List<LineVariants> lineVariants;
    public List<DialogueChoice> dialogueChoices = new();

    public string GetRandomLine()
    {
        List<string> lines = new();

        if (!string.IsNullOrEmpty(mainLine))
            lines.Add(mainLine);

        if (lineVariants != null)
        {
            foreach (var variant in lineVariants)
            {
                if (!string.IsNullOrEmpty(variant.line))
                    lines.Add(variant.line);
            }
        }

        if (lines.Count == 0)
            return "";

        return lines[Random.Range(0, lines.Count)];
    }

    public DialogueActionType GetDialogueAction()
    {
        Debug.Log("I'm giving an acton type of " + nodeID + "   " + actionType.ToString());
        return actionType;
    }


#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(nodeID))
        {
            nodeID = System.Guid.NewGuid().ToString();
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif
}


[System.Serializable]
public class DialogueChoice
{
    [TextArea] public string playersReply;
    [TextArea] public string nextNodeMainLine;
    public DialogueActionType actionType;
    public DialogueNodeSO nextNode;
}

[System.Serializable]
public class LineVariants
{
    public string localizationKey;
    [TextArea] public string line;
}

public enum DialogueActionType { NoAction, OpenShop, OpenDecor, ConfirmDelivery}