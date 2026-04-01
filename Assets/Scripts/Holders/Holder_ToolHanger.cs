using System.Collections.Generic;
using UnityEngine;

public class Holder_ToolHanger : ItemHolder, IHighlightable
{
    private ToolHanger_Slot[] toolSlots;
    private Object_Outline[] outlines;

    private Dictionary<ToolHanger_Slot, bool> slotOccupied = new();

    protected override void Awake()
    {
        base.Awake();
        outlines = GetComponentsInChildren<Object_Outline>();

        foreach (var slot in toolSlots)
        {
            allowedItemData.Add(slot.toolInSlot);
            slotOccupied[slot] = false;
        }
    }


    public override void Highlight(bool enable)
    {
        EnablePlaceholders(enable);


        foreach (var outline in outlines)
            outline.EnableOutline(enable ? OutlineType.Highlight : OutlineType.None);
    }


    protected override void OnItemAdded(Item_Base item)
    {
        Item_Tool tool = item as Item_Tool;
        ToolHanger_Slot slot = GetAvalibleSlot(tool);

        if (slot == null || tool == null)
            return;

        Audio.PlaySFX("anvil_hammer_added", item.transform);
        item.SetItemHolder(this);
        tool.EnableKinematic(true);
        tool.transform.position = slot.transform.position;
        tool.transform.rotation = slot.transform.rotation;
        tool.transform.parent = transform;

        slotOccupied[slot] = true;

        outlines = GetComponentsInChildren<Object_Outline>();

        EnablePlaceholders(false);
    }



    private ToolHanger_Slot GetAvalibleSlot(Item_Tool tool)
    {
        foreach (var slot in toolSlots)
        {
            if (slot.toolInSlot == tool.itemData)
                return slot;
        }

        return null;
    }




    private void EnablePlaceholders(bool enable)
    {
        foreach (var slot in toolSlots)
        {
            // show only if highlight is ON AND slot is not occupied
            bool shouldShow = enable && !slotOccupied[slot];
            slot.gameObject.SetActive(shouldShow);
        }
    }

}
