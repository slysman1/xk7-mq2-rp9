using System;
using System.Collections;
using UnityEngine;

public class Holder_DummyBucket : ItemHolder
{
  
    [Header("Dummy Bucket Details")]
    [SerializeField] private Transform extraCollider;
    private DummyBucket_Slot bucketSlot;
    private Collider triggerCollider;
    

    protected override void Awake()
    {
        base.Awake();
        bucketSlot = GetComponentInChildren<DummyBucket_Slot>(true);
        triggerCollider = GetComponentInChildren<Collider>(true);
    }


    protected override void OnItemAdded(Item_Base item)
    {
        base.OnItemAdded(item);
        
        Item_DummyBucket bucket = item as Item_DummyBucket;
        bucket.EnableHappyFace(true);

        extraCollider.gameObject.SetActive(false);
    }

    protected override void OnItemRemoved(Item_Base item)
    {
        base.OnItemRemoved(item);

        Item_DummyBucket bucket = item as Item_DummyBucket;
        bucket.EnableHappyFace(false);

        extraCollider.gameObject.SetActive(true);
    }


    public Item_DummyBucket GetBucket()
    {
        if (currentItems.Count > 0)
            return currentItems[0] as Item_DummyBucket;

        return null;
    }

    public void PauseTriggerCollider(float pauseTime)
    {
        StartCoroutine(PauseTriggerColliderCo(pauseTime));
    }

    private IEnumerator PauseTriggerColliderCo(float pauseTime)
    {
        triggerCollider.enabled = false;
        yield return new WaitForSeconds(pauseTime);
        triggerCollider.enabled = true;
    }

    //public void EnableBucketPreviwIfCan(bool enable)
    //{
    //    //if (currentItems.Count > 0 && enable)
    //    //    return;

    //    bucketSlot.gameObject.SetActive(enable);
    //}
}