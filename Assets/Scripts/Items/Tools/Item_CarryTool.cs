using static Alexdev.TweenUtils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_CarryTool : Item_Tool
{
    [Header("Carry Rules")]
    [SerializeField] private float pickUpDur;
    [SerializeField] private int maxCarryAmount = 1;
    [SerializeField] private List<Item_Base> carriedItems = new();
    [SerializeField] private Vector3 carriedItemRotation;
    [SerializeField] private Transform carriedItemPoint;

    protected override IEnumerator PerformInteractionCo(Item_Base item)
    {
        if(carriedItems.Contains(item) == false)
            carriedItems.Add(item);

        item.EnableKinematic(true);
        item.EnableCollider(false);
        //item.EnableInteraction(false);
        item.transform.parent = carriedItemPoint;

        StartCoroutine(ArcMovement(item.transform, carriedItemPoint,Vector3.zero, arcMovement, pickUpDur));
        yield return StartCoroutine(SetLocalRotationAs(item.transform,  carriedItemRotation, pickUpDur));

        item.OnItemPickup();
        OnItemGetCarriedByTool(item);
        interactionCo = null;
    }


    public virtual void OnItemGetCarriedByTool(Item_Base item)
    {
        
    }

    public bool CanCarry(Item_Base item)
    {
        if (carriedItems.Count >= maxCarryAmount)
            return false;

        if (allowedItemIds.Contains(item.GetItemId()) == false)
        {
            Debug.Log("Item is not allowed");
            return false;
        }

        return true;
    }

    public void TryRemoveItem(Item_Base item)
    {
        if(carriedItems.Contains(item))
            carriedItems.Remove(item);
    }
    public List<Item_Base> GetCarriedItems() => carriedItems;

    public bool HasCarriedItems() => carriedItems.Count > 0;
    public Transform GetCarryPoint() => carriedItemPoint;
}
