
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Next Task Data/Task data - Place coins in delivery area", fileName = "Next task data - 00 -")]
public class Task_PlaceCoinsInDeliveryArea : TutorialStep
{
    private int amountOfCoinsInDeliveryArea;
    private int neededAmountOfCoinsInDeliveryArea;
    private Order_DeliveryManager delivery;

    [SerializeField] private ItemDataSO[] allCoinsData;

    public void HandleTask(List<Item_Base> itemsInDeliveryArea)
    {
        int coinsInDeliveryArea = 0;

        foreach (var item in itemsInDeliveryArea)
        {
            foreach(var data in allCoinsData)
                if(data == item.itemData)
                    coinsInDeliveryArea++;
        }

        amountOfCoinsInDeliveryArea = coinsInDeliveryArea;

        if (amountOfCoinsInDeliveryArea >= neededAmountOfCoinsInDeliveryArea)
        {
            Complete();
            return;
        }

        UpdateCurrentGoalUI();
    }

    public override void HandleTask()
    {
        //
    }


    public override void StartTask()
    {
        base.StartTask();

        if (delivery == null)
            delivery = FindFirstObjectByType<Order_DeliveryManager>();

        TutorialIndicator.HighlightTarget<DeliveryAreaHolder_AllItems>();

        DeliveryAreaHolder_AllItems.OnCoinAmountChanged += HandleTask;

        FindFirstObjectByType<DeliveryAreaHolder_AllItems>().Highlight(true);
        amountOfCoinsInDeliveryArea = delivery.GetItemsInDeliveryArea().Count;
        neededAmountOfCoinsInDeliveryArea = OrderManager.instance.trackedOrders[0].orderOutput[0].quantity;

        UpdateCurrentGoalUI();
    }

    public override void StopTask()
    {
        DeliveryAreaHolder_AllItems.OnCoinAmountChanged -= HandleTask;
        FindFirstObjectByType<DeliveryAreaHolder_AllItems>().Highlight(false); // needed?
    }

    public override void UpdateCurrentGoalUI()
    {
        string text = $"{Localization.GetString("tutorial_step_add_coins_to_delivery_area")}: {amountOfCoinsInDeliveryArea}/{neededAmountOfCoinsInDeliveryArea}";
        UI.instance.inGameUI.UpdateCurrentGoal(text);
    }
}
