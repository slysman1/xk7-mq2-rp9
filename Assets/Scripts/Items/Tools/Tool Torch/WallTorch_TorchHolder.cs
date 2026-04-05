using System.Collections.Generic;
using UnityEngine;

public class WallTorch_TorchHolder : ItemHolder
{

    protected override void OnItemAdded(Item_Base item)
    {
        base.OnItemAdded(item);
        Audio.PlaySFX("plate_added", transform);

        Tool_Fire fire = item.GetComponent<Tool_Fire>();
        fire?.EnableFire(true);
    }
}
