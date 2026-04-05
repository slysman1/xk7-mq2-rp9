using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Tutorial Step/Clean Dirt And Web", fileName = "Tutorial Step - Clean Dirt And Web")]
public class TutorialStep_CleanDirtAndWeb : TutorialStep
{
    private int dirtsRequired;
    private int dirtsCount;
    private int websRequired;
    private int websCount;

    public override void StartTask()
    {
        base.StartTask();
        dirtsCount = 0;
        websCount = 0;
        dirtsRequired = DirtManager.instance.GetSpotCount();
        websRequired = DirtManager.instance.GetWebsCount();

        DirtManager.OnSpotCleaned += OnDirtCleaned;
        DirtManager.OnWebCleaned += OnWebCleaned;

        if (dirtsRequired == 0 && websRequired == 0)
        {
            Complete();
            return;
        }

        UpdateIndicators();
        UpdateCurrentGoalUI();
    }

   

    private bool AllDirtCleaned() => dirtsRequired == 0 || dirtsCount >= dirtsRequired;

    private void UpdateIndicators()
    {
        TutorialIndicator.Clear();

        if (AllDirtCleaned() == false)
        {
            TutorialIndicator.HighlightTarget<Tool_Broom>();
            TutorialIndicator.HighlightAllTargets<Item_DirtSpot>();
        }
        else
        {
            TutorialIndicator.HighlightAllTargets<Item_DirtWeb>();
            TutorialIndicator.HighlightTarget<Tool_Fire>(true);
        }
    }


    public override void HandleTask() { }

    public override void StopTask()
    {
        DirtManager.OnSpotCleaned -= OnDirtCleaned;
        DirtManager.OnWebCleaned -= OnWebCleaned;
    }

    public override void UpdateCurrentGoalUI()
    {
        string text = !AllDirtCleaned()
            ? $"{Localization.GetString("tutorial_step_clean_dirt_spots")}: {dirtsCount}/{dirtsRequired}"
            : $"{Localization.GetString("tutorial_step_clean_web")}: {websCount}/{websRequired}";

        UI.instance.inGameUI.UpdateCurrentGoal(text);
    }

    private void OnDirtCleaned()
    {
        dirtsCount++;
        if (CheckComplete())
            return;   
        
        UpdateIndicators();
        UpdateCurrentGoalUI();
    }

    private void OnWebCleaned()
    {
        websCount++;
        if (CheckComplete())
            return;

        UpdateIndicators();
        UpdateCurrentGoalUI();
    }

    private bool CheckComplete()
    {
        bool allDone = dirtsCount >= dirtsRequired && websCount >= websRequired;
        if (allDone)
        {
            Complete();
            return true;
        }
        return false;
    }
}