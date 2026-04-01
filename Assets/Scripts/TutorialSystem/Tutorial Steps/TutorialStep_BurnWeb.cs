using UnityEngine;


[CreateAssetMenu(menuName = "Tutorial/Tutorial Step/Burn Web", fileName = "Tutorial Step - Burn Web")]

public class TutorialStep_BurnWeb : TutorialStep
{
    [Tooltip("Actual value is taken from amount of webs created by DirtManager")]
    private int requiredCount;
    private int currentCount;

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

    public override void StartTask()
    {
        base.StartTask();
        currentCount = 0;
        requiredCount = DirtManager.instance.GetWebsCount();

        if (requiredCount == 0)
        {
            Complete();
            return;
        }
        
        DirtManager.OnWebCleaned += HandleTask;
        TutorialIndicator.Clear();
        TutorialIndicator.HighlightAllTargets<Item_DirtWeb>();
        TutorialIndicator.HighlightTarget<Tool_Fire>(true);

        UpdateCurrentGoalUI();
    }

    public override void StopTask()
    {
        DirtManager.OnWebCleaned -= HandleTask;
    }

    public override void UpdateCurrentGoalUI()
    {
        string goalText = $"{Localization.GetString("tutorial_step_clean_web")}: {currentCount}/{requiredCount}";

        UI.instance.inGameUI.UpdateCurrentGoal(goalText);
    }
}
