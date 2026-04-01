using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StorageHolder_Coin : ItemHolder
{
    
    public static event Action<int> OnCoinAdded;
    public static event Action<int> OnCoinAmountChanged;
    public static event Action<int> OnCoinRemoved;
    private Item_CoinStorage storage;

    [Header("Slots")]
    private List<StorageHolder_CoinSlot> slots;
    
    protected override void Awake()
    {
        base.Awake();
        slots = SetupSlots<StorageHolder_CoinSlot>();
        storage = GetComponentInParent<Item_CoinStorage>();
    }

    public override void Highlight(bool enable)
    {
        base.Highlight(enable);
        storage.Highlight(enable);
    }

    protected override void OnItemAdded(Item_Base item)
    {
        base.OnItemAdded(item);

        Audio.PlaySFX("chest_coin_added", transform);
        OnCoinAdded?.Invoke(item.itemData.creditValue);
        OnCoinAmountChanged?.Invoke(currentItems.Count);
        RepositionCoins();
    }

    protected override void OnItemRemoved(Item_Base item)
    {
        base.OnItemRemoved(item);

        OnCoinRemoved?.Invoke(item.itemData.creditValue);
        OnCoinAmountChanged?.Invoke(currentItems.Count);
        RepositionCoins();
    }

    public override bool ItemCanBePlaced(Item_Base item)
    {
        if(storage.IsLidOpen() == false)
            return false;

        if (item is not Item_Coin coin)
            return false;

        if (base.ItemCanBePlaced(item) == false)
            return false;

        return coin.hasStamp;
    }


    private void RepositionCoins()
    {
        // clear all slot registrations
        slotItemPair.Clear();

        for (int i = 0; i < currentItems.Count; i++)
        {
            if (i >= slots.Count) break;

            var item = currentItems[i];
            var slot = slots[i].transform;

            item.transform.SetPositionAndRotation(slot.position, slot.rotation);
            RegisterSlot(slot, item);
        }
    }

    public List<Item_Coin> GetCoinList()
    {
        return currentItems.OfType<Item_Coin>().ToList();
    }


    public override Vector3 GetPlacementPosition()
    {
        float offset = storage.IsLidOpen() ? 1f : 1.25f;
        return transform.position + Vector3.up * offset;
    }
}