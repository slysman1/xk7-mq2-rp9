using System.Collections.Generic;
using System.Linq;
using UnityEditor.Tilemaps;
using UnityEngine;

public class FurnaceHolder_MetalBar : ItemHolder
{
    private List<FuranceHolder_MetalBarSlot> metalBarSlots;
    private bool highlightNeedsUpdate;
    [SerializeField] protected Transform placementPoint;

    protected override void Awake()
    {
        base.Awake();
        metalBarSlots = SetupSlots<FuranceHolder_MetalBarSlot>();
    }

    protected override void OnItemAdded(Item_Base item)
    {
        base.OnItemAdded(item);
        item.EnableKinematic(false);

        Audio.PlaySFX("ingridient_added", transform);
        highlightNeedsUpdate = true;
    }

    public bool HighlightNeedsUpdate()
    {
        if (highlightNeedsUpdate)
        {
            highlightNeedsUpdate = false;
            return true;
        }
        return false;
    }


    public Item_MetalBar GetIngridient() => currentItems.FirstOrDefault() as Item_MetalBar;
}