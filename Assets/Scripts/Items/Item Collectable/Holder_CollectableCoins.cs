using UnityEngine;

public class Holder_CollectableCoins : ItemHolder
{
    [SerializeField] private CollectableCoin_Slot[] collectableSlot;

    protected override void Awake()
    {
        base.Awake();
        collectableSlot = GetComponentsInChildren<CollectableCoin_Slot>(true);
    }


    protected override void OnItemAdded(Item_Base item)
    {
        Item_CollectableCoin collectableCoin = item as Item_CollectableCoin;
        Transform slot = GetCollectableSlot(collectableCoin.GetCollectableType());

        collectableCoin.transform.parent = transform;
        collectableCoin.transform.position = slot.position;
        collectableCoin.transform.rotation = slot.rotation;

        collectableCoin.SetItemHolder(this);
        collectableCoin.EnableKinematic(true);
    }


    public Transform GetCollectableSlot(CollectableCoinType slotType)
    {
        foreach (var slot in collectableSlot)
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
