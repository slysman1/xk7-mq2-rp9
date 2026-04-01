using UnityEngine;


[CreateAssetMenu(menuName = "Tutorial/Tutorial Step/Request Order", fileName = "Tutorial Step - Request Order")]


public class TutorialStep_RequestOrder : TutorialStep
{

    public override void HandleTask()
    {
        Complete();
    }

    public override void StartTask()
    {
        base.StartTask();



        TutorialIndicator.HighlightAllTargets<Quest_StartButton>();
        Quest_StartButton.OnQuestRequested += HandleTask;
        UpdateCurrentGoalUI();
    }

    public override void StopTask()
    {
        Quest_StartButton.OnQuestRequested -= HandleTask;
    }

    public override void UpdateCurrentGoalUI()
    {

        string text = Localization.GetString("tutorial_step_request_order");
        UI.instance.inGameUI.UpdateCurrentGoal(text);
    }
}
