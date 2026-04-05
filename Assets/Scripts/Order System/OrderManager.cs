using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    public static event Action OnOrderCompleted;
    private Animator anim;
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
    public int ordersDelivered { get; private set; } = 0;
    private Dictionary<int, List<ItemDataSO>> pendingInjections = new(); 

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

    public void RequestNextOrder()
    {
        if (remainingOrders.Count >= maxConcurrentOrders)
            return;

        OrderDataSO order = GetNextOrder();

        if (order == null)
        {
            Debug.LogWarning("⚠️ No orders available in pool.");
            return;
        }

        remainingOrders.Add(order);
    }

    private OrderDataSO GetNextOrder()
    {
        RefillOrdersIfNeeded();

        if (orderPool.Count == 0)
            return null;

        OrderDataSO next = orderPool[0];
        orderPool.RemoveAt(0);

        pendingInjections.TryGetValue(ordersDelivered, out List<ItemDataSO> injected);

        if (injected != null) 
            pendingInjections.Remove(ordersDelivered);

        ordersDelivered++;
        CreateOrder(next, injected);  // ← CreateOrder moves here from RequestNextOrder

        return next;
    }

    private void RefillOrdersIfNeeded()
    {
        if (orderPool.Count == 0)
        {
            UpgradeType currentUpgradeType = UpgradeManager.instance.currentUpgrade.upgradeType;
            List<OrderDataSO> questsForUpgrade = allQuests.FindAll(q => q.neededUpgradeType == currentUpgradeType);

            if (questsForUpgrade == null || questsForUpgrade.Count == 0)
            {
                Debug.LogError($"❌ No quests found for upgrade {currentUpgradeType}");
                return;
            }

            orderPool = questsForUpgrade.OrderBy(_ => UnityEngine.Random.value).ToList();
        }
    }


    public void NotifyOrderCompleted(OrderDataSO quest)
    {
        if (remainingOrders.Contains(quest))
            remainingOrders.Remove(quest);


        CurrencyManager.instance.AddFavour(quest.favourPointReward);
        OnOrderCompleted?.Invoke();

        Debug.Log($"✅ Completed “{quest.name}” ({remainingOrders.Count} left in set {currentSetIndex}).");
    }

    private void CreateOrder(OrderDataSO order, List<ItemDataSO> injectedItems = null)
    {
        List<ItemDataSO> questItems = new();
        List<GameObject> extraItems = new();

        extraItems.Add(GetNewScroll(order));

        foreach (var input in order.orderInput)
            for (int i = 0; i < input.itemQuantity; i++)
                questItems.Add(input.itemData);

        if (injectedItems != null)
            questItems.AddRange(injectedItems);

        DeliveryManager.instance.CreateDeliveryBox(questItems, extraItems);
    }

    public int OrdersNeededToReach(int targetCredits)
    {
        int accumulated = 0;
        int count = 0;

        foreach (OrderDataSO order in orderPool)
        {
            if (accumulated >= targetCredits)
                break;

            accumulated += order.totalCreditReward;
            count++;
        }

        return count;
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

    public void RegisterInjection(int poolIndex, List<ItemDataSO> items)
    {
        pendingInjections[poolIndex] = items;
    }


}