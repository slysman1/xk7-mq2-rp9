using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Alexdev.TweenUtils;


public class Tool_CoinStamp : Item_Tool
{
    [Header("Stamp settings")]
    //    [SerializeField] private GameObject onStampVfx;
    [SerializeField] protected Vector3 coinFeedbackVelocity = new Vector3(0, 2.75f);
    [SerializeField] private float minFeedbackRotation = 10f;
    [SerializeField] private float maxFeedbackRotation = 20f;
    [SerializeField] private ItemDataSO allowedToStamp;

    // Ideal stamp speed ( arc move duration ) is .2f seconds.
    // But we want to introduce upgrades to bring player to that value
    // So by default it will be .25f
    // 1 upgrade (−5%): 0.2375 s// 2 upgrades: 0.225 s// 3 upgrades: 0.2125 s// 4 upgrades: 0.20 s

    protected override IEnumerator PerformInteractionCo(Item_Base item)
    {
        Vector3 velocity = GetWorldVelocity(coinFeedbackVelocity);
        Item_Coin coin = item as Item_Coin;

        transform.parent = null;

        float moveDuration = arcMoveDur * ModifierManager.GetMultiplier(ModifierType.StampingSpeed);

        StartCoroutine(SetRotationAs(transform, coin.GetFacingRotation().eulerAngles, moveDuration));
        yield return StartCoroutine(ArcMovement(transform, coin.transform, Vector3.zero, arcMovement, moveDuration));



        if (coin.itemData == allowedToStamp)
            coin.EnableStamps();

        TutorialManager.instance.silverStampTutorial.ShowSilverStampTutorialIndicatorIfNeeded(itemData,coin.itemData);
        coin.TryCreateCollectable();



        Audio.PlaySFX("coin_stamp", transform);// .PlayTestSound(transform.position);
        float randomFeedbackRotation = Random.Range(minFeedbackRotation, maxFeedbackRotation);
        coin.PlayFeedback(velocity, randomFeedbackRotation);

        StartCoroutine(ArcMovement(transform, player.inventory.GetCarryPoint(), Vector3.zero, arcMovement, moveDuration));
        StartCoroutine(SetRotationAs(transform, GetInHandRotation(), moveDuration));
        transform.parent = player.inventory.GetCarryPoint();
        interactionCo = null;
    }

    public override void OnItemPickup()
    {
        base.OnItemPickup();

        if (TutorialManager.instance.silverStampTutorial.silverStampData != itemData)
            return;

        if (objectIndicator == null)
            objectIndicator = GetComponentInChildren<UI_OnObjectIndicator>();

        if (objectIndicator != null)
            objectIndicator.Detach(null);
    }

    public ItemDataSO GetAllowedToStampCoinData() => allowedToStamp;
}
