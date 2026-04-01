using NUnit.Framework.Interfaces;
using System.Collections;
using UnityEngine;
using static Alexdev.TweenUtils;

public class Workstation_WaterBarrel : Workstation
{
    private Item_CoinTemplate templateToCool;


    [Header("Cooling metal Details")]
    [SerializeField] private float coolingDuration = 1f;
    [SerializeField] private int maxUseAmount = 10;

    [Header("Animation Details")]
    [SerializeField] private Vector3 startPositionOffsetY;
    [SerializeField] private Vector3 aboveWaterOffset;
    [SerializeField] private Vector3 belowWaterOffset;
    [SerializeField] private Transform barrelTop;
    [SerializeField] private float preCoolMoveDuration;
    [Space]
    [SerializeField, Range(0f, 1f)] private float arcMovement;
    [SerializeField] private float coolMoveDuration;
    [SerializeField] private float afterCoolMoveDuration;

    [Header("VFX details")]
    [SerializeField] private float shakePower = 10;
    [SerializeField] private int shakeTimes = 2;
    [SerializeField] private float shakeAnimDuration = .1f;
    [Space]
    [SerializeField] private ParticleSystem steamVfx;
    [SerializeField] private ParticleSystem waterSplashVfx;
    [SerializeField] private float steamDuration = .2f;


    [Header("Water details")]
    [SerializeField] private float refillCooldown = 1f;
    [SerializeField] private float waterFillUpDur = 1.5f;
    [SerializeField] private Transform water;
    [SerializeField] private float lowestWaterPoint = .42f;
    [SerializeField] private float highestWaterPoint = 0.786f;
    [SerializeField, Range(.1f, .99f)] private float fillPercent = 0.99f;
    private float lastTimeRefilled;
    private MeshRenderer waterMesh;

    [Header("Audio details")]
    [SerializeField] private float waterDropSoundDelay = .6f;



    private Coroutine interactionCo;
    private Coroutine refillCo;
    private Vector3 defaultWaterPosition;

    protected override void Awake()
    {
        base.Awake();
        defaultWaterPosition = water.localPosition;

        UpdateWaterLevelPosition();
        itemBase.OnItemPickedUp += EmptyBarrel;
        itemBase.OnItemDroppedEvent += DisableWaterMeshIfNeeded;
        waterMesh = water.GetComponent<MeshRenderer>();
        InvokeRepeating(nameof(DisableWaterMeshIfNeeded), .5f, .5f);
        //RefillBarrel();
    }

    private void DisableWaterMeshIfNeeded() => waterMesh.enabled = IsStandingStraight();

    private void EmptyBarrel()
    {
        water.localPosition = new Vector3(defaultWaterPosition.x, water.localPosition.y, defaultWaterPosition.z);
        StartCoroutine(ChangeWaterLevelCo(.1f, .2f));
    }
    


    //public override void Highlight(bool enable)
    //{
    //    base.Highlight(enable);
    //    ShowInputUI(enable);
    //}

    //protected override void ShowInputUI(bool enable)
    //{
    //    base.ShowInputUI(enable);

    //    if (enable)
    //    {
    //        if (inventory.CanPickup(itemBase))
    //        {
    //            if (itemBase.itemData.pickupType == PickupType.Hold)
    //                inputHelp.AddInput(KeyType.LMB_Hold, "input_pickup_hold");
    //            else
    //                inputHelp.AddInput(KeyType.LMB, "input_pickup_click");
    //        }

    //        if (IsStandingStraight())
    //            inputHelp.AddInput(KeyType.F, "input_help_kick_to_refill");
    //        else
    //            inputHelp.AddInput(KeyType.F, "input_help_cannot_refill_barrel_not_straight");

    //        toolInHand = inventory.GetCarryTool();

    //        if (toolInHand == null)
    //            return;

    //        templateToCool = inventory.GetTopItem().GetComponent<Item_CoinTemplate>();

    //        if (templateToCool != null)
    //        {
    //            if (HasEnoughWater() == false)
    //            {
    //                inputHelp.AddInput(KeyType.LMB, "input_help_cannot_cool_template");
    //                return;
    //            }


    //            if (templateToCool.heatHandler.isHot)
    //            {
    //                if (IsStandingStraight())
    //                    inputHelp.AddInput(KeyType.LMB, "input_help_cool_template");
    //                else
    //                    inputHelp.AddInput(KeyType.LMB, "input_help_cannot_cool_template_barrel_not_straight");

    //            }
    //            else
    //            {
    //                if (IsStandingStraight())
    //                    inputHelp.AddInput(KeyType.LMB, "input_help_deep_template");
    //                else
    //                    inputHelp.AddInput(KeyType.LMB, "input_help_cannot_cool_template_barrel_not_straight");
    //            }
    //        }

    //    }
    //    else
    //        inputHelp.RemoveInput();

    //}

    public override void ExecuteInteraction(Transform caller = null)
    {
        base.ExecuteInteraction(caller);


        toolInHand = inventory.GetCarryTool();

        if (toolInHand == null)
            return;

        templateToCool = inventory.GetTopItem().GetComponent<Item_CoinTemplate>();

        if (templateToCool == null)
            return;

        if (interactionCo != null)
            return;

        if (HasEnoughWater() == false)
        {
            Debug.Log("No water!");
            return;
        }

        interactionCo = StartCoroutine(CoolingMetalCo());
    }

