using System.Collections;
using UnityEngine;
using static Alexdev.TweenUtils;

public class Tool_WoodChopper : Item_Tool
{
    private bool woodWasChopped;
    private Item_WoodenLogSet woodenLogSet;

    [SerializeField] private Transform axeHitHolder;
    [SerializeField] private float beforeChopUpOffset = .5f;
    [SerializeField] private float beforeChopForwardOffset = -.2f;
    [SerializeField] private Vector3 beforeHitLocalPos = new Vector3(0.325f, 0, 0);
    [SerializeField] private Vector3 beforeHitRot;
    [SerializeField] private float swingDur = .3f;
    [SerializeField] private Vector3 swingOffset;
    [SerializeField] private Vector3 beforeChopLocalRot = new Vector3(0, 0, 115);
    [SerializeField] private Vector3 onChopRot;
    [SerializeField] private float onChopBackwardsOffset = -.15f;
    [SerializeField] private float onChopRotSpeed;
    [SerializeField] private float afterChopDelay = .3f;
    [SerializeField] private ParticleSystem onChopFx;

    private Coroutine axeMoveCo;
    private Coroutine axeRotCo;


    public override void PerformInteraction(Item_Base itemToInteractWith)
    {
        woodenLogSet = itemToInteractWith.GetComponent<Item_WoodenLogSet>();

        if (woodenLogSet.CanBeChopped() == false)
            return;

        base.PerformInteraction(itemToInteractWith);
    }

    protected override IEnumerator PerformInteractionCo(Item_Base item)
    {

        woodWasChopped = false;
        axeHitHolder.parent = null;
        axeHitHolder.rotation = Quaternion.identity;

        Vector3 beforeChopPos =
            woodenLogSet.currentFocus.transform.position +
            Vector3.up * beforeChopUpOffset +
            axeHitHolder.right * beforeChopForwardOffset;

        Vector3 focusPointPos = woodenLogSet.currentFocus.transform.position;

        axeHitHolder.position = beforeChopPos;
        transform.parent = axeHitHolder;
        EnableCamPriority(false);

        yield return InteractionInputCo(interactionDur);

        if (woodWasChopped == false)
        {
            StopCoroutine(axeRotCo);
            StopCoroutine(axeMoveCo);

            axeHitHolder.parent = transform;
            transform.parent = player.inventory.GetCarryPoint();
            StartCoroutine(SetLocalRotationAs(transform, GetInHandRotation(), arcMoveDur));
            StartCoroutine(MoveLocalPosition(transform, GetInHandPosition(), arcMoveDur));
            EnableCamPriority(true);

            interactionCo = null;
            yield break;
        }

        woodenLogSet.Highlight(false);
        Transform target = woodenLogSet.currentFocus.transform;
        Vector3 originalPosition = transform.localPosition;


        StartCoroutine(SetRotationAs(axeHitHolder, beforeHitRot, swingDur));
        Vector3 currentSwing = axeHitHolder.up * swingOffset.y
            + axeHitHolder.right * swingOffset.z;

        Audio.QueSFX("woodcut_axe_swoosh", transform, swingDur * .5f);
        yield return StartCoroutine(MovePosition(axeHitHolder, axeHitHolder.position + currentSwing, swingDur));


        StartCoroutine(MovePosition(axeHitHolder, axeHitHolder.position - currentSwing, swingDur));
        yield return StartCoroutine(SetRotationAs(axeHitHolder, onChopRot, onChopRotSpeed));


        woodenLogSet.Interact(player.transform);
        Audio.PlaySFX("woodcut_axe_chop", transform);
        Instantiate(onChopFx, focusPointPos + Vector3.up * .02f, Quaternion.identity);


        yield return new WaitForSeconds(afterChopDelay);


        // RETURN TO PLAYER
        axeHitHolder.parent = transform;
        transform.parent = player.inventory.GetCarryPoint();
        StartCoroutine(SetLocalRotationAs(transform, GetInHandRotation(), arcMoveDur));
        StartCoroutine(MoveLocalPosition(transform, GetInHandPosition(), arcMoveDur));
        EnableCamPriority(true);

        interactionCo = null;
    }

    private IEnumerator InteractionInputCo(float duration)
    {
        float timer = 0f;

        if (duration > 0)
            UI.instance.pointerUI.BeginToFeelPointer(duration);

        // before chop rot               
        axeRotCo = StartCoroutine(SetLocalRotationAs(transform, beforeChopLocalRot, duration));
        axeMoveCo = StartCoroutine(MoveLocalPosition(transform, beforeHitLocalPos, duration));

        while (timer < duration && player.interaction.holdingLMB && woodenLogSet.currentFocus.hovered)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        UI.instance.pointerUI.CancelFill();
        woodWasChopped = timer >= duration;
    }
}
