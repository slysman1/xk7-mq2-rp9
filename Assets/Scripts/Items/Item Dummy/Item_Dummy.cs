using System;
using System.Collections;
using UnityEngine;
using static Alexdev.TweenUtils;

public class Item_Dummy : Item_Base
{
    
    private Holder_DummyBucket bucketHolder;

    [Header("VFX details")]
    [SerializeField] private float shakePower = 10;
    [SerializeField] private int shakeTimes = 2;
    [SerializeField] private float shakeAnimDuration = .1f;

    [Header("Bucket Details")]
    [SerializeField] private float backwardsVelocity = 2f;
    [SerializeField] private float removeVelocityY = 3f;
    private Coroutine interactionCo;

    protected override void Awake()
    {
        base.Awake();
        bucketHolder = GetComponentInChildren<Holder_DummyBucket>(true);
        bucketHolder.OnItemAmountChanged += CacheOutlines;
    }

    public override void ShowInputUI(bool enable)
    {
       
        if (enable)
        {
            if (inventory.CanPickup(this))
            {
                if (itemData.pickupType == PickupType.Hold)
                    inputHelp.AddInput(KeyType.LMB_Hold, "input_pickup_hold");
                else
                    inputHelp.AddInput(KeyType.LMB, "input_pickup_click");
            }

            if (bucketHolder.PlayerHasAllowedItem())
                inputHelp.AddInput(KeyType.RMB, "input_help_can_add_bucket",true);


            inputHelp.AddInput(KeyType.F, "input_help_kick_dummy");
        }
        else
            inputHelp.RemoveInput();
    }

    public override void SeconderyInteraction(Transform caller = null)
    {
        base.SeconderyInteraction(caller);

        if (interactionCo != null)
            StopCoroutine(interactionCo);

        interactionCo = StartCoroutine(ShakeDummyVfxCo(caller));

    }
    private IEnumerator ShakeDummyVfxCo(Transform caller)
    {
        Audio.PlaySFX("barrel_shake", transform);

        Vector3 toPlayer = (caller.position - transform.position).normalized;
        toPlayer.y = 0f;

        Vector3 localDir = transform.InverseTransformDirection(toPlayer);

        // punch tilt (away from player)
        Vector3 hitTilt = new Vector3(-localDir.z * shakePower, 0, localDir.x * shakePower);

        // fast recoil
        yield return StartCoroutine(RotateLocal(transform, hitTilt, shakeAnimDuration * 0.35f));

        Item_DummyBucket bucket = bucketHolder.GetBucket();

        if (bucket != null)
        {
            //bucketHolder.PauseTriggerCollider(1f);
            Vector3 bucketVelocity = -localDir * backwardsVelocity + new Vector3(0, removeVelocityY, 0);
            bucket.SendBucketFlying(bucketVelocity);
        }

        // small overshoot back
        yield return StartCoroutine(RotateLocal(transform, -hitTilt * 0.25f, shakeAnimDuration * 0.35f));

        // settle to neutral
        yield return StartCoroutine(RotateLocal(transform, Vector3.zero, shakeAnimDuration * 0.3f));
    }
}
