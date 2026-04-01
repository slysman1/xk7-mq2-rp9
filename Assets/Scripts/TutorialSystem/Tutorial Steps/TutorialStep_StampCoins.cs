using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Tutorial Step/Stamp Coins", fileName = "Tutorial Step - Stamp Coins")]

public class TutorialStep_StampCoins : TutorialStep
{
    [SerializeField] private ItemDataSO itemCoinData;
    private int stampsReqruied;


    public override void HandleTask()
    {
        if (TaskCompleted())
        {
            Complete();
            return;
        }


        UpdateCurrentGoalUI();
    }

    public override void StartTask()
    {
        base.StartTask();

        stampsReqruied = ItemManager.instance.FindAllItems(itemCoinData).Count;


        Item_DeliveryBox.OnBoxOpened += HighlightCoinsAndStamp;
        Item_Coin.OnCoinStamped += HandleTask;
        TutorialIndicator.HighlightAllTargets<Item_DeliveryBox>();



        if (TaskCompleted())
            Complete();

        UpdateCurrentGoalUI();
    }

    public override void StopTask()
    {
        Item_DeliveryBox.OnBoxOpened -= HighlightCoinsAndStamp;
        Item_Coin.OnCoinStamped -= HandleTask;
    }

    public override void UpdateCurrentGoalUI()
    {
        
        string text = $"{Localization.GetString("tutorial_step_stamp_coins")}: {AmountOfStampedCoins()}/{stampsReqruied}";

        UI.instance.inGameUI.UpdateCurrentGoal(text);
    }

    private bool TaskCompleted() => AmountOfStampedCoins() >= stampsReqruied;

    private int AmountOfStampedCoins()
    {
        List<Transform> allCoins = ItemManager.instance.FindAllItems(itemCoinData);
        int stampedCoinsAmount = 0;

        foreach (var item in allCoins)
        {
            Item_Coin itemCoin = item.GetComponent<Item_Coin>();

            if (itemCoin != null && itemCoin.hasStamp)
                stampedCoinsAmount++;
        }

        return stampedCoinsAmount;
    }

    private void HighlightCoinsAndStamp()
    {
        TutorialIndicator.Clear();
        TutorialIndicator.HighlightAllTargets<Item_Coin>();
        TutorialIndicator.HighlightAllTargets<Tool_CoinStamp>();
    }
}
