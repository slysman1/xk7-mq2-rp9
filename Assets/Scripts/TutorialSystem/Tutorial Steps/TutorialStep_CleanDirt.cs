using UnityEngine;


[CreateAssetMenu(menuName = "Tutorial/Tutorial Step/Clean Dirt", fileName = "Tutorial Step - Clean Dirt")]

public class TutorialStep_CleanDirt : TutorialStep
{
    [Tooltip("Actual value is taken from amount of spots created by DirtManager")]
    private int requiredCount;
    private int currentCount;

    public override void StartTask()
    {
        base.StartTask();
        currentCount = 0;
        requiredCount = DirtManager.instance.GetSpotCount();

        if (requiredCount == 0)
        {
            Complete();
            return;
        }

        DirtManager.OnSpotCleaned += HandleTask;


        TutorialIndicator.HighlightTarget<Tool_Broom>();
        TutorialIndicator.HighlightAllTargets<Item_DirtSpot>();


        UpdateCurrentGoalUI();
    }

    

    public override void StopTask()
    {
        DirtManager.OnSpotCleaned -= HandleTask;
    }

    public override void UpdateCurrentGoalUI()
    {
        string goalText = $"{Localization.GetString("tutorial_step_clean_dirt_spots")}: {currentCount}/{requiredCount}";
        UI.instance.inGameUI.UpdateCurrentGoal(goalText);
    }


    public override void HandleTask()
    {
        currentCount++;

        if (currentCount >= requiredCount)
        {
            Complete();
            return;
        }


        UpdateCurrentGoalUI();
    }
}
