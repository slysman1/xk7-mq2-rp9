using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Tutorial Step/Talk To Guard - Report Delivery",
    fileName = "Tutorial Step - Talk To Guard - Report Delivery")]
public class TutorialStep_TalkToGuard_ReportDelivery : TutorialStep_TalkToGuard
{
    private Order_DeliveryManager delivery;
    private int neededCoins;
    private System.Action<OrderDataSO> onScrollAdded;
    private System.Action<OrderDataSO> onScrollRemoved;

    public override void StartTask()
    {
        base.StartTask();

        delivery = FindFirstObjectByType<Order_DeliveryManager>();
        neededCoins = OrderManager.instance.trackedOrders[0].orderOutput[0].quantity;

        onScrollAdded = _ => CheckConditions();
        onScrollRemoved = _ => CheckConditions();

        OrderBoardHolder_Scroll.OnScrollAdded += onScrollAdded;
        OrderBoardHolder_Scroll.OnScrollRemoved += onScrollRemoved;
        DeliveryAreaHolder_AllItems.OnCoinAmountChanged += OnCoinAmountChanged;
        OrderManager.OnOrderCompleted += HandleTask;

        CheckConditions();
    }

    private void OnCoinAmountChanged(List<Item_Base> items) => CheckConditions();

    private void CheckConditions()
    {
        bool hasScroll = OrderManager.instance.trackedOrders.Count > 0;
        bool hasCoins = delivery.GetItemsInDeliveryArea().Count >= neededCoins;

        TutorialIndicator.Clear();

        if (!hasScroll)
        {
            TutorialIndicator.HighlightTarget<Item_OrderScroll>();
            TutorialIndicator.HighlightTarget<OrderBoardHolder_Scroll>();
        }
        else if (!hasCoins)
        {
            TutorialIndicator.HighlightTarget<DeliveryAreaHolder_AllItems>();
        }
        else
        {
            TutorialIndicator.HighlightTarget<MainNPC>();
        }

        UpdateCurrentGoalUI();
    }

    public override void UpdateCurrentGoalUI()
    {
        bool hasScroll = OrderManager.instance.trackedOrders.Count > 0;
        int currentCoins = delivery != null ? delivery.GetItemsInDeliveryArea().Count : 0;
        bool hasCoins = currentCoins >= neededCoins;

        string text;
        if (!hasScroll)
            text = Localization.GetString("tutorial_step_accept_order");
        else if (!hasCoins)
            text = $"{Localization.GetString("tutorial_step_add_coins_to_delivery_area")}: {currentCoins}/{neededCoins}";
        else
            text = Localization.GetString("tutorial_step_talk_to_guard");

        UI.instance.inGameUI.UpdateCurrentGoal(text);
    }

    public override void StopTask()
    {
        base.StopTask();
        OrderBoardHolder_Scroll.OnScrollAdded -= onScrollAdded;
        OrderBoardHolder_Scroll.OnScrollRemoved -= onScrollRemoved;
        DeliveryAreaHolder_AllItems.OnCoinAmountChanged -= OnCoinAmountChanged;
        OrderManager.OnOrderCompleted -= HandleTask;
    }
}