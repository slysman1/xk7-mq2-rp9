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

    [Header("Coin Value Details")]
    [SerializeField] private Item_Coin copperCoin;
    [SerializeField] private Item_Coin silverCoin;
    [SerializeField] private Item_Coin goldenCoin;

    private void Awake()
    {
        instance = this;
        StorageHolder_Coin.OnCoinAdded += AddCurrency;
        StorageHolder_Coin.OnCoinRemoved += RemoveCurrency;
    }

    private IEnumerator Start()
    {
        yield return null;
        AddCurrency(0);
        AddRespect(0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F6))
            AddCurrency(1);

        if(Input.GetKeyDown(KeyCode.F5))
            AddRespect(1);
    }

    public void Pay(int totalCost, out List<GameObject> change)
    {
        List<StorageHolder_Coin> storages = FindObjectsByType<StorageHolder_Coin>(FindObjectsSortMode.None).ToList();
        List<Item_Coin> coinsToUse = new();
        //Dictionary<Item_Coin, ItemHolder> coinsToReturn = new();
        int remaining = totalCost;

        // Step 1: Collect coins from all storages
        foreach (var storage in storages)
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
                storage.RemoveItem(coin,true);
        }

        // Step 4: Generate change
        change = GenerateChangeCoins(changeAmount);
    }


    private List<GameObject> GenerateChangeCoins(int changeAmount)
    {
        int remaining = changeAmount;
        List<GameObject> changeCoins = new();

        List<(Item_Coin prefab, int value)> denominations = new()
    {
        (goldenCoin, goldenCoin.GetCoinValue()),    // coin value assigned on coin itself
        (silverCoin, silverCoin.GetCoinValue()),     
        (copperCoin, copperCoin.GetCoinValue())      
    };

        foreach (var (prefab, value) in denominations)
        {
            while (remaining >= value)
            {
                remaining -= value;

                var coin = Instantiate(prefab, Vector3.one * 999, Quaternion.identity);
                coin.gameObject.SetActive(false);
                coin.EnableStamps(true);
                changeCoins.Add(coin.gameObject);
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

    public void AddRespect(int amount)
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
