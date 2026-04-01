using UnityEngine;

public class Item_Workstation_CoinCut : Item_Workstation
{
    private CoinCutHolder_Template templateHolder;

    protected override void Awake()
    {
        base.Awake();
        templateHolder = GetComponentInChildren<CoinCutHolder_Template>(true);
    }

    public override void ShowInputUI(bool enable)
    {
        if (enable)
        {
            if (templateHolder.currentItems.Count > 0)
            {
                Item_CoinTemplate template = templateHolder.currentItems[0].GetComponent<Item_CoinTemplate>();


                if (template.heatHandler.isHot && template.NeedsTemper() || template.heatHandler.isHot)
                    inputHelp.AddInput(KeyType.LMB, "input_coincut_cannot_cut_hot_template");

                if (template.heatHandler.isHot == false)
                {
                    if (template.NeedsTemper())
                        inputHelp.AddInput(KeyType.LMB, "input_coincut_cannot_cut_not_tempered_template");
                    else
                        inputHelp.AddInput(KeyType.LMB, "input_coincut_can_cut_coin");

                }

            }
            else
                inputHelp.AddInput(KeyType.LMB, "input_coincut_cannot_cut_coin");


            if (player.inventory.CanPickup(this))
                inputHelp.AddInput(KeyType.LMB_Hold, "input_pickup_hold");


            if (templateHolder.PlayerHasAllowedItem())
                inputHelp.AddInput(KeyType.RMB, "input_coincut_add_template", true);
        }
        else
        {
            UI.instance.inGameUI.inputHelp.RemoveInput();
        }
    }
}
