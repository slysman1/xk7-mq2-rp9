using System.Collections.Generic;
using UnityEngine;


public class CoinCutHolder_Template : ItemHolder, IHighlightable
{
    private List<CoinCutHolder_TemplateSlot> slots;
    public Item_CoinTemplate currentTemplate { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        slots = SetupSlots<CoinCutHolder_TemplateSlot>();
    }

    public override void Highlight(bool enable)
    {
        outline.EnableOutline(enable ? OutlineType.Highlight : OutlineType.None);
        ShowInputUI(enable);
    }

    protected override void ShowInputUI(bool enable)
    {
        base.ShowInputUI(enable);

        if (PlayerHasAllowedItem())
            UI.instance.inputHelp.AddInput(KeyType.RMB, "input_coincut_add_template", true);
    }
    protected override void OnItemAdded(Item_Base item)
    {
        base.OnItemAdded(item);


        Audio.PlaySFX("plate_added", transform);
        currentTemplate = item as Item_CoinTemplate;
    }
}
