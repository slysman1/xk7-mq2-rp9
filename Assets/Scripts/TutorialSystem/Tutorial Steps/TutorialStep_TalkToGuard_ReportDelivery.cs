using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Tutorial Step/Talk To Guard - Report Delivery", fileName = "Tutorial Step - Talk To Guard - Report Delivery")]

public class TutorialStep_TalkToGuard_ReportDelivery : TutorialStep_TalkToGuard
{
    [SerializeField] private TutorialStep fallBackAcceptOrder;
    [SerializeField] private TutorialStep fallBackPlaceCoinsInArea;
    private Order_DeliveryManager delivery;
    private OrderManager orderManager;
    private bool canDoFallBack;

    public override void StartTask()
    {
        //canDoFallBack = true;
        delivery = FindFirstObjectByType<Order_DeliveryManager>();
        orderManager = OrderManager.instance;

        //Quest_MainNPC.OnDoorKnocked += StopFallBack;
        //Quest_MainNPC.OnQuickDeliveryReport += StopFallBack;

        if (orderToStartNext != null)
            TutorialManager.instance.SetTutorialOrder(orderToStartNext);

        if (dialogueToStartNext != null)
            DialogueManager.instance.SetPriorityDialogue(dialogueToStartNext);


        TutorialIndicator.Clear();
        TutorialIndicator.HighlightTarget<MainNPC>();

        UpdateCurrentGoalUI();
        OrderManager.OnOrderCompleted += HandleTask;
    }


    public override void StopTask()
    {
        base.StopTask();
        //Quest_MainNPC.OnDoorKnocked -= StopFallBack;
        TutorialManager.instance.ResetFallbackTutorial();
    }
    private void StopFallBack()
    {
        if (orderManager.trackedOrders[0].CanBeCompleted(delivery.GetItemsInDeliveryArea()))
            canDoFallBack = false;
    }

    public override void Update()
    {
        base.Update();

        if (delivery == null)
            delivery = FindFirstObjectByType<Order_DeliveryManager>();

        if (orderManager == null)
            orderManager = OrderManager.instance;

        if (delivery.deliveryCo != null)
            return;

        if (canDoFallBack == false)
            return;

        if (orderManager.trackedOrders.Count > 0 &&
            orderManager.trackedOrders[0].CanBeCompleted(delivery.GetItemsInDeliveryArea()))
            return;

        if (OrderManager.instance.trackedOrders.Count == 0)
            TutorialManager.instance.ForceStartFallbackStep(fallBackAcceptOrder);

        if (orderManager.trackedOrders.Count > 0)
        {
            if (delivery.GetItemsInDeliveryArea().Count < orderManager.trackedOrders[0].orderOutput[0].quantity)
                TutorialManager.instance.ForceStartFallbackStep(fallBackPlaceCoinsInArea);
        }
    }
}
