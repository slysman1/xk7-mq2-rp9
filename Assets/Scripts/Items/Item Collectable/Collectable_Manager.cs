using UnityEngine;

public class Collectable_Manager : MonoBehaviour
{
    public static Collectable_Manager instance;
    private ItemManager _itemManager;

    private ItemManager itemManager
    {
        get
        {
            if (_itemManager == null)
                _itemManager = FindFirstObjectByType<ItemManager>();

            return _itemManager;
        }

    }
    private Holder_CollectableCoins collectableCoins;

    [SerializeField] private ItemDataSO[] collectableCoinData;

    private void Awake()
    {
        instance = this;
        collectableCoins = GetComponentInChildren<Holder_CollectableCoins>();
    }

    public bool CanCreateCollectableOfType(CollectableCoinType collectableType, out GameObject collectable)
    {
        collectable = null;

        if (collectableCoins.HasCoinOfType(collectableType))
            return false;

        if(itemManager.HasItem(GetItemDataByCollectableType(collectableType)))
            return false;

        ItemDataSO collectableData = GetItemDataByCollectableType(collectableType);
        collectable = itemManager.CreateItem(collectableData);
        return true;
    }

    public ItemDataSO GetItemDataByCollectableType(CollectableCoinType type)
    {
        foreach (var data in collectableCoinData)
        {
            if (data == null || data.itemPrefab == null)
                continue;

            var comp = data.itemPrefab.GetComponentInChildren<Item_CollectableCoin>(true);

            if (comp != null && comp.GetCollectableType() == type)
                return data;
        }

        return null;
    }
}
