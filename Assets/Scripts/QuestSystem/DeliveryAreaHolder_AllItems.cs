using System;
using System.Collections.Generic;
using UnityEngine;


public class DeliveryAreaHolder_AllItems : ItemHolder
{
    public static event Action<List<Item_Base>> OnCoinAmountChanged;

    [SerializeField] private BoxCollider checkArea; // assign a box collider (set to trigger)


    public void RefreshCurrentItems()
    {
        currentItems.Clear();

        Collider[] hits = Physics.OverlapBox(
           checkArea.bounds.center,
           checkArea.bounds.extents,
           checkArea.transform.rotation
       );


        foreach (Collider col in hits)
        {
            // Skip the holder’s own colliders
            if (col.transform.IsChildOf(transform.root))
                continue;

            Item_Base item = col.GetComponentInParent<Item_Base>();

            if (item == null)
                continue;

            if (currentItems.Contains(item) == false)
            {
                currentItems.Add(item);
                OnItemAdded(item);
            }
        }
    }
    

    protected override void OnTriggerEnter(Collider other) 
    {   
        RefreshCurrentItems();
    }

    protected override void OnItemAdded(Item_Base item) 
    {
        item.SetItemHolder(this);
        item.EnableCamPriority(false);
        item.Highlight(false);

        OnCoinAmountChanged?.Invoke(currentItems);
    }

    protected override void OnItemRemoved(Item_Base item)
    {
        base.OnItemRemoved(item);
        OnCoinAmountChanged?.Invoke(currentItems);
    }

    public override bool ItemCanBePlaced(Item_Base item)
    {
        return true;
    }
}
