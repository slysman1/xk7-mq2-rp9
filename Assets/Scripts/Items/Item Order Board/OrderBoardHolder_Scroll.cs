using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class OrderBoardHolder_Scroll : ItemHolder, IHighlightable
{
    public static event Action<OrderDataSO> OnScrollAdded;
    public static event Action<OrderDataSO> OnScrollRemoved;

    private List<OrderBoardHolder_ScrollSlot> slots;
  
    

    protected override void Awake()
    {
        base.Awake();
        slots = SetupSlots<OrderBoardHolder_ScrollSlot>();
    }

    
    protected override void OnItemAdded(Item_Base item)
    {
        base.OnItemAdded(item);

        Item_OrderScroll orderScroll = item as Item_OrderScroll;
        orderScroll.EnableFoldedScroll(false);

        Audio.PlaySFX("scroll_attached_to_board", transform);
        OnScrollAdded?.Invoke(orderScroll.GetOrderData());
    }

    protected override void OnItemRemoved(Item_Base item)
    {
        base.OnItemRemoved(item);
        Item_OrderScroll orderScroll = item as Item_OrderScroll;
        OnScrollRemoved?.Invoke(orderScroll.GetOrderData());
    }

    public override void TakeItems(int amount = -1)
    {
        if(currentItems.Count > 1)
            currentItems[0].GetComponent<Item_OrderScroll>().EnableFoldedScroll(true);

        base.TakeItems(amount);
    }

    public void RemoveOrder(OrderDataSO quest)
    {
        var scroll = currentItems
            .OfType<Item_OrderScroll>()
            .FirstOrDefault(s => s.GetOrderData() == quest);

        if (scroll == null)
            return;

        RemoveItem(scroll, true);
    }

    public override void Highlight(bool enable)
    {
        base.Highlight(enable);
        ShowSlots(enable);
    }

    public override void ShowSlots(bool showSlots)
    {
        foreach(var slot in slots)
            slot.gameObject.SetActive(showSlots);
    }
}
