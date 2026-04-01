using UnityEngine;

public class Item_WallTorch : Item_Base
{
    private WallTorch_TorchHolder torchHolder;

    public override void Interact(Transform caller)
    {
        torchHolder.TakeItems();
    }

}