    private IEnumerator CoolingMetalCo()
    {
        toolInHand.transform.parent = null;

        toolInHand.EnableCamPriority(false);
        templateToCool.EnableCamPriority(false);

        Vector3 toolsRotation = new Vector3(0, 0, 0);

        Audio.QueSFX("tongs_move_to_start", transform, preCoolMoveDuration / 2);
        StartCoroutine(SetRotationAs(toolInHand.transform, toolsRotation, preCoolMoveDuration));
        yield return StartCoroutine(
            ArcMovement(toolInHand.transform, barrelTop.transform, startPositionOffsetY, arcMovement, preCoolMoveDuration));

        // used for as delay before deeping metal into water
        //yield return new WaitForSeconds(.3f);

        yield return StartCoroutine(MovePosition(toolInHand.transform, water.position + aboveWaterOffset, coolMoveDuration));

        waterSplashVfx.Play();
        Audio.PlaySFX("tongs_move_to_cool", transform);
        yield return StartCoroutine(MovePosition(toolInHand.transform, water.position + belowWaterOffset, coolMoveDuration));



        if (templateToCool.heatHandler.isHot)
        {
            steamVfx.Play();
            Audio.PlaySFX("barrel_water_item_deeped", transform);
            Audio.QueSFX("barrel_water_drop", transform, waterDropSoundDelay);
            Audio.PlaySFX("barrel_water_steam", transform);
            templateToCool.EnableHot(false);
            templateToCool.SetCanPickUpTo(true);
            yield return ReduceWaterLevelCo();
        }



        yield return StartCoroutine(MovePosition(toolInHand.transform, barrelTop.position + startPositionOffsetY, coolMoveDuration));

        toolInHand.transform.parent = inventory.GetCarryPoint();

        Audio.QueSFX("tongs_move_to_hand", transform, afterCoolMoveDuration / 2);
        StartCoroutine(SetLocalRotationAs(toolInHand.transform, toolInHand.GetInHandRotation(), afterCoolMoveDuration));
        yield return StartCoroutine(ArcMovement(toolInHand.transform, inventory.GetCarryPoint(), Vector3.zero, .5f, afterCoolMoveDuration));

        toolInHand.EnableCamPriority(true);
        templateToCool.EnableCamPriority(true);

        interactionCo = null;
    }

    public override void ExecuteSecondInteraction(Transform caller = null)
    {
        base.ExecuteSecondInteraction(caller);

        if (IsStandingStraight() == false)
            return;

        if (Time.time < lastTimeRefilled + refillCooldown)
            return;

        lastTimeRefilled = Time.time;


        Audio.PlaySFX("barrel_punch_impact", transform);
        Audio.PlaySFX("barrel_water_wave", transform);
        Audio.QueSFX("barrel_water_drop", transform, waterDropSoundDelay);
        waterSplashVfx.Play();
        RefillBarrel(caller);
    }

    private IEnumerator ReduceWaterLevelCo()
    {
        //   Audio.PlaySFX("barrel_water_reduce", transform.position);
        float waterPerUse = .99f / maxUseAmount; // percent cost
        float target = Mathf.Clamp01(fillPercent - waterPerUse);

        //StartCoroutine(SteamVfxCo(steamDuration));
        yield return StartCoroutine(ChangeWaterLevelCo(target, coolingDuration));
    }


    public void RefillBarrel(Transform caller = null)
    {
        if (refillCo == null && IsBarrelFull() == false)
        {
            refillCo = StartCoroutine(ChangeWaterLevelCo(highestWaterPoint, waterFillUpDur));
            Audio.PlaySFX("barrel_water_refill", transform, waterFillUpDur - .1f);
            Debug.Log("refilling bater anyway!");
        }

        StartCoroutine(ShakeBarrelVfxCo(caller != null ? caller : transform));
    }


    private IEnumerator ShakeBarrelVfxCo(Transform caller)
    {
        if(IsBarrelFull() == false) 
            water.localPosition = new Vector3(defaultWaterPosition.x,water.localPosition.y,defaultWaterPosition.z);


        Audio.PlaySFX("barrel_shake", transform);
        waterSplashVfx.transform.parent = null;
        // Direction to player
        Vector3 toPlayer = (caller.position - transform.position).normalized;
        toPlayer.y = 0f;

        Vector3 localDir = transform.InverseTransformDirection(toPlayer);
        Vector3 tiltAway = new Vector3(-localDir.z * shakePower, 0, localDir.x * shakePower);

        for (int i = 0; i < shakeTimes; i++)
        {


            float strength = 1f - (i / (float)shakeTimes); // damping
            Vector3 thisTilt = tiltAway * strength;

            yield return StartCoroutine(RotateLocal(transform, thisTilt, shakeAnimDuration));
            yield return StartCoroutine(RotateLocal(transform, -thisTilt, shakeAnimDuration));
        }

        waterSplashVfx.transform.parent = transform;
        waterSplashVfx.transform.localPosition = new Vector3(defaultWaterPosition.x, waterSplashVfx.transform.localPosition.y,defaultWaterPosition.z);
    }


    private IEnumerator ChangeWaterLevelCo(float target, float duration)
    {
        float start = fillPercent;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            fillPercent = Mathf.Lerp(start, target, time / duration);
            UpdateWaterLevelPosition();
            yield return null;
        }

        fillPercent = target;
        UpdateWaterLevelPosition();
        waterSplashVfx.transform.localPosition = water.localPosition;
        refillCo = null;
    }

    private void UpdateWaterLevelPosition()
    {
        float newY = Mathf.Lerp(lowestWaterPoint, highestWaterPoint, fillPercent);
        water.localPosition = new Vector3(defaultWaterPosition.x, newY, defaultWaterPosition.z);
        waterSplashVfx.transform.position = water.position;
    }

    private bool HasEnoughWater()
    {
        float waterPerUse = 0.99f / maxUseAmount;
        return fillPercent >= waterPerUse;
    }

    private bool IsBarrelFull()
    {
        return fillPercent >= highestWaterPoint; // or >= 1f if you prefer exact
    } 
}
