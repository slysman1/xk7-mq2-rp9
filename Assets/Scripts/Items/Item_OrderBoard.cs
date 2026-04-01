public class Item_OrderBoard : Item_Base
{

    public override void Highlight(bool enable)
    {
        base.Highlight(enable);
        ShowInputUI(enable);
    }

    public override void ShowInputUI(bool enable)
    {


        if (enable)
        {
            if (inventory.CanPickup(this))
            {
                if (itemData.pickupType == PickupType.Hold)
                    inputHelp.AddInput(KeyType.LMB_Hold, "input_pickup_hold");
                else
                    inputHelp.AddInput(KeyType.LMB, "input_pickup_click");
            }

            Item_Base itemInHand = inventory.GetTopItem();

            if (itemInHand != null && itemInHand.GetComponent<Item_QuestScroll>() != null)
                inputHelp.AddInput(KeyType.RMB, "input_help_add_order_scroll");
        }
        else
            inputHelp.RemoveInput();
    }
}
