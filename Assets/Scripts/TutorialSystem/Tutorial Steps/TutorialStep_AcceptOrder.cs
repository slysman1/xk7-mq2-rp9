using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Tutorial Step/Accept Order", fileName = "Tutorial Step - Accept Order")]
public class TutorialStep_AcceptOrder : TutorialStep
{
    private System.Action<OrderDataSO> onScrollAdded;

    public override void HandleTask()
    {
        Complete();
    }

    public override void StartTask()
    {
        base.StartTask();

        if (OrderManager.instance.trackedOrders.Count > 0)
        {
            Complete();
            return;
        }

        onScrollAdded = _ => HandleTask();
        OrderBoardHolder_Scroll.OnScrollAdded += onScrollAdded;

        TutorialIndicator.HighlightTarget<Item_OrderScroll>();
        TutorialIndicator.HighlightTarget<OrderBoardHolder_Scroll>();
        TutorialIndicator.HighlightAllTargets<Item_DeliveryBox>();

        UpdateCurrentGoalUI();
    }

    public override void StopTask()
    {
        OrderBoardHolder_Scroll.OnScrollAdded -= onScrollAdded;
    }

    public override void UpdateCurrentGoalUI()
    {
        UI.instance.inGameUI.UpdateCurrentGoal(Localization.GetString("tutorial_step_accept_order"));
    }
}