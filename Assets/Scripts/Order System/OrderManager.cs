using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    public static event Action OnOrderCompleted;

    private ItemManager itemManager => ItemManager.instance;
    public static OrderManager instance;

    [SerializeField] private int ordersPerCall = 1;
    [SerializeField] private int maxConcurrentOrders = 4;
    [SerializeField] private ItemDataSO orderScrollData;
    [SerializeField] private ItemDataSO orderTrackBoardData;

    private int currentSetIndex = 0;

    public List<OrderDataSO> trackedOrders { get; private set; } = new List<OrderDataSO>();
    public List<OrderDataSO> remainingOrders { get; private set; } = new List<OrderDataSO>();
    private List<OrderDataSO> orderPool = new();
    [SerializeField] private List<OrderDataSO> allQuests = new();
    private int completedQuestCount = 0;
    private int ordersCompletedInPhase = 0;

    [Header("Forced Order")]
    [SerializeField] private List<ForcedOrderRule> forcedRules = new();

    private void Awake()
    {
        instance = this;

        OrderBoardHolder_Scroll.OnScrollAdded += AddOrder;
        OrderBoardHolder_Scroll.OnScrollRemoved += RemoveOrder;
        //CollectData();
    }


    [ContextMenu("Collect data")]
    public void CollectData()
    {
#if UNITY_EDITOR
        allQuests = Alexdev.DataUtils.GetData<OrderDataSO>().ToList();
#endif
    }

    public void ResetQuestPool()
    {
        ordersCompletedInPhase = 0;
        orderPool.Clear();
    }

    public void StartTutorialOrder(OrderDataSO tutorialOrder)
    {
        if (tutorialOrder == null)
        {
            Debug.Log("Tutorial order is not assigned!");
            return;
        }

        remainingOrders.Add(tutorialOrder);
        CreateOrder(tutorialOrder);

        TutorialManager.instance.SetTutorialOrder(null);
    }

    public void StartNewOrderSet()
    {
        int maxConcurrentOrders = itemManager.FindAllItems(orderTrackBoardData).Count;
        this.maxConcurrentOrders = maxConcurrentOrders;

        // 🔥 RESET PHASE

        if (remainingOrders.Count >= this.maxConcurrentOrders)
            return;

        for (int i = 0; i < ordersPerCall; i++)
        {
            OrderDataSO order = GetNextOrder();

            if (order == null)
            {
                Debug.LogWarning("⚠️ No orders available in pool.");
                return;
            }

            remainingOrders.Add(order);
            CreateOrder(order);

            Debug.Log($"🧾 Loaded Quest Set {currentSetIndex} with {remainingOrders.Count} quests.");
        }
    }

    private OrderDataSO GetNextOrder()
    {
        // FORCED FIRST
        OrderDataSO forcedOrder = GetForcedOrder();

        if (forcedOrder != null)
            return forcedOrder;
        
        // NORMAL FLOW
        RefillOrdersIfNeeded();

        int randomIndex = UnityEngine.Random.Range(0, orderPool.Count);
        OrderDataSO selectedQuest = orderPool[randomIndex];
        orderPool.RemoveAt(randomIndex);

        return selectedQuest;
    }

    private void RefillOrdersIfNeeded()
    {
        //if(UpgradeManager.)

        if (orderPool.Count == 0)
        {
            UpgradeType currentUpgradeType = UpgradeManager.instance.currentUpgrade.upgradeType;

            List<OrderDataSO> questsForUpgrade = allQuests.FindAll(q => q.neededUpgradeType == currentUpgradeType);

            if (questsForUpgrade == null || questsForUpgrade.Count == 0)
            {
                Debug.LogError($"❌ No quests found for upgrade {currentUpgradeType}");
                return;
            }

            orderPool = new List<OrderDataSO>(questsForUpgrade);
        }
    }

    public void NotifyOrderCompleted(OrderDataSO quest)
    {
        if (remainingOrders.Contains(quest))
            remainingOrders.Remove(quest);

        completedQuestCount++;
        ordersCompletedInPhase++;

        // (your old logic preserved)
        // if (completedQuestCount == 2)
        //     UI.instance.NotifyPlayer("not_can_do_multiple_quests");

        CurrencyManager.instance.AddFavour(quest.favourPointReward);
        OnOrderCompleted?.Invoke();

        Debug.Log($"✅ Completed “{quest.name}” ({remainingOrders.Count} left in set {currentSetIndex}).");
    }

    private OrderDataSO GetForcedOrder()
    {
        UpgradeDataSO upgradeData = UpgradeManager.instance.GetCurrentUpgrade();

        if (upgradeData == null)
            return null;

        for (int i = forcedRules.Count - 1; i >= 0; i--)
        {
            var rule = forcedRules[i];

            if (rule.neededUpgradeType != upgradeData.upgradeType)
                continue;

            if (ordersCompletedInPhase >= rule.forceAfterCompleting)
            {
                if (rule.forcedOrder == null)
                    continue;

                OrderDataSO result = rule.forcedOrder;

                // remove rule so it doesn't trigger again
                forcedRules.RemoveAt(i);

                return result;
            }
        }

        return null;
    }

    private void CreateOrder(OrderDataSO order)
    {
        List<ItemDataSO> questItems = new();
        List<GameObject> extraItems = new();

        extraItems.Add(GetNewScroll(order));

        foreach (var input in order.orderInput)
        {
            for (int i = 0; i < input.itemQuantity; i++)
                questItems.Add(input.itemData);
        }

        DeliveryManager.instance.CreateDeliveryBox(questItems, extraItems);
    }

    public void AddOrder(OrderDataSO questToAdd)
    {
        if (!trackedOrders.Contains(questToAdd))
            trackedOrders.Add(questToAdd);

        UI.instance.inGameUI.UpdateOrderListUI(trackedOrders);
    }

    public void RemoveOrder(OrderDataSO questToRemove)
    {
        if (trackedOrders.Contains(questToRemove))
            trackedOrders.Remove(questToRemove);

        UI.instance.inGameUI.UpdateOrderListUI(trackedOrders);
    }

    public bool HasActiveQuests() => remainingOrders.Count > 0;

    private GameObject GetNewScroll(OrderDataSO order)
    {
        Item_OrderScroll newScroll =
            itemManager.CreateItem(orderScrollData).GetComponent<Item_OrderScroll>();

        newScroll.SetupScroll(order);

        return newScroll.gameObject;
    }
}

[System.Serializable]
public class ForcedOrderRule
{
    public UpgradeType neededUpgradeType;
    public int forceAfterCompleting;
    public OrderDataSO forcedOrder;
}