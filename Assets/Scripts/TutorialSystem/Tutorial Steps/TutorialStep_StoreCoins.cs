using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Tutorial Step/Store Coins", fileName = "Tutorial Step - Store Coins")]


public class TutorialStep_StoreCoins : TutorialStep
{
    private Item_CoinStorage coinStorage;

    [SerializeField] private ItemDataSO itemCoinData;
    [SerializeField] private ItemDataSO itemStorageData;
    [SerializeField] private int targetAmount = 1;
    private int coinsInStorageAmount;
    private int totalCoins;


    public override void StartTask()
    {
        base.StartTask();


        coinStorage = ItemManager.instance.FindFirstItemWithComponent<Item_CoinStorage>();
        List<Item_Coin> avalibleCoinsList = ItemManager.instance.FindAllItemsWithComponent<Item_Coin>();

        StorageHolder_Coin.OnCoinAdded += HandleTask;

        TutorialIndicator.HighlightTarget<Item_DeliveryBox>();
        TutorialIndicator.HighlightTarget<StorageHolder_Coin>();



        for (int i = avalibleCoinsList.Count - 1; i >= 0; i--)
        {
            if (coinStorage.coinHolder.currentItems.Contains(avalibleCoinsList[i]))
                avalibleCoinsList.RemoveAt(i);
        }

        List<Transform> coinTransforms = avalibleCoinsList.Select(c => c.transform).ToList();


        TutorialIndicator.HighlightTargetList(coinTransforms);

        coinsInStorageAmount = coinStorage.coinHolder.GetCoinList().Count;
        targetAmount = avalibleCoinsList.Count;

        if (avalibleCoinsList.Count == 0 && coinStorage.coinHolder.currentItems.Count > 0)
        {
            TutorialIndicator.Clear();
            Complete();
            return;
        }


        UpdateCurrentGoalUI();
    }

    public override void StopTask()
    {
        StorageHolder_Coin.OnCoinAdded -= HandleTask;
    }

    public override void HandleTask() { }

    public void HandleTask(int coinsInStorage)
    {
        coinsInStorageAmount = coinsInStorage;

        if (coinsInStorage >= targetAmount)
        {
            TutorialIndicator.Clear();
            Complete();
            return;
        }

        UpdateCurrentGoalUI();
    }

    public override void UpdateCurrentGoalUI()
    {
        string text = $"{Localization.GetString("tutorial_step_store_coins")}: {coinsInStorageAmount}/{targetAmount}";
        UI.instance.inGameUI.UpdateCurrentGoal(text);
    }
}
