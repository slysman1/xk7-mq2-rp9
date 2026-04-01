using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Next Task Data/Task data - Pickup Broom", fileName = "Tutorial Task - 00 -")]
public class Task_PickupBroom : TutorialStep
{
    private bool pickedUpBroom;

    public override void StartTask()
    {
        base.StartTask();
        Tool_Broom.OnBroomPickedUp += HandleTask;
        UpdateCurrentGoalUI();
    }

    public override void StopTask()
    {
        Tool_Broom.OnBroomPickedUp -= HandleTask;
    }

    public override void HandleTask()
    {
        if (pickedUpBroom)
            return;

        pickedUpBroom = true;
        Complete();
    }


    public override void UpdateCurrentGoalUI()
    {
        string text = Localization.GetString("goal_take_broom");
        UI.instance.inGameUI.UpdateCurrentGoal(text);
    }
}
