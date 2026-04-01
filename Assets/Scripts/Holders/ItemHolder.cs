using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class ItemHolder : MonoBehaviour, IHighlightable
{

    protected Player player
    {
        get
        {
            if (_player == null)
                _player = FindFirstObjectByType<Player>();

            return _player;
        }
    }
    private Player _player;
    private Player_Inventory inventory => player.inventory;

    protected Object_Outline outline;
    protected Item_Base itemBase;
    public ItemDataSO atachedItemData { get; private set; }
    protected Dictionary<Transform, Item_Base> slotItemPair = new();
    private List<MonoBehaviour> registeredSlots = new();



    [Header("General info")]
    [SerializeField] protected bool itemsCanBeSnapped = true;
    [Tooltip("Leave at 0 to auto-set from slot count")]
    [SerializeField] protected int maxCapacity = 0;
    [SerializeField] protected List<ItemDataSO> allowedItemData = new();

    //private Coroutine takingItemsCo;



    [Header("Reject Item Setup")]
    [SerializeField] private float rejectUpPower = 2;
    [SerializeField] private float rejectForwardPower = 2;
    public List<Item_Base> currentItems/* { get; private set; } */= new();
    private Collider holderTrigger;
    private Coroutine pauseTriggerCo;

    protected virtual void Awake()
    {
        outline = GetComponentInChildren<Object_Outline>();
        atachedItemData = GetComponent<Item_Base>()?.itemData;
        itemBase = GetComponentInParent<Item_Base>();
    }



    public void TakeItems(int amount = -1)
    {
        if (inventory.takingItemsCo != null || inventory.addingItemsCo != null)
            return;

        int count = amount == -1 ? currentItems.Count : amount;
        inventory.takingItemsCo = StartCoroutine(TakeItemCo(count));
    }

    protected IEnumerator TakeItemCo(int amount)
    {

        int countTaken = 0;

        for (int i = currentItems.Count - 1; i >= 0 && countTaken < amount; i--)
        {
            Item_Base item = currentItems[i];

            if (inventory.CanPickup(item) == false)
                break;

            inventory.PickupItem(item);
            RemoveItem(item);
            countTaken++;
            yield return null;

        }

        inventory.takingItemsCo = null;
    }


    public virtual void AddItem(Item_Base itemToAdd)
    {

        if (ItemCanBePlaced(itemToAdd) == false)
        {
            Debug.Log("Item could not be palced directly");
            return;
        }

        currentItems.Add(itemToAdd);
        OnItemAdded(itemToAdd);
    }

    public virtual void RemoveItem(Item_Base item, bool destroy = false)
    {
        if (currentItems.Contains(item) == false)
            return;


        var slot = GetItemSlot(item);

        if (slot != null) 
            UnregisterSlot(slot);

        if (item.currentItemHolder == this) 
            item.SetItemHolder(null);


        currentItems.Remove(item);
        OnItemRemoved(item);

        if (destroy)
            ItemManager.instance.DestroyItem(item);
    }


    public virtual void RemoveAmount(int amount, Item_Base item, bool destroy = false)
    {
        var toRemove = currentItems
            .Where(i => i == item)
            .TakeLast(amount)
            .ToList();

        foreach (var i in toRemove)
            RemoveItem(i,destroy);
    }
    public virtual void RemoveAmount(int amount,bool destroy = false)
    {
        int removeCount = Mathf.Min(amount, currentItems.Count);
        for (int i = 0; i < removeCount; i++)
            RemoveItem(currentItems[currentItems.Count - 1],destroy);
    }


    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.transform.IsChildOf(transform.root))
            return;

        Item_Base item = other.GetComponentInParent<Item_Base>();

        if (item == null)
            return;

        if (item.currentItemHolder == this)
            return;

        if (ItemCanBePlaced(item) == false)
        {
            RejectItem(item);
            return;
        }

        AddItem(item);
    }

    public virtual bool ItemCanBePlaced(Item_Base item)
    {
        bool valid = true;

        if (item == null)
        {
            Debug.Log("It's not an item - " + gameObject.name);
            valid = false;
        }

        if (IsFull())
        {
            Debug.Log("Capacity is full for - " + gameObject.name);
            valid = false;
        }

        if (IsValidItem(item) == false)
        {
            Debug.Log("Item is not valid - " + item.gameObject.name);
            valid = false;
        }

        if (currentItems.Contains(item))
        {
            Debug.Log("Object already in holder - " + item.gameObject.name);
            valid = false;
        }

        return valid;
    }


    public virtual Transform GetPlacementPoint()
    {
        var slot = GetFreeSlot();

        if (slot != null)
            return slot;

        return null;
    }

    public virtual Vector3 GetPlacementPosition()
    {
        
        return transform.position + (Vector3.up * .5f);
    }


    public int GetItemCount() => currentItems.Count;
    public bool HasItem() => currentItems.Count > 0;
    public bool IsFull() => currentItems.Count >= maxCapacity;
    public virtual List<Item_Base> GetCurrentItems() => currentItems;
    
    protected virtual bool IsValidItem(Item_Base item) => allowedItemData.Contains(item.itemData);

    protected virtual void OnItemAdded(Item_Base item)
    {

        var slot = GetPlacementPoint();

        if (slot == null)
        {
            Debug.Log("Tried to add with no slot.");
            return;
        }

        item.transform.position = slot.position;
        item.transform.rotation = slot.rotation; 

        
        RegisterSlot(slot, item);

        item.transform.parent = transform;
        item.SetItemHolder(this);
        item.EnableKinematic(true);
        item.EnableCamPriority(false);
        item.Highlight(false);
        item.EnableCollider(false);
        item.EnableInHolderLayer(true);
    }

    protected virtual void OnItemRemoved(Item_Base item) 
    { 
        item.EnableInHolderLayer(true);

    }



    public virtual void RejectItem(Item_Base item, float colliderPause = .5f)
    {
        Vector3 forawrd = transform.forward * rejectForwardPower;
        Vector3 up = transform.up * rejectForwardPower;

        item.PauseCollider(colliderPause);
        item.SetVelocity(forawrd + up);
    }

    public void PauseTrigger(float duration = 0.5f)
    {
        if (holderTrigger == null)
        {
            foreach (var col in GetComponentsInChildren<Collider>())
            {
                if (col.isTrigger)
                {
                    holderTrigger = col;
                    break;
                }
            }
        }

        if (pauseTriggerCo != null)
            StopCoroutine(pauseTriggerCo);

        pauseTriggerCo = StartCoroutine(PauseTriggerCo(duration));
    }

    private IEnumerator PauseTriggerCo(float duration)
    {
        holderTrigger.enabled = false;
        yield return new WaitForSeconds(duration);
        holderTrigger.enabled = true;
        pauseTriggerCo = null;
    }


    public virtual void Highlight(bool enable)
    {
        if (outline != null)
            outline.EnableOutline(enable ? OutlineType.Highlight : OutlineType.None);
    }

    protected virtual void ShowInputUI(bool enable)
    {

    }

    public bool PlayerHasAllowedItem()
    {

        foreach (var item in allowedItemData)
            if (player.inventory.HasItem(item))
                return true;

        return false;
    }

    public bool CanSnapPreview()
    {
        return itemsCanBeSnapped;
    }

    #region Slot Setup
    protected List<TSlot> SetupSlots<TSlot>() where TSlot : MonoBehaviour
    {
        var found = GetComponentsInChildren<TSlot>(true).ToList();

        if (maxCapacity == 0)
            maxCapacity = found.Count;

        foreach (var slot in found)
        {
            slot.gameObject.SetActive(false);
            registeredSlots.Add(slot); // store reference here
        }

        return found;
    }

    protected void RegisterSlot(Transform slot, Item_Base item) => slotItemPair[slot] = item;
    protected void UnregisterSlot(Transform slot) => slotItemPair.Remove(slot);
    protected Transform GetItemSlot(Item_Base item)
    {
        foreach (var pair in slotItemPair)
            if (pair.Value == item)
                return pair.Key;
        return null;
    }
    protected Transform GetFreeSlot<TSlot>(List<TSlot> slots) where TSlot : MonoBehaviour
    {
        foreach (var slot in slots)
            if (!slotItemPair.ContainsKey(slot.transform))
                return slot.transform;
        return null;
    }

    public Transform GetFreeSlot()
    {
        foreach (var slot in registeredSlots)
            if (!slotItemPair.ContainsKey(slot.transform))
                return slot.transform;
        return null;
    }

    #endregion

}