using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Tutorial Step/Complete Orders", fileName = "Tutorial Step - Complete Orders")]


public class TutorialStep_CompleteOrders : TutorialStep
{
    [SerializeField] private UpgradeDataSO targetUpgrade;
    [SerializeField] private List<ItemDataSO> itemsToInject;
    [Space]
    [SerializeField] private int ordersToComplete = 1;
    private int completedOrders;

    [Space]
    [SerializeField] private bool ignoreOwnedCredits = true;
    [SerializeField] private int requiredCredits = 0;

    [Space]
    [SerializeField] private float waitTillHelpIndicator = 15f;
    private Coroutine helpCo;

    public override void StartTask()
    {
        base.StartTask();
        completedOrders = 0;

        if (targetUpgrade != null)
        {
            int upgradeCost = targetUpgrade.upgradeCost;

            if (!ignoreOwnedCredits)
            {
                // Calculate how many MORE credits needed after what's already in the room
                int ownedCredits = GetTotalCreditsInRoom();
                int creditsNeeded = Mathf.Max(0, upgradeCost - ownedCredits);

                // Calculate how many orders needed to get those credits
                ordersToComplete = OrderManager.instance.OrdersNeededToReach(creditsNeeded);
            }
            else
            {
                // Original behavior - calculate based on full upgrade cost
                ordersToComplete = OrderManager.instance.OrdersNeededToReach(upgradeCost);
            }
        }

        if (itemsToInject?.Count > 0)
        {
            int injectIndex = ordersToComplete - 1;
            OrderManager.instance.RegisterInjection(injectIndex, itemsToInject);
        }

        OrderManager.OnOrderCompleted += HandleTask;
        Order_RequestButton.OnOrderRequested += TutorialIndicator.Clear;

        if (waitTillHelpIndicator > 0)
            helpCo = TutorialManager.instance.StartCoroutine(EnableHelpIndicatorIfNeededCo());

        UpdateCurrentGoalUI();
    }

    public override void HandleTask()
    {
        completedOrders++;
        UpdateCurrentGoalUI();

        if (completedOrders >= ordersToComplete)
            Complete();
    }


    public override void StopTask()
    {
        if(helpCo != null)
            TutorialManager.instance.StopCoroutine(helpCo);

        OrderManager.OnOrderCompleted -= HandleTask;    
        Order_RequestButton.OnOrderRequested -= TutorialIndicator.Clear;
    }

    public override void UpdateCurrentGoalUI()
    {
        string text = $"{Localization.GetString("tutorial_step_complete_orders")}: {completedOrders}/{ordersToComplete}";
        UI.instance.inGameUI.UpdateCurrentGoal(text);
    }

    private IEnumerator EnableHelpIndicatorIfNeededCo()
    {
        if (OrderManager.instance.remainingOrders.Count > 0 || OrderManager.instance.trackedOrders.Count > 0)
            yield break;

        yield return new WaitForSeconds(waitTillHelpIndicator);
        TutorialIndicator.HighlightAllTargets<Order_RequestButton>();
    }

    private int GetTotalCreditsInRoom()
    {
        int total = 0;

        // Use ItemManager's existing tracking instead of FindObjectsByType
        List<Item_Coin> allCoins = ItemManager.instance.FindAllItemsWithComponent<Item_Coin>();

        foreach (var coin in allCoins)
        {
            total += coin.GetCoinValue();
        }

        return total;
    }

}
