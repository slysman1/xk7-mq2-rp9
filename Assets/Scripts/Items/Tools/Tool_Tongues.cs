using System.Linq;
using UnityEngine;

public class Tool_Tongues : Item_CarryTool
{
    [SerializeField] private ItemDataSO[] immediateCooling;

    public override void PerformInteraction(Item_Base itemToInteractWith)
    {
        base.PerformInteraction(itemToInteractWith);
    }

    public override void OnItemGetCarriedByTool(Item_Base item)
    {
        base.OnItemGetCarriedByTool(item);

        if (item is Item_CoinTemplate template && immediateCooling.Contains(template.itemData))
        {
            template.EnableHot(false);
            template.SetCanPickUpTo(true);
        }
    }
}
