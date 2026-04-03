using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using static Alexdev.TweenUtils;

public class DirtManager : MonoBehaviour
{
    public static DirtManager instance;
    public static event Action OnWebCleaned;
    public static event Action OnSpotCleaned;

    // -------------------- WEB --------------------

    private Dictionary<Item_DirtWebSlot, GameObject> activeWebs = new();

    [Header("Web Setup")]
    [SerializeField] private bool randomizeWebSlot = false;
    [SerializeField] private int webAtStartOfTheGame = 2;
    [SerializeField] private int maxWebAmount;
    [SerializeField] private ItemDataSO webPrefab;
    [SerializeField] private Item_DirtWebSlot[] webSlots;
    [Range(0f, 1f)][SerializeField] private float webCreateChance = 0.2f;

    // -------------------- DIRT --------------------

    private Dictionary<Item_DirtSpotSlot, GameObject> activeDirts = new();

    [Header("Dirt Spot Setup")]
    [SerializeField] private bool randomizeDirtSlot = false;
    [SerializeField] private int dirtAtStartOfTheGame = 2;
    [SerializeField] private int maxDirtAmount;
    [SerializeField] private ItemDataSO dirtPrefab;
    [SerializeField] private Item_DirtSpotSlot[] dirtSlots;
    [Range(0f, 1f)][SerializeField] private float dirtCreateChance = 0.2f;

    private bool cellIsClean = true;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        webSlots = GetComponentsInChildren<Item_DirtWebSlot>();
        dirtSlots = GetComponentsInChildren<Item_DirtSpotSlot>();

        InitializeWebs();
        InitializeDirts();
    }

    // -------------------- INIT --------------------

    private void InitializeWebs()
    {
        foreach (var slot in webSlots)
            slot.HideSlot();

        for (int i = 0; i < webAtStartOfTheGame; i++)
            CreateWeb();

        randomizeWebSlot = true;
    }

    private void InitializeDirts()
    {
        foreach (var slot in dirtSlots)
            slot.HideSlot();

        for (int i = 0; i < dirtAtStartOfTheGame; i++)
            CreateDirt();

        randomizeDirtSlot = true;

    }

    // -------------------- CREATE --------------------

    public void TryCreateWeb()
    {
        if (Random.value < webCreateChance)
            CreateWeb();
    }

    public void TryCreateDirt()
    {
        if (Random.value < dirtCreateChance)
            CreateDirt();
    }

    private void CreateWeb()
    {
        if (activeWebs.Count >= maxWebAmount)
            return;

        var slot = GetFreeWebSlot(randomizeWebSlot);
        if (slot == null)
            return;

        var web = ItemManager.instance
            .CreateItem(webPrefab)
            .GetComponent<Item_DirtWeb>();

        web.transform.localScale = Vector3.one * .01f;
        web.SetupWeb(slot.GetDetails());

        StartCoroutine(ScaleLocal(web.transform, Vector3.one, .15f));

        activeWebs.Add(slot, web.gameObject);
    }

    private void CreateDirt()
    {
        if (activeDirts.Count >= maxDirtAmount)
            return;

        var slot = GetFreeDirtSlot(randomizeDirtSlot);
        if (slot == null)
            return;

        var dirt = ItemManager.instance
            .CreateItem(dirtPrefab)
            .GetComponent<Item_Dirt>();

        dirt.SetupDirt(slot.GetDetails());

        activeDirts.Add(slot, dirt.gameObject);
    }

    // -------------------- SLOT SELECTION --------------------

    private Item_DirtWebSlot GetFreeWebSlot(bool random)
    {
        if (!random)
        {
            foreach (var slot in webSlots)
            {
                if (!activeWebs.ContainsKey(slot))
                    return slot;
            }
        }
        else
        {
            List<Item_DirtWebSlot> free = new();

            foreach (var slot in webSlots)
            {
                if (!activeWebs.ContainsKey(slot))
                    free.Add(slot);
            }

            if (free.Count > 0)
                return free[Random.Range(0, free.Count)];
        }

        return null;
    }

    private Item_DirtSpotSlot GetFreeDirtSlot(bool random)
    {
        if (!random)
        {
            foreach (var slot in dirtSlots)
            {
                if (!activeDirts.ContainsKey(slot))
                    return slot;
            }
        }
        else
        {
            List<Item_DirtSpotSlot> free = new();

            foreach (var slot in dirtSlots)
            {
                if (!activeDirts.ContainsKey(slot))
                    free.Add(slot);
            }

            if (free.Count > 0)
                return free[Random.Range(0, free.Count)];
        }

        return null;
    }

    // -------------------- CLEAN --------------------

    public void CleanWeb(Item_DirtWeb web)
    {
        foreach (var pair in activeWebs)
        {
            if (pair.Value == web.gameObject)
            {
                UI.instance.taskIndicator.RemoveTarget(web.transform);

                activeWebs.Remove(pair.Key);
                ItemManager.instance.DestroyItem(web);

                OnWebCleaned?.Invoke();
                UpdateCleanState();
                return;
            }
        }
    }

    public void CleanDirt(Item_Dirt dirt)
    {
        foreach (var pair in activeDirts)
        {
            if (pair.Value == dirt.gameObject)
            {
                UI.instance.taskIndicator.RemoveTarget(dirt.transform);
                


                activeDirts.Remove(pair.Key);
                ItemManager.instance.DestroyItem(dirt);

                OnSpotCleaned?.Invoke();
                UpdateCleanState();
                return;
            }
        }
    }

    // -------------------- STATE --------------------

    private void UpdateCleanState()
    {
        cellIsClean = activeWebs.Count == 0 && activeDirts.Count == 0;
    }

    public bool CellIsClean() => cellIsClean;
    public int GetWebsCount() => activeWebs.Count;
    public int GetSpotCount() => activeDirts.Count;
}