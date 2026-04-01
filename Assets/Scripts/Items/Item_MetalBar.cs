using UnityEngine;

public class Item_MetalBar : Item_Base
{
    [SerializeField] private ItemDataSO productionResult;


    public ItemDataSO GetProductionResult() => productionResult;


    public override void ShowInputUI(bool enable)
    {
        if (enable)
        {
            Item_Base itemInHand = inventory.GetTopItem();

            if (inventory.CanPickup(this))
            {
                if (itemData.pickupType == PickupType.Hold)
                    inputHelp.AddInput(KeyType.LMB_Hold, "input_pickup_hold");
                else
                    inputHelp.AddInput(KeyType.LMB, "input_pickup_click");
            }


            if (itemInHand != null && itemInHand.GetComponent<Tool_Hammer>() != null)
            {
                Hammer_ItemCombiner combiner = itemInHand.GetComponent<Hammer_ItemCombiner>();

                if (itemData.creditValue < 10)
                {
                    if (combiner.CanCombineBars(transform))
                        inputHelp.AddInput(KeyType.LMB, "input_help_metal_bar_can_combine");
                    else
                        inputHelp.AddInput(KeyType.LMB, "input_help_metal_bar_cannot_combine_need_more");
                }
            }
        }
        else
            inputHelp.RemoveInput();
    }

    public void EnableHot(bool enableHot)
    {
        if (enableHot)
        {
            heatHandler.TransitionToHot(.1f);
            return;
        }


        heatHandler.TransitionToCool(.1f);
    }

    public override bool CanBePickedUp()
    {
        return base.CanBePickedUp() && heatHandler.isHot == false;
    }
}
