using UnityEngine;

public class WallTorch_TorchHolder : ItemHolder
{
    private Holder_TorchSlot slot;

    protected override void Awake()
    {
        base.Awake();

        slot = GetComponentInChildren<Holder_TorchSlot>();
    }

    protected override void OnItemAdded(Item_Base item)
    {
        Audio.PlaySFX("plate_added", transform);

        item.SetItemHolder(this);
        item.EnableKinematic(true);
        item.transform.parent = transform.parent;
        item.transform.position = slot.transform.position;
        item.transform.rotation = slot.transform.rotation;
    }
}
