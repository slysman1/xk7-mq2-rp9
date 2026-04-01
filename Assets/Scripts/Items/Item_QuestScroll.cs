using System;
using UnityEngine;

public class Item_QuestScroll : Item_Base
{
    public static event Action OnScrollUnpacked;

    [Header("Order Scroll Detals")]
    [SerializeField] private OrderDataSO orderData;
    [SerializeField] private GameObject foldedScroll;
    [SerializeField] private GameObject unfoldedScroll;


    public override void Highlight(bool enable)
    {
        base.Highlight(enable);
    }

    public void SetupScroll(OrderDataSO orderData)
    {
        if (orderData == null)
        {
            Debug.Log("Had no data to setup");
            return;
        }

        this.orderData = orderData;
    }

    public OrderDataSO GetQuestData() => orderData;

    public override void OnItemPickup()
    {
        base.OnItemPickup();
        EnableFoldedScroll(true);
    }

    public override void OnItemUnpack()
    {
        base.OnItemUnpack();
        EnableFoldedScroll(true);
        OnScrollUnpacked?.Invoke();
    }

    public void EnableFoldedScroll(bool enable)
    {
        foldedScroll.SetActive(enable);
        unfoldedScroll.SetActive(!enable);
    }

}
