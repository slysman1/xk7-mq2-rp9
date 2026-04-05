using UnityEngine;

public class Item_WallTorch : Item_Base
{
    private WallTorch_TorchHolder torchHolder;

    protected override void Awake()
    {
        base.Awake();
        torchHolder = GetComponentInChildren<WallTorch_TorchHolder>();
        torchHolder.OnItemAmountChanged += CacheOutlines;
    }

    public override void Interact(Transform caller)
    {
        if (player.interaction.QuickPressLMB())
        {
            torchHolder.TakeItems(1);
        }
        else if (itemCanBePickedUp)
        {
            base.Interact(caller); // hold — pickup
        }
    }

}
