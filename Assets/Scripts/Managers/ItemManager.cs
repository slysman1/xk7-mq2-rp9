using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager instance;

    // All unique item types currently in the scene
    public ItemDataSO[] currentItems;

    // All actual item instances grouped by type
    private Dictionary<ItemDataSO, List<Item_Base>> sceneItems = new();

    private void Awake()
    {
        if (instance != null && instance != this)
            Destroy(gameObject);
        else
            instance = this;
    }

    private void Start()
    {
        CollectInitialItems();
    }

    public int GetItemAmountInGame(ItemDataSO itemData) => FindAllItems(itemData).Count;


    // --- Registration system ---
    public void RegisterItem(Item_Base item)
    {
        if (item == null || item.itemData == null)
            return;

        if (!sceneItems.ContainsKey(item.itemData))
            sceneItems[item.itemData] = new List<Item_Base>();

        if (!sceneItems[item.itemData].Contains(item))
            sceneItems[item.itemData].Add(item);

        RefreshCurrentItemsArray();
    }

    public void UnregisterItem(Item_Base item)
    {
        if (item == null || item.itemData == null)
            return;

        if (sceneItems.TryGetValue(item.itemData, out var list))
        {
            list.Remove(item);

            if (list.Count == 0)
                sceneItems.Remove(item.itemData);
        }

        RefreshCurrentItemsArray();
    }

    // --- Find functions ---
    public Transform FindItem(ItemDataSO itemToFind)
    {
        if (itemToFind == null)
            return null;

        if (sceneItems.TryGetValue(itemToFind, out var list) && list.Count > 0)
            return list[0].transform;

        return null;
    }

    public List<Transform> FindAllItems(ItemDataSO itemToFind)
    {
        if (itemToFind == null)
            return new List<Transform>();

        if (sceneItems.TryGetValue(itemToFind, out var list))
            return list
                .Where(i => i != null)
                .Select(i => i.transform)
                .ToList();

        return new List<Transform>();
    }

    public List<Transform> FindAllFreeItems(ItemDataSO itemToFind)
    {
        if (itemToFind == null)
            return new List<Transform>();

        if (!sceneItems.TryGetValue(itemToFind, out List<Item_Base> items))
            return new List<Transform>();

        List<Transform> result = new List<Transform>();

        foreach (Item_Base item in items)
        {
            if (item == null)
                continue;

            if (item.currentItemHolder == null)
                result.Add(item.transform);
        }

        return result;
    }

    // --- Create / Destroy ---
    public GameObject CreateItem(ItemDataSO itemData)
    {
        if (itemData == null || itemData.itemPrefab == null)
            return null;

        GameObject newItem = Instantiate(
            itemData.itemPrefab,
            new Vector3(999, 999, 999),
            Quaternion.identity
        );

        // Find all Item_Base components on the new object and its children
        Item_Base[] itemBases = newItem.GetComponentsInChildren<Item_Base>(true);

        foreach (var itemBase in itemBases)
            RegisterItem(itemBase);

        return newItem;
    }


    public void DestroyItem(Item_Base item)
    {
        if (item == null)
            return;

        UI_OnObjectIndicator indicator = item.GetComponentInChildren<UI_OnObjectIndicator>();

        if (indicator != null)
            indicator.transform.parent = null;


        UnregisterItem(item);
        Destroy(item.gameObject);
    }


    public List<T> FindAllItemsWithComponent<T>() where T : Component
    {
        List<T> result = new List<T>();

        foreach (var pair in sceneItems)
        {
            foreach (var item in pair.Value)
            {
                if (item == null)
                    continue;

                T comp = item.GetComponent<T>();
                if (comp != null)
                    result.Add(comp);
            }
        }

        return result;
    }

    public T FindFirstItemWithComponent<T>() where T : Component
    {
        foreach (var pair in sceneItems)
        {
            foreach (var item in pair.Value)
            {
                if (item == null)
                    continue;

                T comp = item.GetComponent<T>();
                if (comp != null)
                    return comp;
            }
        }

        return null;
    }

    // --- Helpers ---
    private void RefreshCurrentItemsArray()
    {
        currentItems = sceneItems.Keys.ToArray();
    }

    private void CollectInitialItems()
    {
        Item_Base[] allSceneItems = FindObjectsByType<Item_Base>(FindObjectsSortMode.None);//,FindObjectsInactive.Include);

        foreach (var item in allSceneItems)
            RegisterItem(item);
    }

    public bool HasItem(ItemDataSO itemData)
    {
        return sceneItems.TryGetValue(itemData, out var list) && list.Count > 0;
    }
}

[System.Serializable]
public class CarryAmountData
{
    public ItemDataSO itemData;
    public int maxCarryAmount = 1;
}
