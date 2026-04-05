using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Alexdev.TweenUtils;

public class Tool_Hammer : Item_Tool
{
    private Material material;
    public Hammer_ItemCombiner itemCombiner { get; private set; }

    [Header("Hammer Animation")]
    public float hitPower = 1f;
    public float returnHammerDur = .4f;
    [SerializeField] private Transform hammerHolder;
    [SerializeField] private Vector3 resetPos = new Vector3(0, 0, -2.5f);
    [SerializeField] private Vector3 resetRot = new Vector3(0, -90, -90);
    [SerializeField] private float halfHammerLength = .12f;

    [Header("Runes Animation")]
    [SerializeField] private float hitRuneIntensity = 8f;
    [SerializeField] private float idleIntensity = 1f;
    [SerializeField] private float runesDeactivationDur = 1.5f;


    [Header("Pre-Hit Details")]
    [SerializeField] private float preHitMoveDur = .4f;
    [SerializeField] private Vector3 preHitRot;
    [SerializeField] private float preHitRotDur;
    [SerializeField] private float preHitDistance;
    [SerializeField] private float preHitBackwards;

    [Header("Hit Details")]
    [SerializeField] private float hitSpeed;
    [SerializeField] private Vector3 onHitRot;


    [Header("After-Hit Details")]
    [SerializeField] private Vector3 afterHitRot;
    public float afterHitDelay = .2f;

    [Header("VFX")]
    public ParticleSystem hitVfx;


    protected override void Awake()
    {
        base.Awake();
        material = GetComponentInChildren<MeshRenderer>().material;
        itemCombiner = GetComponent<Hammer_ItemCombiner>();
    }

    private void HammerHit(Item_Base item)
    {
        Audio.PlaySFX("anvil_hammer_hit_impact", item.transform);
        hitVfx.gameObject.SetActive(true);
        hitVfx.Play();
        EnableRunes();

        if (IsPiggyBank(item, out Item_PiggyBank piggyBank))
        {
            piggyBank.BreakPiggyBank();
            return;
        }

        if (IsChisel(item, out Tool_Chisel chisel) && chisel.wall != null)
        {

            chisel.Interact(null);
            return;
        }

        if (IsWall(item, out Holder_CrackedWall wall))
        {

            wall.PushBricks(player.raycaster.interactableHit.transform);
            return;
        }

        if (IsTemplate(item, out Item_CoinTemplate template))
        {

            if (template.heatHandler.isHot || template.NeedsTemper())
            {
                template.HitTemperPoint(player.raycaster.HitPoint, hitPower);
            }
            else
            {

                if (template.GetTotalCoinSlots() == 10 && template.EmptyPlate())
                {
                    itemCombiner.TryConvertToBars(template);
                    return;
                }


                itemCombiner.TryCombinePlates(template.EmptyPlate());
            }

            return;
        }

        // 🔥 NEW: BAR LOGIC (same level as template)
        if (item.GetComponent<Item_MetalBar>() != null)
        {

            itemCombiner.TryCombineBars();
            return;
        }
    }



