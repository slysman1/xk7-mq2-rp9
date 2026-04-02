using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager instance;
    public static event Action<int> OnCreditUpdate;
    public static event Action<int> OnRespectUpdate;

    [SerializeField] private int currentCredit;
    [SerializeField] private int currentRespect;



    private void Awake()
    {
        instance = this;
    }
    private IEnumerator Start()
    {
        yield return null;
        AddCurrency(0);
        AddFavour(0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F6))
            AddCurrency(1);

        if(Input.GetKeyDown(KeyCode.F5))
            AddFavour(1);
    }

    public void Pay(int totalCost, out List<GameObject> change)
    {
        List<StorageHolder_Coin> coinHolders = FindAllStorages();

        List<Item_Coin> coinsToUse = new();
        int remaining = totalCost;

        // Step 1: Collect coins from all storages
        foreach (var storage in coinHolders)
        {
            List<Item_Coin> coinList = storage.GetCoinList();

            for (int i = coinList.Count - 1; i >= 0 && remaining > 0; i--)
            {
                Item_Coin coin = coinList[i];

                coinsToUse.Add(coin);
                remaining = remaining - coin.GetCoinValue();

            }

            if (remaining <= 0)
                break;
        }

        // Step 2: Calculate change
        int paidAmount = coinsToUse.Sum(c => c.GetCoinValue());
        int changeAmount = paidAmount - totalCost;


        // Step 3: Remove coins from storages
        foreach (var coin in coinsToUse)
        {
            if (coin.GetItemHolder() is StorageHolder_Coin storage)
                storage.RemoveItem(coin, true);
        }

        // Step 4: Generate change
        change = GenerateChangeCoins(changeAmount);
    }

    private  List<StorageHolder_Coin> FindAllStorages()
    {
        List<Item_CoinStorage> itemStorages = ItemManager.instance.FindAllItemsWithComponent<Item_CoinStorage>();
        List<StorageHolder_Coin> coinHolders = new List<StorageHolder_Coin>();

        foreach (var item in itemStorages)
            coinHolders.Add(item.coinHolder);


        return coinHolders;
    }

    private List<GameObject> GenerateChangeCoins(int changeAmount)
    {
        int remaining = changeAmount;
        List<GameObject> changeCoins = new();

        var config = MetalConfig.Get();
        var sortedCoins = config.allCoinsData
            .Where(d => d != null)
            .OrderByDescending(d => d.creditValue)
            .ToList();

        foreach (var data in sortedCoins)
        {
            while (remaining >= data.creditValue)
            {
                remaining -= data.creditValue;
                GameObject newCoin = ItemManager.instance.CreateItem(data);
                Item_Coin coin = newCoin.GetComponent<Item_Coin>();
                coin.gameObject.SetActive(false);
                coin.EnableStamps(true);
                changeCoins.Add(newCoin);
            }
        }

        return changeCoins;
    }


    public bool HasCurrencyAmount(int totalPrice) => totalPrice <= currentCredit;
    public bool HasFavourAmout(int respectPrice) => respectPrice <= currentRespect;

    public int GetCurrentCredit() => currentCredit;


    public void AddCurrency(int amount)
    {
        currentCredit = currentCredit + amount;
        OnCreditUpdate?.Invoke(currentCredit);
    }

    public void RemoveCurrency(int amount)
    {
        currentCredit = currentCredit - amount;
        OnCreditUpdate?.Invoke(currentCredit);
    }

    public void AddFavour(int amount)
    {
        currentRespect = currentRespect + amount;
        OnRespectUpdate?.Invoke(currentRespect);
    }

    public void RemoveFavour(int amount)
    {
        currentRespect = Mathf.Max(0, currentRespect - amount);
        OnRespectUpdate?.Invoke(currentRespect);
    }

}
