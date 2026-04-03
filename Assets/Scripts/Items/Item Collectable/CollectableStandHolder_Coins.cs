using System.Collections.Generic;
using UnityEngine;

public class CollectableStandHolder_Coins : ItemHolder
{
    [SerializeField] private List<CollectableStandHolder_CoinsSlot> slots;

    protected override void Awake()
    {
        base.Awake();
        slots = SetupSlots<CollectableStandHolder_CoinsSlot>();
    }

    protected override void OnItemAdded(Item_Base item)
    {
        base.OnItemAdded(item);
        item.EnableCollider(true);
    }

    public Transform GetCollectableSlot(CollectableCoinType slotType)
    {
        foreach (var slot in slots)
            if (slot.GetCollectableType() == slotType)
                return slot.transform;

        return null;
    }

    public bool HasCoinOfType(CollectableCoinType collectableType)
    {
        foreach (var item in currentItems)
        {
            Item_CollectableCoin collectable = item as Item_CollectableCoin;
            
            if(collectable.GetCollectableType() == collectableType)
                return true;
        }

        return false;
    }
}
