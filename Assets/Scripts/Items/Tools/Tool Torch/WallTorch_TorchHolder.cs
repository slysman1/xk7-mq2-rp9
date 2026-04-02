using System.Collections.Generic;
using UnityEngine;

public class WallTorch_TorchHolder : ItemHolder
{
    private List<Holder_TorchSlot> slots;

    protected override void Awake()
    {
        base.Awake();
        slots = SetupSlots<Holder_TorchSlot>();
    }

    protected override void OnItemAdded(Item_Base item)
    {
        base.OnItemAdded(item);
        item.EnableDefaultLayer();
        Audio.PlaySFX("plate_added", transform);
        Tool_Fire fire = item.GetComponent<Tool_Fire>();

        fire?.EnableFire(true);
        slots[0].gameObject.SetActive(true);
    }

    protected override void OnItemRemoved(Item_Base item)
    {
        base.OnItemRemoved(item);
        slots[0].gameObject.SetActive(false);
    }
}
