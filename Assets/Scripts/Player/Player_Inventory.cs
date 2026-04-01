using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Alexdev.TweenUtils;

public class Player_Inventory : MonoBehaviour
{
    private Player_Interaction interaction;
    public event Action OnItemAmountUpdate;
    [SerializeField] private Transform carryPoint;
    [SerializeField] private List<Item_Base> carriedItems = new();

    public ItemWeightType weightInHands { get; private set; }

    [Header("Pickup & Drop")]
    [SerializeField] private float holdPickupDuration = 0.5f;
    [Space]
    [Range(0, 1)]
    [SerializeField] private float pickUpArc = .2f;
    [Range(0, 1)]
    [SerializeField] private float pickUpDuration = .2f;
    [Space]
    [Range(0, 1)]
    [SerializeField] private float dropArc = .2f;
    [Range(0, 1)]
    [SerializeField] private float dropDuration = .2f;

    public Coroutine addingItemsCo { get; private set; }
    public Coroutine takingItemsCo;// { get; private set; }

    private void Awake()
    {
        interaction = GetComponent<Player_Interaction>();
    }
    public void TryPickup(Item_Base item)
    {
        if (CanPickup(item) == false)
            return;

        StartCoroutine(BeginPickupCo(item));
    }

    private IEnumerator BeginPickupCo(Item_Base item)
    {
        float duration = item.itemData.pickupType == PickupType.Hold ? holdPickupDuration : 0f;

        if (duration > 0)
            UI.instance.pointerUI.BeginToFeelPointer(duration);

        float timePassed = 0f;
        while (timePassed < duration && interaction.holdingLMB)
        {
            timePassed += Time.deltaTime;
            yield return null;
        }


        if (timePassed >= duration)
            PickupItem(item);
    }


    public void AddAllItemsDirectlyToHolder(ItemHolder holder)
    {
        if (addingItemsCo != null)
            return;

        List<Item_Base> listOfItems = new List<Item_Base>(GetCarriedItems());
        carriedItems.Clear();
        OnItemAmountUpdate?.Invoke();

        addingItemsCo = StartCoroutine(AddDirectlyToHolderCo(listOfItems, holder));
    }

    public void AddSingleItemDirectlyToHolder(Item_Base item, ItemHolder holder)
    {
        if (addingItemsCo != null)
            return;

        carriedItems.Remove(item);
        OnItemAmountUpdate?.Invoke();

        List<Item_Base> itemsToAdd = new List<Item_Base>();
        itemsToAdd.Add(item);
        addingItemsCo = StartCoroutine(AddDirectlyToHolderCo(itemsToAdd, holder));
    }

    private IEnumerator AddDirectlyToHolderCo(List<Item_Base> listOfItems, ItemHolder holder)
    {
        for (int i = listOfItems.Count - 1; i >= 0; i--)
        {
            Item_Base itemToAdd = listOfItems[i];
            itemToAdd.EnableCollider(false);

            yield return new WaitForSeconds(.1f);

            if (holder.ItemCanBePlaced(itemToAdd))
            {
                RemoveItem(itemToAdd, holder.GetPlacementPosition(), Quaternion.identity, listOfItems, false);
                holder.AddItem(itemToAdd);
            }
        }

        addingItemsCo = null;
    }



    public void InstantReleaseAllItems(Vector3 position,Transform placementPoint = null)
    {
        if (DoingAction())
            return;

        RemoveItemsAll(position, GetCarriedItems(), true, placementPoint);
    }

    private void RemoveItemsAll(Vector3 position, List<Item_Base> itemsToModify, bool forceDynamic,Transform placementPoint)
    {
        // A: Iterate backwards
        for (int i = itemsToModify.Count - 1; i >= 0; i--)
        {
            Item_Base item = itemsToModify[i];

            Quaternion rotation = placementPoint != null ? placementPoint.rotation : item.transform.rotation;

            Vector3 newPosition = position + new Vector3(0, i * item.GetStackYOffset());
            RemoveItem(item, newPosition, rotation, itemsToModify, forceDynamic);
        }

        itemsToModify.Clear();
    }



    public void PlaceItem(Item_Base item, Vector3 position, Quaternion rotation,float interactionDelay = 0)
    {
        item.OnItemBeingPlaced(position);
        RemoveItem(item, position, rotation, GetCarriedItems(),interactionDelay);
    }

    public void PickupItem(Item_Base item)
    {
        AddItem(item, carriedItems);
    }
    public void AddItem(Item_Base item, List<Item_Base> itemsToModify)
    {
        if (itemsToModify.Contains(item) == false)
            itemsToModify.Add(item);

        StartCoroutine(AddItemCo(item));
        //topItem = item;
    }