    protected override IEnumerator PerformInteractionCo(Item_Base item)
    {
        Vector3 hit = player.raycaster.HitPoint;
        Transform hitTransform = player.raycaster.interactableHit.transform;


        float currentPreHitDist = this.preHitDistance;

        Item_CoinTemplate template = item as Item_CoinTemplate;
        Holder_CrackedWall wall = item.GetComponent<Holder_CrackedWall>();

        transform.parent = null;
        hammerHolder.parent = null;
        hammerHolder.position = hit;


        if (wall != null)
        {
            hammerHolder.rotation = hitTransform.rotation * Quaternion.Euler(90, 0, 0);
            currentPreHitDist = currentPreHitDist * 1.5f;
        }
        else
            hammerHolder.rotation = hitTransform.rotation;

        if (template != null)
        {
            // your original logic (Y only)
            hammerHolder.rotation = GetRotationTowardsPlayer(-40);

            // FIX: only if upside down
            if (Vector3.Dot(hammerHolder.up, Vector3.up) < 0f)
            {
                Vector3 euler = hammerHolder.eulerAngles;
                euler.z += 180f; // flip back upright
                hammerHolder.rotation = Quaternion.Euler(euler);
            }
        }

        transform.parent = hammerHolder; // This should be used only after we placed hammer holder


        preHitRot = new Vector3(resetRot.x, resetRot.y, preHitRot.z);
        onHitRot = new Vector3(resetRot.x, resetRot.y, onHitRot.z);
        afterHitRot = new Vector3(resetRot.x, resetRot.y, afterHitRot.z);

        StartCoroutine(SetLocalRotationAs(transform, resetRot, preHitMoveDur));
        StartCoroutine(MoveLocalPosition(transform, resetPos, preHitMoveDur));


        Vector3 hitPoint = hit;
        Vector3 preHitUp = hit + (hammerHolder.up * currentPreHitDist);
        Vector3 onHitPosition = hit + (hammerHolder.up * halfHammerLength);

        // BEFORE HIT SETUP
        Audio.PlaySFX("anvil_hammer_swing", transform);
        StartCoroutine(MovePosition(hammerHolder, preHitUp, preHitRotDur));
        yield return StartCoroutine(SetLocalRotationAs(transform, preHitRot, preHitRotDur));

        // ON HIT SETUP
        StartCoroutine(MovePosition(hammerHolder, onHitPosition, hitSpeed));
        yield return StartCoroutine(SetLocalRotationAs(transform, onHitRot, hitSpeed));

        // HAMMER HIT
        HammerHit(item);

        yield return StartCoroutine(SetLocalRotationAs(transform, afterHitRot, hitSpeed * 2));
        yield return new WaitForSeconds(afterHitDelay);

        // RETURN HAMMER SETUP
        hammerHolder.parent = transform;
        transform.parent = player.inventory.GetCarryPoint().transform;

        StartCoroutine(SetLocalRotationAs(transform, GetInHandRotation(), returnHammerDur));
        yield return StartCoroutine(ArcLocal(transform, GetInHandPosition(), arcMovement, returnHammerDur));

        interactionCo = null;
    }


    public override void OnItemPickup()
    {
        base.OnItemPickup();
        interactionCo = null;
    }


    private void EnableRunes()
    {
        StartCoroutine(AnimateEmission(hitRuneIntensity, 0));
        StartCoroutine(AnimateEmission(idleIntensity, 1.5f));
    }


    private IEnumerator AnimateEmission(float targetIntensity, float duration)
    {
        if (material == null || !material.HasProperty("_EmissionColor"))
            yield break;

        // Get current emission color & split into tint + intensity
        Color currentColor = material.GetColor("_EmissionColor");
        float startIntensity = currentColor.maxColorComponent;
        Color tint = startIntensity > 0f ? currentColor / startIntensity : Color.white;

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);
            float intensity = Mathf.Lerp(startIntensity, targetIntensity, t);

            material.SetColor("_EmissionColor", tint * intensity);
            material.EnableKeyword("_EMISSION");

            yield return null;
        }

        material.SetColor("_EmissionColor", tint * targetIntensity);
        material.EnableKeyword("_EMISSION");
    }

    
    private Quaternion GetRotationTowardsPlayer(float yOffset = 0f)
    {
        Vector3 dir = player.transform.position - hammerHolder.position;
        dir.y = 0f; // ignore height difference

        if (dir.sqrMagnitude > 0.001f)
        {
            // Get target Y angle
            float targetY = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

            // Apply offset
            targetY += yOffset;

            // Keep current X and Z, only replace Y
            Vector3 euler = hammerHolder.eulerAngles;
            euler.y = targetY;

            return Quaternion.Euler(euler);
        }

        return Quaternion.identity; // safer than 'default'
    }

    private bool IsPiggyBank(Item_Base item, out Item_PiggyBank piggyBank)
    {
        piggyBank = item.GetComponent<Item_PiggyBank>();
        return piggyBank != null;
    }
    private bool IsChisel(Item_Base item, out Tool_Chisel piggyBank)
    {
        piggyBank = item.GetComponent<Tool_Chisel>();
        return piggyBank != null;
    }
    private bool IsTemplate(Item_Base item, out Item_CoinTemplate template)
    {
        template = item.GetComponent<Item_CoinTemplate>();
        return template != null;
    }
    private bool IsWall(Item_Base item, out Holder_CrackedWall wall)
    {
        wall = item.GetComponent<Holder_CrackedWall>();
        return wall != null;
    }



}
