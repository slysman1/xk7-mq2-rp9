using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDummyHolder_Bucket : ItemHolder
{
  
    [Header("Dummy Bucket Details")]
    [SerializeField] private Transform extraCollider;
    private List<DummyHolder_BucketSlot> slots;
    
    

    protected override void Awake()
    {
        base.Awake();
        slots = SetupSlots<DummyHolder_BucketSlot>();
    }


    protected override void OnItemAdded(Item_Base item)
    {
        extraCollider.gameObject.SetActive(false);
        base.OnItemAdded(item);
        
        Item_DummyBucket bucket = item as Item_DummyBucket;
        bucket.EnableHappyFace(true);

    }

    protected override void OnItemRemoved(Item_Base item)
    {
        extraCollider.gameObject.SetActive(true);
        base.OnItemRemoved(item);

        Item_DummyBucket bucket = item as Item_DummyBucket;
        bucket.EnableHappyFace(false);
    }


    public Item_DummyBucket GetBucket()
    {
        if (currentItems.Count > 0)
            return currentItems[0] as Item_DummyBucket;

        return null;
    }

    //public void EnableBucketPreviwIfCan(bool enable)
    //{
    //    //if (currentItems.Count > 0 && enable)
    //    //    return;

    //    bucketSlot.gameObject.SetActive(enable);
    //}
}