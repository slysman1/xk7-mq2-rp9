using System;
using System.Collections;
using UnityEngine;
using static Alexdev.TweenUtils;

public class Workstation_CoinCut : Workstation
{
    public static event Action OnCoinCut;

    public CoinCutHolder_Template templateHolder { get; private set; }
    private Item_CoinTemplate currentTemplate;
    private Holder_ItemRejecter rejecter;

    [Header("Coin creation Settings")]
    [SerializeField] private float speedMultiplier = 1f;
    [SerializeField] private Transform coinCreationPoint;
    [SerializeField] private Vector3 coinCreationVelocity;
    [SerializeField] private float minCoinFlipPower = 10;
    [SerializeField] private float maxCoinFlipPower = 20;
    private int coinIndex;


    [Header("Animation settings")]
    [SerializeField] private float templateMoveDuration = .15f;
    [SerializeField] private Transform leverTransform;
    [SerializeField] private Transform pressTransform;
    [SerializeField] private Transform pressPoint;
    [Space]
    [SerializeField] private Vector3 leverRotationAngle;
    [SerializeField] private float moveDistance;
    [Space]
    [SerializeField] private float pressDuration = .1f;
    [SerializeField] private float pressReturnDuration = .25f;
    private Vector3 originalPressPosition;
    private Quaternion buttonDefaultRotation;

    [Header("Remove template settings")]
    [SerializeField] private Transform usedTemplateDirection;
    [SerializeField] private float removeForwardVelocity = 2;
    [SerializeField] private float removeUpVelocity = .5f;

    private Coroutine createCoinCo;


    protected override void Awake()
    {
        base.Awake();
        if (leverTransform != null)
            buttonDefaultRotation = leverTransform.localRotation;


        originalPressPosition = pressTransform.localPosition;
        templateHolder = GetComponentInChildren<CoinCutHolder_Template>();
        rejecter = GetComponentInChildren<Holder_ItemRejecter>();
    }

  
    public override void ExecuteInteraction(Transform caller = null)
    {
        if (CanBeExecuted() == false)
        {
            Debug.Log("Could not be used;");
            return;
        }

        currentTemplate = templateHolder.currentTemplate;

        if (currentTemplate != null && currentTemplate.CanBeCut() == false)
        {
            Debug.Log("No template OR template can't be cut");
            return;
        }

        pressTransform.localPosition = originalPressPosition;

        if (leverTransform != null)
            leverTransform.localRotation = buttonDefaultRotation;

        createCoinCo = StartCoroutine(CreateCoinCo());

    }

    private IEnumerator CreateCoinCo()
    {

        yield return StartCoroutine(rejecter.RejectItemsIfNeededCo());

        Vector3 startPos = pressTransform.localPosition;
        Vector3 downPos = startPos + Vector3.down * moveDistance;

        currentTemplate = templateHolder.currentTemplate;


        Transform nextSlot = currentTemplate?.GetAvalibleSlot();

        if(currentTemplate != null) 
            currentTemplate.SetCanPickUpTo(false);

        if (currentTemplate != null && nextSlot != null)
        {
            AlignCurrentTemplate(nextSlot, currentTemplate.transform);
            yield return new WaitForSeconds(templateMoveDuration * speedMultiplier);
        }

        if (leverTransform != null)
            StartCoroutine(RotateLocal(leverTransform, leverRotationAngle, pressDuration));

        Audio.PlaySFX("coin_cut_move_cutter", transform);
        StartCoroutine(MoveLocalPosition(pressTransform, downPos, pressDuration));

        Audio.QueSFX("coincut_cut_coin_impact", transform, pressDuration);
        yield return new WaitForSeconds(pressDuration);



        if (currentTemplate != null && currentTemplate.CanBeCut())
        {
            Audio.PlaySFX("coincut_create_coin", transform);
            CreateCoin();


            if (currentTemplate.CanBeCut() == false)
            {
                templateHolder.PauseTrigger(.75f);
                templateHolder.RemoveItem(currentTemplate);


                Vector3 velocity = usedTemplateDirection.forward * removeForwardVelocity + usedTemplateDirection.up * removeUpVelocity;
                currentTemplate.EnableKinematic(false);
                currentTemplate.SetVelocity(velocity);
                currentTemplate.SetCanPickUpTo(true);
                currentTemplate.EnableCollider(true);
                currentTemplate.transform.parent = null;
                currentTemplate = null;
                coinIndex = 0;
            }
        }

        if (leverTransform != null)
            StartCoroutine(RotateLocal(leverTransform, -leverRotationAngle, pressReturnDuration));


        yield return MoveLocalPosition(pressTransform, startPos, pressReturnDuration);


        createCoinCo = null;
    }

    private bool IsWorking() => createCoinCo != null;
    protected override bool CanBeExecuted() => templateHolder.GetItemCount() <= 1 && IsWorking() == false;

    private void CreateCoin()
    {
        OnCoinCut?.Invoke();
        Transform currentSlot = currentTemplate.GetAvalibleSlot();
        ItemDataSO coinData = currentTemplate.GetCoinData();


        if (currentSlot == null || currentTemplate.CanBeCut() == false)
            return;

        Item_Coin newCoin =
            ItemManager.instance.CreateItem(coinData).GetComponent<Item_Coin>();

        newCoin.transform.position = coinCreationPoint.position;
        newCoin.GetRb().linearVelocity = GetCoinVelocity();
        newCoin.GetRb().AddTorque(GetCoinTorque(), ForceMode.VelocityChange);
        newCoin.EnableStamps(false);

        currentSlot.gameObject.SetActive(false);


        Transform nextSlot = currentTemplate.GetAvalibleSlot();

        if (nextSlot == null)
            return;
    }

    

    private Vector3 GetCoinVelocity()
    {
        Vector3 worldVelocity =
            coinCreationPoint.forward * coinCreationVelocity.z +
            coinCreationPoint.right * coinCreationVelocity.x +
            coinCreationPoint.up * coinCreationVelocity.y;

        return worldVelocity;
    }
    private Vector3 GetCoinTorque()
    {
        return new Vector3(0, 0, UnityEngine.Random.Range(minCoinFlipPower, maxCoinFlipPower));
    }


    public void AlignCurrentTemplate(Transform nextSlot, Transform template)//,float delay = 0)
    {
        if (template == null)
            return;


        // Full offset
        Vector3 offset = pressPoint.position - nextSlot.position;

        // Ignore Y (keep template's current height)
        offset.y = 0;

        // Apply offset to template world position
        Vector3 targetWorldPos = template.position + offset;

        // Convert back into local space of the parent
        Vector3 targetLocalPos = template.parent.InverseTransformPoint(targetWorldPos);

        // Lock Y to current local Y
        targetLocalPos.y = template.localPosition.y;

        // Tween move in local space
        StartCoroutine(MoveLocalPosition(template, targetLocalPos, templateMoveDuration * speedMultiplier));
    }
}
