using UnityEngine;


[CreateAssetMenu(menuName = "Tutorial/Tutorial Step/Accept Order", fileName = "Tutorial Step - Accept Order")]


public class TutorialStep_AcceptOrder : TutorialStep
{
    private bool scrollAttachedToBoard;

    public override void HandleTask()
    {
        scrollAttachedToBoard = true;
        UpdateCurrentGoalUI();

        if (scrollAttachedToBoard)
            Complete();
    }

    public override void StartTask()
    {
        base.StartTask();

        if(OrderManager.instance.trackedOrders.Count > 0)
            Complete();
        

        scrollAttachedToBoard = false;
        Holder_QuestTrackers.OnScrollAttached += HandleTask;

        TutorialIndicator.HighlightTarget<Item_QuestScroll>();
        TutorialIndicator.HighlightTarget<Holder_QuestTrackers>();

        UpdateCurrentGoalUI();

        TutorialIndicator.HighlightAllTargets<Item_DeliveryBox>();
    }

    public override void StopTask()
    {
        Holder_QuestTrackers.OnScrollAttached -= HandleTask;
    }

    public override void UpdateCurrentGoalUI()
    {
        string text = Localization.GetString("tutorial_step_accept_order");
        UI.instance.inGameUI.UpdateCurrentGoal(text);
    }

}
