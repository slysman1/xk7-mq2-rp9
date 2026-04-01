using System;
using System.Linq;
using UnityEngine;


public class Holder_QuestTrackers : ItemHolder, IHighlightable
{
    private OrderManager orderManager => OrderManager.instance;
    public static event Action OnScrollAttached;


    protected override void OnItemAdded(Item_Base item)
    {
        //int index = currentItems.IndexOf(item);


        //item.SetItemHolder(this);
        //item.EnableKinematic(true);
        //item.transform.position = slot.position;
        //item.transform.rotation = slot.rotation;
        //item.transform.parent = transform;

        //Item_QuestScroll questTracker = item as Item_QuestScroll;

        //questTracker.EnableFoldedScroll(false);
        //Audio.PlaySFX("scroll_attached_to_board", transform);
        //OrderManager.instance.AddOrderToObserver(questTracker.GetQuestData());

        //OnScrollAttached?.Invoke();
    }


    protected override void OnItemRemoved(Item_Base item)
    {
        base.OnItemRemoved(item);

        Item_QuestScroll orderScroll = item as Item_QuestScroll;
        orderManager.RemoveOrderToObserve(orderScroll.GetQuestData());
    }

    public void RemoveOrder(OrderDataSO quest)
    {
        var scroll = currentItems
            .OfType<Item_QuestScroll>()
            .FirstOrDefault(s => s.GetQuestData() == quest);

        if (scroll == null)
            return;

        RemoveItem(scroll, true);
    }

}
