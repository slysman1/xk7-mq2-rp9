using UnityEditor;
using UnityEngine;


[CreateAssetMenu(menuName = "Tutorial/Tutorial Step/Pick up item", fileName = "Tutorial Step - Pick Up Coin")]



public class TutorialStep_PickupCoins : TutorialStep
{
    private int coinsInHands;
    private int neededAmount;


    public void HandleTask(int amount)
    {
        
    }

    public override void HandleTask()
    {
        string text = $"{Localization.GetString("tutorial_step_pickup_stamped_coins")}: {0}/{neededAmount}";
        UI.instance.inGameUI.UpdateCurrentGoal(text);


        if (Player.instance.inventory.TryGetCoinsInHands(out int stampedCoins,out int unstampedCoins))
        {
            this.coinsInHands = stampedCoins;
            UpdateCurrentGoalUI();

            if (this.coinsInHands >= neededAmount)
                Complete();
        }
    }

    public override void StartTask()
    {
        base.StartTask();
        coinsInHands = 0;
        Player.instance.inventory.OnItemAmountUpdate += HandleTask;

        neededAmount = OrderManager.instance.trackedOrders[0].orderOutput[0].quantity;

        TutorialIndicator.HighlightTargets<Item_Coin>(neededAmount);        
        UpdateCurrentGoalUI();
    }

    public override void StopTask()
    {
        Player.instance.inventory.OnItemAmountUpdate -= HandleTask;
    }

    public override void UpdateCurrentGoalUI()
    {
        string text = $"{Localization.GetString("tutorial_step_pickup_stamped_coins")}: {coinsInHands}/{neededAmount}";
        UI.instance.inGameUI.UpdateCurrentGoal(text);
    }
}
