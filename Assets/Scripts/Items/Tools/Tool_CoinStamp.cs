using System.Collections;
using UnityEngine;
using static Alexdev.TweenUtils;


public class Tool_CoinStamp : Item_Tool
{
    [Header("Stamp settings")]
    [SerializeField] protected float stampTime = .4f;
    [SerializeField] protected Vector3 feedbackVelocity = new Vector3(0, 2.75f);
    [SerializeField] private float feedbackMinTorq = 10f;
    [SerializeField] private float feedbackMaxTorq = 20f;

    public override void PerformInteraction(Item_Base itemToInteractWith)
    {
        base.PerformInteraction(itemToInteractWith);
    }

    protected override IEnumerator PerformInteractionCo(Item_Base item)
    {
        Item_Coin coin = item as Item_Coin;

        // switch to ignore layer so stamp doesn't block coin
        gameObject.layer = LayerMask.NameToLayer("IgnoreAllCollisions");
        transform.parent = null;


        StartCoroutine(SetRotationAs(transform, coin.GetFacingRotation().eulerAngles, stampTime / 2));
        yield return StartCoroutine(ArcMovement(transform, coin.transform, Vector3.zero, arcMovement, stampTime / 2));

        if (MetalConfig.SameMetalType(this, coin))
            coin.EnableStamps();

        coin.TryCreateCollectable();



        Audio.PlaySFX("coin_stamp", transform);
        coin.OnStampFeedback(feedbackVelocity, feedbackMinTorq, feedbackMaxTorq);

        StartCoroutine(ArcMovement(transform, player.inventory.GetCarryPoint(), Vector3.zero, arcMovement, stampTime / 2));
        yield return StartCoroutine(SetRotationAs(transform, GetInHandRotation(), stampTime / 2));
        transform.parent = player.inventory.GetCarryPoint();


        EnableCamPriority(true);
        interactionCo = null;
    }

    public override void OnItemPickup()
    {
        base.OnItemPickup();

        if (objectIndicator == null)
            objectIndicator = GetComponentInChildren<UI_OnObjectIndicator>();

        if (objectIndicator != null)
            objectIndicator.Detach(null);
    }
}
