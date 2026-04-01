using UnityEngine;

public class Holder_PiggyBankContent : ItemHolder
{
    private Collider triggerColldier;

    protected override void Awake()
    {
        base.Awake();
        triggerColldier = GetComponent<Collider>();
    }

    protected override void OnItemAdded(Item_Base item)
    {
        base.OnItemAdded(item);

        item.gameObject.SetActive(false);
        triggerColldier.enabled = false;
    }

    protected override bool IsValidItem(Item_Base item)
    {
        if(item == null) 
            return false;

        Item_Coin coin = item.GetComponent<Item_Coin>();


        if(coin != null && coin.hasStamp == false)
            return false;

        return base.IsValidItem(item);
    }
}
