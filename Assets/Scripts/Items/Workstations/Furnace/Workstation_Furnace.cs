using System.Collections;
using UnityEngine;
using static Alexdev.TweenUtils;


public class Workstation_Furnace : Workstation
{
    public ParticleSystem fireFx;//{ get; private set; }
    private float fireVfxOriginalScale;

    public FurnaceHolder_Logs logHolder { get; private set; }
    public FurnaceHolder_MetalBar metalBarHolder { get; private set; }
    public FurnaceHolder_ProductionResult productionResult { get; private set; }

    private Furnace_DeliveryDoor deliveryDoor;
    private Furnace_AnimationBellows bellowsAnimation;

    [Header("Materials & Production Details")]
    [SerializeField] private float productionDuration = 3f;
    [SerializeField] private int metalNeededPerProduction = 1;
    [SerializeField] private int logsNeededPerProduction = 2;
    public bool isBusy { get; private set; }
    private float remainingProductionTime;

    [Header("Speed Up Production Details")]
    [SerializeField] private float speedUpInteractionTime = 1f;
    [SerializeField] private float quickProductionTime = 3f;
    private Coroutine speedUpProductionCo;

    [Header("Delivery Details")]
    [SerializeField] private float doorOpenDuration = 0.3f;
    [SerializeField] private float trayMoveDuration = 0.3f;
    [SerializeField] private float doorCloseDuration = 0.3f;
    [SerializeField] private float trayReturnDuration = 0.3f;

    [Header("Metal Melt Details")]
    [SerializeField] private Transform hotMetalDummy;
    [SerializeField] private Vector3 hotMetalOriginalPosition;


    protected override void Awake()
    {
        base.Awake();

        logHolder = GetComponentInChildren<FurnaceHolder_Logs>();
        metalBarHolder = GetComponentInChildren<FurnaceHolder_MetalBar>();
        deliveryDoor = GetComponentInChildren<Furnace_DeliveryDoor>();
        productionResult = GetComponentInChildren<FurnaceHolder_ProductionResult>();
        bellowsAnimation = GetComponentInChildren<Furnace_AnimationBellows>(true);

        productionResult.OnTrayEmptied += CompleteProduction;

        fireVfxOriginalScale = fireFx.transform.localScale.x;
        hotMetalOriginalPosition = hotMetalDummy.position;
    }



    public override void ExecuteInteraction(Transform caller = null)
    {
        base.ExecuteInteraction(caller);

        if (CanBeExecuted() == false)
            return;

        ActivateFurnice();        
    }

    public override void ExecuteSecondInteraction(Transform caller = null)
    {
        base.ExecuteSecondInteraction(caller);

        if(CanExecuteSecondInteraction() == false)
            return;

        speedUpProductionCo = StartCoroutine(SpeedUpProductionCo());
    }



    private void ActivateFurnice()
    {
        StartCoroutine(ProduceCo());
        EnableFireVFX(true);
        bellowsAnimation.StartBellows();
    }

    private IEnumerator ProduceCo()
    {
        isBusy = true;
        deliveryDoor.CloseDoor();
        hotMetalDummy.position = hotMetalOriginalPosition;


        Item_MetalBar ingridientToUse = metalBarHolder.GetMetalBar();

        ingridientToUse.EnableHot(true);
        ingridientToUse.EnableKinematic(true);


        Vector3 hotMetalTarget = hotMetalDummy.position + (Vector3.up * .275f);
        StartCoroutine(MovePosition(hotMetalDummy, hotMetalTarget, quickProductionTime));
        StartCoroutine(MovePosition(ingridientToUse.transform.transform, hotMetalOriginalPosition, quickProductionTime, quickProductionTime * .1f));




        remainingProductionTime = productionDuration;

        while (remainingProductionTime > 0f)
        {
            if (speedUpProductionCo != null)
            {
                // clamp remaining time
                remainingProductionTime = Mathf.Min(remainingProductionTime, quickProductionTime);
            }

            remainingProductionTime -= Time.deltaTime;
            yield return null;
        }


        ItemDataSO productionResult = ingridientToUse.GetProductionResult();
        Item_Base newProduct = ItemManager.instance.CreateItem(productionResult).GetComponent<Item_Base>();

        this.productionResult.AddItem(newProduct);

        Audio.PlaySFX("producton_result_ready", this.productionResult.transform);
        DirtManager.instance.TryCreateDirt();




        bellowsAnimation.StopBellows();
        EnableFireVFX(false);

        logHolder.RemoveAmount(logsNeededPerProduction, true);
        metalBarHolder.RemoveAmount(metalNeededPerProduction, true);



        StartCoroutine(MovePosition(hotMetalDummy.transform, hotMetalOriginalPosition, 1.5f));

        deliveryDoor.OpenDoor(doorOpenDuration);

        Audio.QueSFX("tray_move", this.productionResult.transform, doorOpenDuration - .1f);
        yield return new WaitForSeconds(doorOpenDuration);
        this.productionResult.MoveTray(trayMoveDuration);
        remainingProductionTime = 0;
    }

    private void CompleteProduction()
    {
        StartCoroutine(CompleteProductionCo());
    }

    private IEnumerator CompleteProductionCo()
    {
        productionResult.ReturnTray(trayReturnDuration);
        yield return new WaitForSeconds(trayReturnDuration);
        deliveryDoor.CloseDoor(doorCloseDuration);
        isBusy = false;
    }

    private IEnumerator SpeedUpProductionCo()
    {
        StartCoroutine(ScaleLocal(fireFx.transform, Vector3.one * .4f, .1f));
        Audio.PlaySFX("fire_start", transform);
        bellowsAnimation.SpeedUpAnimation(true);

        float elapsed = 0f;

        while (elapsed < quickProductionTime)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // reset speeds
        StartCoroutine(ScaleLocal(fireFx.transform, Vector3.one * fireVfxOriginalScale, .1f));
        bellowsAnimation.SpeedUpAnimation(false);


        speedUpProductionCo = null;
    }

    private void EnableFireVFX(bool enable)
    {
        fireFx.gameObject.SetActive(true);

        if (enable)
        {
            fireFx.Play();
            Audio.PlaySFX("fire_start", transform);
            Audio.LoopSFX("fire_loop", transform);
        }
        else
        {
            fireFx.Stop();
            Audio.StopLoopSFX("fire_loop");
        }
    }
    protected override bool CanBeExecuted()
    {
        if (HasEnoughLogs() == false)
            return false;

        if (HasEnoughMetalBars() == false)
            return false;

        if (isBusy)
            return false;

        return true;
    }

    public override float GetInteractionTime()
    {
        return speedUpInteractionTime;
    }

    public override bool IsBusy() => isBusy;

    public override bool CanExecuteSecondInteraction()
    {
        bool isWorkingAndHasEnoughRemainingTime = remainingProductionTime > quickProductionTime && isBusy;
        return isWorkingAndHasEnoughRemainingTime && speedUpProductionCo == null;
    }

    public bool HasEnoughLogs() => logHolder.GetItemCount() >= logsNeededPerProduction;
    public bool HasEnoughMetalBars() => metalBarHolder.GetItemCount() >= metalNeededPerProduction;
    public int LogsNeeded() => logsNeededPerProduction;

}
