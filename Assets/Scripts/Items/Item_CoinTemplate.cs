using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Item_CoinTemplate : Item_Base
{
    private CoinTemplate_Slot[] coinSlots;
    [Header("Template details")]
    [SerializeField] private float coolingDuration = 1.5f;
    [SerializeField] private ItemDataSO coinPrefab;
    //[SerializeField] private bool isHotByDefault;

    [Header("Temper details")]
    //[SerializeField] private bool hasTemperPointsByDefault;
    [SerializeField] private int minTemperAmount = 0;
    [SerializeField] private int maxTemperAmount = 5;
    [SerializeField] private GameObject temperPointPrefab;
    [SerializeField] private float temperPointOffsetY = .01f;
    [SerializeField] private List<CoinTemplate_TemperPoint> temperPoints = new List<CoinTemplate_TemperPoint>();
    private BoxCollider spawnArea;
    [SerializeField] private ParticleSystem onTemperHitVfx;
    [SerializeField] private float vfxOffsetY = .01f;

    [Header("Recycle Details")]
    //public bool noSlotsByDefault;
    [SerializeField] private int refineValue;



    protected override void Awake()
    {
        base.Awake();
        SetupSlots();
    }

    //protected override void Start()
    //{
    //    base.Start();

    //    if (hasTemperPointsByDefault)
    //        CreateTemperPoints();

    //    if (isHotByDefault)
    //        EnableHot(true);

    //    if (noSlotsByDefault)
    //        MakePlateEmpty(true);
    //}

    public override void ShowInputUI(bool enable)
    {

        if (enable)
        {
            Item_Base itemInHand = inventory.GetTopItem();

            if (inventory.CanPickup(this))
            {
                if (itemData.pickupType == PickupType.Hold)
                    inputHelp.AddInput(KeyType.LMB_Hold, "input_pickup_hold");
                else
                    inputHelp.AddInput(KeyType.LMB, "input_pickup_click");
            }


            if (itemInHand == null && heatHandler.isHot)
                inputHelp.AddInput(KeyType.LMB, "input_help_template_cannot_pick_up_need_tongs");



            if (itemInHand != null)
            {
                if (itemInHand.GetComponent<Tool_Hammer>() != null)
                {
                    Hammer_ItemCombiner combiner = itemInHand.GetComponent<Hammer_ItemCombiner>();

                    if (GetValue() == 10 && EmptyPlate())
                    {

                        if (combiner.CanRefineTemplates(transform))
                            inputHelp.AddInput(KeyType.LMB, "input_help_template_can_refine");
                        else
                            inputHelp.AddInput(KeyType.LMB, "input_help_template_cannot_refine_need_more");
                    }

                    if (GetValue() < 10 && EmptyPlate())
                    {
                        if(combiner.CanCombineTemplates(transform))
                            inputHelp.AddInput(KeyType.LMB, "input_help_template_can_combine");
                        else
                            inputHelp.AddInput(KeyType.LMB, "input_help_template_cannot_combine_need_more");
                    }

                    if (NeedsTemper())
                        inputHelp.AddInput(KeyType.LMB, "input_help_template_can_temper");
                }

                if (itemInHand.GetComponent<Tool_Tongues>() != null)
                {
                    if (itemData.pickupType == PickupType.Hold)
                        inputHelp.AddInput(KeyType.LMB_Hold, "input_pickup_hold");
                    else
                        inputHelp.AddInput(KeyType.LMB, "input_pickup_click");
                }
            }
        }
        else
            inputHelp.RemoveInput();

    }

    public override void Highlight(bool enable)
    {
        base.Highlight(enable);
        ShowInputUI(enable);
    }

    public void MakePlateEmpty(bool makePlateEmpty)
    {
        if (makePlateEmpty == false)
            return;


        foreach (var slot in coinSlots)
            slot.gameObject.SetActive(false);

        foreach (var point in temperPoints)
            if (point != null)
                Destroy(point.gameObject);

        temperPoints.Clear();

        EnableHot(false);
    }


    public void ShowTemperPoints(bool enable)
    {
        foreach (var point in temperPoints)
        {
            if (point.gameObject.activeSelf)
                point.ShowRune(enable);
        }
    }

    public bool NeedsTemper() => temperPoints.Count > 0;
    public void HitTemperPoint(Vector3 hitPoint, float hitPower)
    {
        CoinTemplate_TemperPoint temperPoint = GetHoveredTemperPoint();

        if (temperPoint != null)
        {
            if (temperPoints.Contains(temperPoint))
                temperPoints.Remove(temperPoint);

            if (temperPoint != gameObject)
                temperPoint.gameObject.SetActive(false);
        }

        Vector3 com = rb.worldCenterOfMass;

        // Mirror point = COM + (COM - hitPoint)
        Vector3 mirrorPoint = com + (com - hitPoint);

        // Always apply upward force at the mirrored side
        Vector3 hitForce = Vector3.up * hitPower;

        rb.AddForceAtPosition(hitForce, mirrorPoint, ForceMode.Impulse);

    }

    public CoinTemplate_TemperPoint GetHoveredTemperPoint()
    {
        foreach (var point in temperPoints)
        {
            if (point.gameObject.activeSelf && point.hovered)
                return point;
        }

        return null;
    }

    public override void EnableCamPriority(bool enable)
    {
        base.EnableCamPriority(enable);

        foreach (var point in temperPoints)
            point.EnableCamPriority(enable);
    }

    public void CreateTemperPoints()
    {
        spawnArea = GetCollider() as BoxCollider;

        for (int i = 0; i < Random.Range(minTemperAmount, maxTemperAmount + 1); i++)
        {
            CoinTemplate_TemperPoint newTemperPoint = GetNewTemperPoint()?.GetComponent<CoinTemplate_TemperPoint>();

            if (newTemperPoint != null)
                temperPoints.Add(newTemperPoint);
        }
    }


    private void SetupSlots()
    {
        coinSlots = GetComponentsInChildren<CoinTemplate_Slot>(true);
        refineValue = coinSlots.Length;
        foreach (var placeholder in coinSlots)
        {
            placeholder.SetupPlaceHolder(meshRenderer.material);
            placeholder.gameObject.SetActive(true);
        }
    }

    public void EnableHot(bool enableHot)
    {
        if (enableHot)
        {
            SetCanPickUpTo(false);
            heatHandler.TransitionToHot(.1f);
        }
        else
        {
            SetCanPickUpTo(true);
            heatHandler.TransitionToCool(coolingDuration);
        }
    }


    public bool CanBeCut()
    {
        return coinSlots.Count(p => p.gameObject.activeSelf) > 0 && temperPoints.Count <= 0 && heatHandler.isHot == false;
    }

    public bool EmptyPlate()
    {
        return coinSlots.Count(p => p.gameObject.activeSelf) == 0;
    }

    public int GetValue()
    {
        if (refineValue == 0)
            return coinSlots.Count();

        return refineValue;
    }

    public override bool CanBePickedUp()
    {
        return base.CanBePickedUp() && heatHandler.isHot == false;
    }



    public ItemDataSO GetCoinData() => coinPrefab;
    public Transform GetAvalibleSlot()
    {
        foreach (var p in coinSlots)
        {
            if (p.gameObject.activeSelf)
                return p.transform;

        }

        return null;
    }

    public GameObject GetNewTemperPoint()
    {
        if (spawnArea == null)
            spawnArea = GetComponent<BoxCollider>();

        int attempts = 20; // max tries before giving up
        float safeRadius = 0.1f; // minimum spacing between points

        for (int i = 0; i < attempts; i++)
        {
            // Get collider bounds in local space
            Vector3 localCenter = spawnArea.center;
            Vector3 localSize = spawnArea.size * 0.5f;

            // Clamp offset so it doesn't exceed half-size
            float safeX = Mathf.Max(0, localSize.x - .05f);
            float safeZ = Mathf.Max(0, localSize.z - .05f);

            // Random local point inside safe X/Z range
            float randX = Random.Range(-safeX, safeX);
            float randZ = Random.Range(-safeZ, safeZ);

            // Final local spawn position
            Vector3 localSpawnPos = new Vector3(localCenter.x + randX, localCenter.y, localCenter.z + randZ);

            // Convert to world position
            Vector3 worldSpawnPos = spawnArea.transform.TransformPoint(localSpawnPos);
            worldSpawnPos.y += temperPointOffsetY;

            // Check for overlap with existing temper points
            bool overlaps = false;

            foreach (var tp in temperPoints)
            {
                if (tp == null)
                    continue;

                if (Vector3.Distance(tp.transform.position, worldSpawnPos) < safeRadius)
                {
                    overlaps = true;
                    break;
                }
            }

            if (!overlaps)
            {
                Quaternion randomRot = Quaternion.Euler(0f, Random.Range(0f, 180f), 0f);
                // Valid spawn position → create the point
                return Instantiate(temperPointPrefab, worldSpawnPos, randomRot, transform);
            }
        }

        Debug.LogWarning("Could not find non-overlapping position for temper point.");
        return null;
    }
    //public override void EnableInteraction(bool enable, float delay = 0)
    //{
    //    base.EnableInteraction(enable);

    //    foreach (var point in temperPoints)
    //        point.gameObject.layer = LayerMask.NameToLayer("Hoverable");
    //}

}
