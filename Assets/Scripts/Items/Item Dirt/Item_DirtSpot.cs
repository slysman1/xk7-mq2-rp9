using UnityEngine;

public class Item_DirtSpot : Item_Dirt
{

    public override void Interact(Transform carryPoint)
    {
        base.Interact(carryPoint);
        Highlight(false);
        DirtManager.instance.CleanDirt(this);
    }

    public override void ShowInputUI(bool enable)
    {
        base.ShowInputUI(enable);

        Item_Tool toolInHand = player.inventory.GetToolInHand();
        bool canBeCleaned = toolInHand != null && toolInHand.CanInteractWith(itemData);

        if (enable)
        {
            string key = canBeCleaned ? "input_dirt_can_clean" : "input_dirt_cannot_clean";
            UI.instance.inGameUI.inputHelp.AddInput(KeyType.LMB_Hold, key);
        }
        else
            UI.instance.inGameUI.inputHelp.RemoveInput();
    }
}
