
using UnityEngine;

public class Item_DirtBrokenPiggyBank : Item_Dirt
{
    private Item_PiggyBankPiece[] brokenPiece;

    protected override void Awake()
    {
        base.Awake();
        brokenPiece = GetComponentsInChildren<Item_PiggyBankPiece>(true);
    }

    public void EnableBrokenPieces()
    {
        foreach(var piece in brokenPiece)
            piece.gameObject.SetActive(true);
    }

    public override void Interact(Transform carryPoint)
    {
        base.Interact(carryPoint);


        Highlight(false);
        ItemManager.instance.DestroyItem(this);
    }

    public override void ShowInputUI(bool enable)
    {
        base.ShowInputUI(enable);

        Item_Tool toolInHand = player.inventory.GetToolInHand();
        bool canBeCleaned = toolInHand != null && toolInHand.CanInteractWith(itemData);

        if (enable)
        {
            string key = canBeCleaned ? "input_help_dirt_can_clean" : "input_help_dirt_cannot_clean";
            UI.instance.inGameUI.inputHelp.AddInput(KeyType.LMB_Hold, key);
        }
        else
            UI.instance.inGameUI.inputHelp.RemoveInput();
    }
}
