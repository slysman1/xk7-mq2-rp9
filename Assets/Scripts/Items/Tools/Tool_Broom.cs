using System;
using System.Collections;
using UnityEngine;
using static Alexdev.TweenUtils;

public class Tool_Broom : Item_Tool
{

    public static event Action OnBroomPickedUp;

    [SerializeField] private float swingSpeed = 10f;
    [SerializeField] private float swingAmount = 15f;
    [SerializeField] private float yOffset = .4f;

    [SerializeField] private ParticleSystem smokeFx;

    [Header("Audio details")]
    [Range(0f, .5f)]
    [SerializeField] private float sweepImpactSoundCooldown = .4f;
    [Range(0f, .5f)]
    [SerializeField] private float sweepSoundCooldown = 0.25f;

    private bool canCleanDirt;
    private float previousZSine;

    public override void OnItemPickup()
    {
        base.OnItemPickup();
        OnBroomPickedUp?.Invoke();
    }

    public override void PerformInteraction(Item_Base itemToInteractWith)
    {
        base.PerformInteraction(itemToInteractWith);

        if (interactionCo != null)
            return;

        interactionCo = StartCoroutine(PerformInteractionCo(itemToInteractWith)); 
    }

    protected override IEnumerator PerformInteractionCo(Item_Base item)
    {
        canCleanDirt = false;
        Item_Dirt dirt = item as Item_Dirt;
        // Move to item
        UI.instance.pointerUI.BeginToFeelPointer(interactionDur + arcMoveDur - .1f );
        yield return StartCoroutine(ArcMovement(transform, item.transform, new Vector3(0, yOffset), arcMovement, arcMoveDur));

        Audio.PlaySFX("broom_dirt_impact", transform);
        Audio.PlaySFX("broom_dirt_sweep", transform);

        transform.rotation = Quaternion.Euler(Vector3.zero);


        smokeFx.transform.position = item.transform.position + Vector3.up * .01f;


        // Sweep rotation in place
        yield return StartCoroutine(SweepRotation(transform, interactionDur));

        if (canCleanDirt)
        {
            Audio.PlaySFX("brom_clean_up_finish", dirt.transform);
            DirtManager.instance.CleanDirt(dirt); 
        }


        // Return to player
        yield return StartCoroutine(ArcMovement(transform, player.inventory.GetCarryPoint(),Vector3.zero , arcMovement, arcMoveDur));
        StartCoroutine(SetLocalRotationAs(transform, GetInHandRotation(), arcMoveDur));

        interactionCo = null;
    }

    private IEnumerator SweepRotation(Transform target, float duration)
    {
        Quaternion startRot = target.localRotation;
        Vector3 currentPosition = transform.position;
        float timer = 0f;
        float sweepSoundTimer = 0f;
        float impactSoundTimer = 0f;


        smokeFx.transform.parent = null;
        smokeFx.gameObject.SetActive(true);
        smokeFx.Play();

        while (timer < duration && player.interaction.holdingLMB)
        {
            transform.position = currentPosition;
            float dt = Time.deltaTime;
            timer += dt;
            sweepSoundTimer += dt;
            impactSoundTimer += dt;

            float xSwing = Mathf.Sin(timer * swingSpeed * 0.8f) * swingAmount * 0.5f;
            float zSwing = Mathf.Sin(timer * swingSpeed) * swingAmount;

            target.localRotation = startRot * Quaternion.Euler(xSwing, 0f, zSwing);

            // SOUND LOGIC
            if (sweepSoundTimer >= sweepSoundCooldown)
            {
                Audio.PlaySFX("broom_dirt_sweep", transform);
                sweepSoundTimer = 0f;
            }

            if (impactSoundTimer >= sweepImpactSoundCooldown)
            {
                Audio.PlaySFX("broom_dirt_impact", transform);
                impactSoundTimer = 0f;

            }

            yield return null;
        }

        smokeFx.Stop();
        smokeFx.gameObject.SetActive(false);
        smokeFx.transform.parent = transform;

        UI.instance.pointerUI.CancelFill();

        canCleanDirt = timer >= duration;
        target.localRotation = startRot;
    }



}
