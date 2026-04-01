using UnityEngine;

public class Item_Workstation_Furnace : Item_Workstation
{
    protected Workstation_Furnace furnaceWorkstation;

    protected override void Awake()
    {
        base.Awake();
        furnaceWorkstation = workstation as Workstation_Furnace;
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

            if (itemInHand != null)
            {
                if (itemInHand.GetComponent<Item_WoodenLog>() != null)
                    inputHelp.AddInput(KeyType.RMB, "input_help_add_wooden_log");

                if (itemInHand.GetComponent<Item_MetalBar>() != null)
                    inputHelp.AddInput(KeyType.RMB, "input_help_add_metal_bar");
            }


            bool hasEnoughLogs = furnaceWorkstation.HasEnoughLogs();
            bool hasEnoughMetalBars = furnaceWorkstation.HasEnoughMetalBars();

            if (hasEnoughLogs == false)
                furnaceWorkstation.logHolder.ShowSlots(true);


            if (hasEnoughLogs && hasEnoughMetalBars == false)
                inputHelp.AddInput(KeyType.LMB, "input_help_cannot_start_furnace_need_ingridient");

            if (hasEnoughLogs == false && hasEnoughMetalBars)
                inputHelp.AddInput(KeyType.LMB, "input_help_cannot_start_furnace_need_fuel");

            if (hasEnoughLogs == false && hasEnoughMetalBars == false)
                inputHelp.AddInput(KeyType.LMB, "input_help_cannot_start_furnace_need_ingridient_and_fuel");

            if (hasEnoughLogs && hasEnoughMetalBars && furnaceWorkstation.isBusy == false)
                inputHelp.AddInput(KeyType.LMB, "input_help_can_start_furnace");


            if (workstation.CanExecuteSecondInteraction())
                inputHelp.AddInput(KeyType.F, "input_help_speed_up_furnace");

        }
        else
        {
            inputHelp.RemoveInput();
            furnaceWorkstation.logHolder.ShowSlots(false);
        }
    }
}
