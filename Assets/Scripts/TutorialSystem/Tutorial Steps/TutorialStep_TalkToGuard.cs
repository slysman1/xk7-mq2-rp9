using UnityEngine;


[CreateAssetMenu(menuName = "Tutorial/Tutorial Step/Talk To Guard", fileName = "Tutorial Step - Talk To Guard")]

public class TutorialStep_TalkToGuard : TutorialStep
{
    [SerializeField] private string goalOverride;
    public override void StartTask()
    {
        base.StartTask();
        TutorialIndicator.HighlightTarget<Quest_MainNPC>();

        UI_DialogueAnswerSlot.OnConversationFinished += HandleTask;
//        Item_DeliveryBox.OnBoxOpened += HandleTask;

        if (dialogueToStartNext != null)
            DialogueManager.instance.SetPriorityDialogue(dialogueToStartNext);

        UpdateCurrentGoalUI();
    }

    public override void HandleTask()
    {
        Complete();
        UI.instance.dialogueUI.ResetPriorityDialogue();
        TutorialIndicator.Clear();
    }

    public override void StopTask()
    {
        UI_DialogueAnswerSlot.OnConversationFinished -= HandleTask;
     //   Item_DeliveryBox.OnBoxOpened -= HandleTask;
    }

    public override void UpdateCurrentGoalUI()
    {
        string goalText = Localization.GetString(goalOverride == "" ? "tutorial_step_talk_to_guard" : goalOverride);
        UI.instance.inGameUI.UpdateCurrentGoal(goalText);
    }
}
