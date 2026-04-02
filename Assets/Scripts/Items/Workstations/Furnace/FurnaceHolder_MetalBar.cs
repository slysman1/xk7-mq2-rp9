using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FurnaceHolder_MetalBar : ItemHolder
{
    private List<FurnaceHolder_MetalBarSlot> slots;
    [SerializeField] protected Transform placementPoint;

    protected override void Awake()
    {
        base.Awake();
        slots = SetupSlots<FurnaceHolder_MetalBarSlot>();
    }

    public override void Highlight(bool enable)
    {
        base.Highlight(enable);
        itemBase.Highlight(enable);
    }

    protected override void OnItemAdded(Item_Base item)
    {
        // Set to Kinematic - false - on purpose so it falls with physics applied
        base.OnItemAdded(item);
        item.EnableKinematic(false);

        Audio.PlaySFX("ingridient_added", transform);
    }

    public Item_MetalBar GetMetalBar() => currentItems.FirstOrDefault() as Item_MetalBar;

    public override void ShowSlots(bool showSlots)
    {
        if (!showSlots)
        {
            foreach (var slot in slots) 
                slot.gameObject.SetActive(false);
        }
        else
        {
            for (int i = 0; i < maxCapacity; i++)
                slots[i].gameObject.SetActive(true); 
        }
    }
}