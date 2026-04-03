
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Item_PiggyBank : Item_Base
{
    private Item_DirtBrokenPiggyBank brokenPiggyBank;
    private Holder_PiggyBankContent holderPiggyContent;
    private CollectableStandHolder_Coins collectableHolder;

    [Header("Piggy Details")]
    [Range(0f, 1f)]
    [SerializeField] private float chanceToMultiply;
    [SerializeField] private float minVelocityToBreak;
    [SerializeField] private ItemDataSO brokenPiggyBankData;
    private Vector3 lastVelocity;

    [Header("Collectable Details")]
    [Range(0f, 1f)]
    [SerializeField] private float chanceToGetCollectable;
    [SerializeField] private ItemDataSO collectableInPiggyBank;

    protected override void Awake()
    {
        base.Awake();
        holderPiggyContent = GetComponentInChildren<Holder_PiggyBankContent>(true);
        collectableHolder = FindAnyObjectByType<CollectableStandHolder_Coins>();
    }

    private void InitializeBrokenPieces()
    {
        brokenPiggyBank = ItemManager.instance.CreateItem(brokenPiggyBankData).GetComponent<Item_DirtBrokenPiggyBank>();
        brokenPiggyBank.gameObject.SetActive(false);
        brokenPiggyBank.transform.parent = transform;
        brokenPiggyBank.transform.localPosition = Vector3.zero;
    }
    protected override void Start()
    {
        base.Start();
        InitializeBrokenPieces();
    }

    private void FixedUpdate()
    {
        lastVelocity = rb.linearVelocity;
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        float impactVelocity = lastVelocity.magnitude;

        if (impactVelocity < minVelocityToBreak)
            return;

        BreakPiggyBank();
    }

    [ContextMenu("Piggy Bank")]
    public void BreakPiggyBank()
    {
        brokenPiggyBank.transform.parent = null;
        brokenPiggyBank.EnableBrokenPieces();
        brokenPiggyBank.gameObject.SetActive(true);

        TryMultiplyContent();
        ItemManager.instance.DestroyItem(this);
    }


    private void TryMultiplyContent()
    {
        if (holderPiggyContent.currentItems.Count == 0)
            return;

        GameObject newCollectable = null;

        List<ItemDataSO> itemsToCreate = holderPiggyContent.currentItems.Select(x => x.itemData).ToList();
        List<Transform> createdItems = new List<Transform>();

        foreach (var item in holderPiggyContent.currentItems)
        {
            bool createCollectable = Random.value < chanceToGetCollectable;

            if (createCollectable &&
                Collectable_Manager.instance
                    .CanCreateCollectableOfType(CollectableCoinType.OldBitcoin, out newCollectable))
            {
                // replace coin with collectable
                itemsToCreate.Remove(item.itemData);

                if (newCollectable != null)
                    createdItems.Add(newCollectable.transform);

                continue; // IMPORTANT: skip duplication
            }

            bool canDoubleResult = Random.value < chanceToMultiply;

            if (canDoubleResult)
                itemsToCreate.Add(item.itemData);
        }


        foreach (var item in itemsToCreate)
        {
            Transform newItem = ItemManager.instance.CreateItem(item).transform;
            Item_Coin coinScript = newItem.GetComponent<Item_Coin>();
            coinScript?.EnableStamps(true);
            createdItems.Add(newItem);


        }

        foreach(var item in createdItems)
            item.transform.position = (transform.position + new Vector3(0, .2f)) + Random.insideUnitSphere * .2f;
    }
}
