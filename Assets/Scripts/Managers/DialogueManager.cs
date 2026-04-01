using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    public DialogueNodeSO priorityDialogue;
    public DialogueNodeSO mainDialogue;
    public DialogueNodeSO shopRejectDialogue;


    private void Awake()
    {
        instance = this;

    }

    private void Start()
    {
        
    }


    public DialogueNodeSO GetNextDialogue()
    {
        if (priorityDialogue != null)
            return priorityDialogue;

        return mainDialogue;
    }

    public void SetPriorityDialogue(DialogueNodeSO newDialogue) => priorityDialogue = newDialogue;
}