    private IEnumerator AddItemCo(Item_Base item)
    {
        Transform carryPoint = GetCarryPoint();

        item.gameObject.SetActive(true);
        item.EnableKinematic(true);
        
        item.EnableCollider(false);

        item.transform.parent = carryPoint;
        item.SetItemHolder(null);


        int itemCountBeforePickup = carriedItems.Count - 1;
        Vector3 targetOffset = new Vector3(0, itemCountBeforePickup * item.GetStackYOffset());

        OnItemAmountUpdate?.Invoke();

        Audio.PlaySFX("item_default_move_anim", item.transform);
        StartCoroutine(SetLocalRotationAs(item.transform, item.GetInHandRotation(), pickUpDuration));
        yield return StartCoroutine(ArcLocal(item.transform, item.GetInHandPosition() + targetOffset, pickUpArc, pickUpDuration));

        weightInHands = item.GetItemWeightType();
        item.OnItemPickup();

        Item_CarryTool carryTool = carryPoint.GetComponentInParent<Item_CarryTool>();
        carryTool?.OnItemGetCarriedByTool(item);
    }

    private void RemoveItem(Item_Base item, Vector3 position, Quaternion rotation, List<Item_Base> itemsToModify = null, bool forceDynamic = false)
    {
        StartCoroutine(RemoveItemCo(item, position, rotation, itemsToModify, forceDynamic));
    }

    private void RemoveItem(Item_Base item, Vector3 position, Quaternion rotation, List<Item_Base> itemsToModify = null, float interactionDelay = 0)
    {
        StartCoroutine(RemoveItemCo(item, position, rotation, itemsToModify,false,interactionDelay));
    }


    private IEnumerator RemoveItemCo(Item_Base item, Vector3 position, Quaternion rotation, List<Item_Base> itemsToModify = null, bool forceDynamic = false,float interactionDelay = 0)
    {
        if (itemsToModify != null && itemsToModify.Contains(item))
            itemsToModify.Remove(item);


        item.transform.SetParent(null);
        item.transform.position = position;
        item.transform.rotation = Quaternion.Euler(rotation.eulerAngles.x, rotation.eulerAngles.y, rotation.eulerAngles.z);
        item.Highlight(false);

        if (item.itemData.placementType == PlacementType.Anywhere || forceDynamic)
            item.EnableKinematic(false);

        yield return new WaitForSeconds(.02f);

        if (item.currentItemHolder == null)
            item.OnItemDrop();
        

        if (carriedItems.Count == 0)
            weightInHands = ItemWeightType.None;

        OnItemAmountUpdate?.Invoke();
    }

    public Transform GetCarryPoint() => carryPoint;

    public List<Item_Base> GetCarriedItems()
    {
        if (GetToolInHand() is Item_CarryTool carryTool && carryTool.GetCarriedItems().Count > 0)
            return carryTool.GetCarriedItems();

        return carriedItems;
    }

    public Item_Base GetTopItem()
    {
        Item_CarryTool carryTool = carriedItems.OfType<Item_CarryTool>().FirstOrDefault();

        //if (carryTool != null)
        //{
        //    if (carryTool.GetCarriedItems().Count > 0)
        //        return carryTool.GetCarriedItems().FirstOrDefault();
        //    else
        //        return carryTool;
        //}

        if (carryTool != null)
            return carryTool.GetCarriedItems().LastOrDefault() ?? carryTool;

        return /*carriedItems.Count > 1 ? */carriedItems.LastOrDefault();// : carriedItems.FirstOrDefault();
    }

    public Item_CarryTool GetCarryTool() => carriedItems.FirstOrDefault(i => i is Item_CarryTool) as Item_CarryTool;
    public Item_Tool GetToolInHand()
    {
        return carriedItems.FirstOrDefault(i => i is Item_Tool) as Item_Tool;
    }
   

    public bool HasItemInHands() => carriedItems.Count > 0;


    public bool HasItem(ItemDataSO itemData)
    {
        foreach(var item in carriedItems)
            if(item.itemData == itemData)
                return true;

        return false;
    }
    public bool CanPickup(Item_Base item)
    {
        if (item == null)
            return false;

        // 1. If the item needs a tool, and we DON'T have the right tool — deny
        //if (toolInHand != null && item.CanBeCarriedWithTool(toolInHand))
        //    return true;

        if (GetToolInHand() != null)
            return false;

        if (item.CanBePickedUp() == false)
            return false;

        // 2. If hands are empty and no tool is needed — allow pickup
        if (carriedItems.Count == 0)
            return true;

        //// 3. If we have a tool, and the tool can carry this item — allow pickup into tool
        //if (toolInHand != null && toolInHand.CanCarry(item))
        //    return true;

        // 4. If stacking is allowed — allow pickup
        if (item.CanStackWith(GetTopItem(), carriedItems.Count))
            return true;

        // 5. Otherwise, deny
        return false;
    }

    public int GetAmountOfItemsInHand() => carriedItems.Count;
    public bool TryGetCoinsInHands(out int stampedCoins, out int unstampedCoins)
    {
        stampedCoins = 0;
        unstampedCoins = 0;

        foreach (var item in GetCarriedItems())
        {
            if (item == null)
                continue;

            Item_Coin coin = item.GetComponent<Item_Coin>();
            if (coin == null)
                continue;

            if (coin.hasStamp)
                stampedCoins++;
            else
                unstampedCoins++;
        }

        return stampedCoins > 0 || unstampedCoins > 0;
    }

    public bool DoingAction() => addingItemsCo != null || takingItemsCo != null;

}
