using UnityEngine;

public class Item_CollectableStand : Item_Base
{
    private Object_Outline[] outlineSet;

    private CollectableStandHolder_Coins collectableHolder;

    protected override void Awake()
    {
        base.Awake();
        collectableHolder = GetComponentInChildren<CollectableStandHolder_Coins>();

    }

    protected override void Start()
    {
        base.Start();
        UpdateOutlineSet();
    }

    public override void Highlight(bool enable)
    {

        foreach (var outline in outlineSet)
            outline.EnableOutline(enable ? OutlineType.Highlight : OutlineType.None);

        Item_Base firstItem = player.inventory.GetTopItem();

        if (firstItem == null)
            return;

        Item_CollectableCoin collectableCoin = firstItem.GetComponent<Item_CollectableCoin>();

        if (collectableCoin != null)
        {
            GameObject collectableSlot = collectableHolder.GetCollectableSlot(collectableCoin.GetCollectableType()).gameObject;
            collectableSlot.SetActive(enable);

        }

        ShowInputUI(enable);

    }

    public override void ShowInputUI(bool enable)
    {
        base.ShowInputUI(enable);

        if (enable)
        {
            if (player.inventory.CanPickup(this))
            {
                if (itemData.pickupType == PickupType.Hold)
                    base.inputHelp.AddInput(KeyType.LMB_Hold, "input_pickup_hold");
                else
                    base.inputHelp.AddInput(KeyType.LMB, "input_pickup_click");
            }

            if (collectableHolder.PlayerHasAllowedItem())
                inputHelp.AddInput(KeyType.RMB, "input_help_add_collectable_coin", true);
        }
        else
            inputHelp.RemoveInput();

    }


    private void UpdateOutlineSet() => outlineSet = GetComponentsInChildren<Object_Outline>(true);
}
