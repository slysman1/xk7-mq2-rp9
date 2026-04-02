using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Tutorial Step/Talk To Guard - Report Delivery",
    fileName = "Tutorial Step - Talk To Guard - Report Delivery")]
public class TutorialStep_TalkToGuard_ReportDelivery : TutorialStep_TalkToGuard
{
    [SerializeField] private DialogueNodeSO incorrectDeliveryDialogue;
    [SerializeField] private ItemDataSO itemCoinData;

    private Order_DeliveryManager delivery;
    private int neededCoins;
    private System.Action<OrderDataSO> onScrollAdded;
    private System.Action<OrderDataSO> onScrollRemoved;
    private bool boxOpened;
    private Item_DeliveryBox deliveryBox;

    private int stampsRequired;
    private int coinsWithStamp;

    public override void StartTask()
    {
        TutorialIndicator.Clear();

        coinsWithStamp = 0;
        boxOpened = false;
        deliveryBox = ItemManager.instance.FindFirstItemWithComponent<Item_DeliveryBox>();

        if (orderToStartNext != null)
            TutorialManager.instance.SetTutorialOrder(orderToStartNext);
        if (dialogueToStartNext != null)
            DialogueManager.instance.SetPriorityDialogue(dialogueToStartNext);

        delivery = FindFirstObjectByType<Order_DeliveryManager>();
        neededCoins = 0;
        stampsRequired = 0;

        onScrollAdded = _ => CheckConditions();
        onScrollRemoved = _ => CheckConditions();

        Item_DeliveryBox.OnBoxOpened += OnBoxOpened;
        Item_Coin.OnCoinStamped += OnCoinStamped;
        DeliveryAreaHolder_AllItems.OnCoinAmountChanged += OnCoinAmountChanged;
        OrderManager.OnOrderCompleted += HandleTask;
        Order_DeliveryManager.OnDeliveryFailed += OnDeliveryFailed;
        Player.instance.inventory.OnItemAmountUpdate += OnInventoryUpdate;

        OrderBoardHolder_Scroll.OnScrollAdded += onScrollAdded;
        OrderBoardHolder_Scroll.OnScrollRemoved += onScrollRemoved;

        CheckConditions();
    }

    private void OnBoxOpened()
    {
        boxOpened = true;

        if (deliveryBox != null)
        {
            stampsRequired = 0;
            foreach (var item in deliveryBox.GetContainedItems())
            {
                Item_Coin coin = item.GetComponent<Item_Coin>();
                if (coin != null && !coin.hasStamp)
                    stampsRequired++;

                Item_OrderScroll scroll = item.GetComponent<Item_OrderScroll>();
                if (scroll != null)
                    neededCoins = scroll.GetOrderData().orderOutput[0].quantity;
            }
        }

        CheckConditions();
    }


    private void OnCoinAmountChanged(List<Item_Base> items) => CheckConditions();
    private void OnInventoryUpdate() => CheckConditions();

    private bool AllCoinsStamped() => coinsWithStamp >= stampsRequired;


    private void CheckConditions()
    {
        bool hasScroll = OrderManager.instance.trackedOrders.Count > 0;
        bool hasPickedUp = (Player.instance.inventory.TryGetCoinsInHands(out int stamped, out _) && stamped >= neededCoins)
    || delivery.GetItemsInDeliveryArea().Count > 0;
        bool hasCoins = delivery.GetItemsInDeliveryArea().Count >= neededCoins;

        TutorialIndicator.Clear();

        if (!boxOpened)
            TutorialIndicator.HighlightAllTargets<Item_DeliveryBox>();
        else if (AllCoinsStamped() == false)
        {
            TutorialIndicator.HighlightAllTargets<Item_Coin>();
            TutorialIndicator.HighlightAllTargets<Tool_CoinStamp>();
        }
        else if (!hasScroll)
        {
            TutorialIndicator.HighlightTarget<Item_OrderScroll>();
            TutorialIndicator.HighlightTarget<OrderBoardHolder_Scroll>();
        }
        else if (!hasPickedUp && !hasCoins)
            TutorialIndicator.HighlightTargets<Item_Coin>(neededCoins);
        else if (!hasCoins)
            TutorialIndicator.HighlightTarget<DeliveryAreaHolder_AllItems>();
        else
            TutorialIndicator.HighlightTarget<MainNPC>();

        UpdateCurrentGoalUI();
    }

    public override void UpdateCurrentGoalUI()
    {
        if (delivery.deliveryCo != null)
            return;
        
        bool hasScroll = OrderManager.instance.trackedOrders.Count > 0;
        
        int currentCoins = delivery != null ? delivery.GetItemsInDeliveryArea().Count : 0;
        bool hasPickedUp = (Player.instance.inventory.TryGetCoinsInHands(out int stamped, out _) && stamped >= neededCoins)
    || delivery.GetItemsInDeliveryArea().Count > 0;
        bool hasCoins = currentCoins >= neededCoins;

        string text;

        if (!boxOpened)
            text = Localization.GetString("tutorial_step_open_box");
        else if (AllCoinsStamped() == false)
            text = $"{Localization.GetString("tutorial_step_stamp_coins")}: {coinsWithStamp}/{stampsRequired}";
        else if (!hasScroll)
            text = Localization.GetString("tutorial_step_accept_order");
        else if (!hasPickedUp && !hasCoins)
            text = $"{Localization.GetString("tutorial_step_pickup_stamped_coins")}: {stamped}/{neededCoins}";
        else if (!hasCoins)
            text = $"{Localization.GetString("tutorial_step_add_coins_to_delivery_area")}: {currentCoins}/{neededCoins}";
        else
            text = Localization.GetString("tutorial_step_talk_to_guard");

        UI.instance.inGameUI.UpdateCurrentGoal(text);
    }

    public override void StopTask()
    {
        base.StopTask();
        Item_DeliveryBox.OnBoxOpened -= OnBoxOpened;
        Item_Coin.OnCoinStamped -= OnCoinStamped;
        OrderBoardHolder_Scroll.OnScrollAdded -= onScrollAdded;
        OrderBoardHolder_Scroll.OnScrollRemoved -= onScrollRemoved;
        DeliveryAreaHolder_AllItems.OnCoinAmountChanged -= OnCoinAmountChanged;
        OrderManager.OnOrderCompleted -= HandleTask;
        Order_DeliveryManager.OnDeliveryFailed -= OnDeliveryFailed;
        Player.instance.inventory.OnItemAmountUpdate -= OnInventoryUpdate;
    }

    private void OnDeliveryFailed()
    {
        if (incorrectDeliveryDialogue == null) 
            return;

        DialogueManager.instance.SetPriorityDialogue(incorrectDeliveryDialogue);
    }

    private void OnCoinStamped()
    {
        coinsWithStamp++;
        CheckConditions();
    }
}