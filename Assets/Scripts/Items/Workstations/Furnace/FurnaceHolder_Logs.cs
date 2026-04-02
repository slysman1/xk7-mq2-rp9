using System;
using System.Collections.Generic;
using System.Linq;

public class FurnaceHolder_Logs : ItemHolder
{
    protected Workstation_Furnace workstationFurnace;
    private List<FurnaceHolder_LogSlot> logSlots;
    public static event Action OnLogAdded;

    protected override void Awake()
    {
        base.Awake();
        logSlots = SetupSlots<FurnaceHolder_LogSlot>();
        workstationFurnace = GetComponentInParent<Workstation_Furnace>();
    }

    public override void Highlight(bool enable)
    {
        base.Highlight(enable);
        ShowSlots(enable);
    }

    protected override void OnItemAdded(Item_Base item)
    {
        base.OnItemAdded(item);

        Audio.PlaySFX("wood_added", transform);
        OnLogAdded?.Invoke();
    }


    public override void ShowSlots(bool showSlots)
    {
        if (!showSlots)
        {
            foreach (var slot in logSlots) // was fuelSlots
                slot.gameObject.SetActive(false);
        }
        else
        {
            for (int i = 0; i < workstationFurnace.LogsNeeded(); i++)
                logSlots[i].gameObject.SetActive(true); // was fuelSlots
        }
    }

}