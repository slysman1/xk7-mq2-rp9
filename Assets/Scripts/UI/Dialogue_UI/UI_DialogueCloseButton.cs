using UnityEngine;

public class UI_DialogueCloseButton : MonoBehaviour
{
    private UI_Dialogue dialogue;

    private void Awake()
    {
        dialogue = GetComponentInParent<UI_Dialogue>();
    }


    public void CloseDialogue() => dialogue.GetFirstDialogueChoice().ConfirmDialogueChoice();

}
