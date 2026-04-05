using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeliveryManager : MonoBehaviour
{
    public static DeliveryManager instance;
    private ItemManager itemManager => ItemManager.instance;

    [SerializeField] private ItemDataSO deliveryBoxPrefab;
    [SerializeField] private Transform deliveryPoint;

    [SerializeField] private ItemDataSO[] firstDelivery;
    [SerializeField] private float initialSpeed = 2;

    [Header("delivery test")]
    [SerializeField] private ItemDataSO[] testDeliveryItems;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        deliveryPoint = FindAnyObjectByType<Quest_CreationPointForDeliveryBox>().transform;

        if (firstDelivery.Length > 0)
            CreateDeliveryBox(firstDelivery.ToList());
    }

    private void Update()
    {
        //    if (Input.GetKeyDown(KeyCode.Q))
        //        TestEmptyDelivery();
    }

    [ContextMenu("Test Empty Delivery")]
    public void TestEmptyDelivery()
    {
        CreateDeliveryBox(new List<ItemDataSO>());
    }

    public void BuyItem(List<ItemDataSO> itemsToBuy, int cost)
    {
        CurrencyManager.instance.Pay(cost, out List<GameObject> change);
        CreateDeliveryBox(itemsToBuy, change);
    }

    public void CreateDeliveryBox(List<ItemDataSO> inBoxDelivery, List<GameObject> extraItems = null)
    {
        StartCoroutine(CreateDeliveryBoxCo(inBoxDelivery, extraItems));
    }

    private IEnumerator CreateDeliveryBoxCo(List<ItemDataSO> inBoxDelivery, List<GameObject> extraItems = null)
    {
        List<Item_Base> itemList = new List<Item_Base>();
        List<GameObject> boxContent = new List<GameObject>();
        Item_DeliveryBox deliveryBox = itemManager.CreateItem(deliveryBoxPrefab).GetComponent<Item_DeliveryBox>();


        if (inBoxDelivery != null)
        {
            foreach (var data in inBoxDelivery)
            {
                GameObject newItem = itemManager.CreateItem(data);
                newItem.gameObject.SetActive(false);
                boxContent.Add(newItem);
                yield return null;
            }
        }

        if (extraItems != null)
        {
            foreach (var item in extraItems)
            {
                item.gameObject.SetActive(false);
                boxContent.Add(item);
            }
        }

        foreach (var content in boxContent)
        {
            if (content == null)
            {
                Debug.Log("There was no game object in items");
                continue;
            }

            Item_Base item = content.GetComponent<Item_Base>();

            if (item == null)
            {
                Debug.LogWarning($"GameObject {content.name} does not have an Item_Base component.");
                continue;
            }

            item.EnableKinematic(true);
            item.SetVelocity(Vector3.zero);

            item.transform.SetParent(deliveryBox.transform, true);
            item.transform.localPosition = Vector3.zero;
            itemList.Add(item);
        }

        deliveryBox.transform.position = deliveryPoint.position;
        deliveryBox.transform.localScale = Vector3.one * .35f;

        yield return new WaitForSeconds(.1f);
        deliveryBox.SetupBox(itemList);
    }
}
