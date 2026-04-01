using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Order_DeliveryManager : MonoBehaviour
{
    private MainNPC mainNpcBTN;
    private DeliveryAreaHolder_AllItems deliveryArea;
    private Quest_DeliveryFeedback[] deliveryTorchFeedback;
    private Delivery_ItemWhirlAnim whirlAnimation;
    private Delivery_ItemDeliveryAnim itemDeliveryAnimation;
    private Quest_DoorAnimation doorAnimation;
    private Quest_WindowAnimation windowAnimation;

    [Header("Delivery duration")]
    [SerializeField] private float correctDeliveryDuration = 2f;
    [SerializeField] private float wrongDeliveryDuration = .8f;
    [SerializeField] private float perItemDeliveryDelay = 0.3f; // Delay between each item delivery


    public Coroutine deliveryCo { get; private set; }


    private void Awake()
    {
        mainNpcBTN = GetComponentInChildren<MainNPC>();
        deliveryArea = GetComponentInChildren<DeliveryAreaHolder_AllItems>();
        whirlAnimation = GetComponentInChildren<Delivery_ItemWhirlAnim>();
        doorAnimation = GetComponentInChildren<Quest_DoorAnimation>();
        windowAnimation = GetComponentInChildren<Quest_WindowAnimation>();
        itemDeliveryAnimation = GetComponentInChildren<Delivery_ItemDeliveryAnim>();


        //deliveryTorchFeedback = GetComponentInChildren<Quest_DeliveryFeedback>();

        deliveryTorchFeedback = FindObjectsByType<Quest_DeliveryFeedback>(
     FindObjectsInactive.Exclude,
     FindObjectsSortMode.None);

        UI_DialogueAnswerSlot.OnDeliveryConfirmed += TryToDeliver;

    }

    private void CompleteQuestDelivery(OrderDataSO orderToComplete)
    {
        OrderManager.instance.NotifyOrderCompleted(orderToComplete);
        DirtManager.instance.TryCreateWeb();

        OrderBoardHolder_Scroll[] orderBoards =
            FindObjectsByType<OrderBoardHolder_Scroll>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var board in orderBoards)
            board.RemoveOrder(orderToComplete);
        
    }

    public void TryToDeliver()
    {
        deliveryArea.RefreshCurrentItems();
        deliveryCo = StartCoroutine(DeliveryCo());
    }

    private IEnumerator DeliveryCo()
    {
        if (windowAnimation.windowOpen)
        {
            yield return windowAnimation.CloseWindowCo();
            yield return new WaitForSeconds(.25f);
        }

        OrderDataSO questToDeliver = FindOrderToDeliver();

        bool hasCorrectDelivery = questToDeliver != null;
        float deliveryDuration = hasCorrectDelivery ? correctDeliveryDuration : wrongDeliveryDuration;
        float closeDoorDelay = hasCorrectDelivery ? perItemDeliveryDelay + .1f : 0;
        float delaySFX = hasCorrectDelivery ? 0 : .05f;
        string deliveryFeedbackSFX = hasCorrectDelivery ? "delivery_finished" : "delivery_rejected";


        PlayTorchFeedback(hasCorrectDelivery, deliveryDuration);
        doorAnimation.OpenDoor(deliveryDuration);
        yield return AnimateItemsDeliveryCo(deliveryDuration, hasCorrectDelivery);
        yield return new WaitForSeconds(closeDoorDelay);

        if (hasCorrectDelivery)
            CompleteQuestDelivery(questToDeliver);


        Audio.QueSFX(deliveryFeedbackSFX, mainNpcBTN.soundSource, delaySFX);
        yield return doorAnimation.CloseDoorCo(hasCorrectDelivery);
        UI.instance.inGameUI.orderListUI.feedback.PlayTextFeedback();
        deliveryCo = null;
    }

    private void PlayTorchFeedback(bool hasCorrectDelivery, float deliveryDuration)
    {
        foreach (var torch in deliveryTorchFeedback)
            torch?.ShowDeliveryFeedback(hasCorrectDelivery, deliveryDuration);
    }

    private IEnumerator AnimateItemsDeliveryCo(float deliveryDuration, bool hasCorrectDelivery)
    {
        List<Item_Base> items = new List<Item_Base>(deliveryArea.currentItems);
        whirlAnimation.WhirlAllItems(items, deliveryDuration);

        yield return new WaitForSeconds(deliveryDuration);

        foreach (var item in items)
        {
            if (hasCorrectDelivery)
            {
                // We need this to stop item one by one. Then we can send it to delivery
                whirlAnimation.StopItemsWhirl(item);
                Audio.PlaySFX("delivery_door_send_item", item.transform);
                itemDeliveryAnimation.DeliverItem(item, deliveryArea);

                yield return new WaitForSeconds(perItemDeliveryDelay);
            }
            else
            {
                yield return null;

                whirlAnimation.StopItemsWhirl(item);
                itemDeliveryAnimation.ResetItem(item);
                //deliveryArea.itemsToReject.Add(item);
            }
        }

        whirlAnimation.WhirlReset();
    }



    private OrderDataSO FindOrderToDeliver()
    {
        foreach (var orderData in OrderManager.instance.trackedOrders)
        {
            if (CanDeliverItems(orderData))
                return orderData;
        }

        return null;
    }
    public bool CanDeliverItems(OrderDataSO orderToCheck)
    {
        if (orderToCheck == null)
            return false;

        // Queue will help us scan all items inside delivery area (including nested containers)
        var queue = new Queue<Item_Base>(deliveryArea.GetCurrentItems());
        var allItems = new List<Item_Base>();

        // Breadth-first search through items
        while (queue.Count > 0)
        {
            // Take next item from the queue
            var item = queue.Dequeue();

            // Add it to the final list
            allItems.Add(item);

            // Check if this item has a holder with more items inside
            var subHolder = item.GetComponentInChildren<ItemHolder>();
            if (subHolder != null)
            {
                // Enqueue all nested items so they get processed too
                foreach (var nested in subHolder.GetCurrentItems())
                    queue.Enqueue(nested);
            }
        }

        return orderToCheck.CanBeCompleted(allItems);
    }

    public List<Item_Base> GetItemsInDeliveryArea() => deliveryArea.currentItems;
}
