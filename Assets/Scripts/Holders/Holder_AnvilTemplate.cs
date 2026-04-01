using UnityEngine;


public class Holder_AnvilTemplate : ItemHolder, IHighlightable
{
    private Workstation_Anvil workstation;

    protected override void Awake()
    {
        base.Awake();
        itemBase = GetComponentInParent<Item_Base>();
        workstation = GetComponentInParent<Workstation_Anvil>();
    }

    public override void Highlight(bool enable)
    {
        //foreach (var outline in allOutlines)
        outline.EnableOutline(enable ? OutlineType.Highlight : OutlineType.None);

        ItemDataSO itemData = itemBase.itemData;

        //if (enable)
        //{
        //    //UI.instance.inGameUI.inputHelp.AddInput(itemData.interactWithItem);


        //    if (player.inventory.CanPickup(itemBase))
        //        UI.instance.inGameUI.inputHelp.AddInput(itemData.pickUpItem);
        //}
        //else
        //{
        //    UI.instance.inGameUI.inputHelp.RemoveInput(itemData.pickUpItem);
        //    UI.instance.inGameUI.inputHelp.RemoveInput(itemData.interactWithItem);
        //}
    }

    protected override void OnItemAdded(Item_Base item)
    {
        //Audio.PlaySFX("anvil_template_added", item.transform);
        //item.SetItemHolder(this);
        //item.EnableKinematic(false);
        //item.transform.position = placementPoint.position; 
        //item.transform.rotation = placementPoint.rotation;
        //item.transform.parent = null;

        //Item_CoinTemplate template = item as Item_CoinTemplate;
        //template.ShowTemperPoints(true);
        //itemBase.Highlight(false);
    }


    protected override void OnItemRemoved(Item_Base item)
    {
        base.OnItemRemoved(item);

        Item_CoinTemplate template = item as Item_CoinTemplate;
        template.ShowTemperPoints(false);
    }
}
