using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Alexdev.TweenUtils;

public class DeliveryManager : MonoBehaviour
{
    public static DeliveryManager instance;
    private ItemManager itemManager => ItemManager.instance;

    [SerializeField] private ItemDataSO deliveryBoxPrefab;
    [SerializeField] private Transform deliveryPoint;

    [SerializeField] private ItemDataSO[] firstDelivery;
    [SerializeField] private ItemDataSO[] priorityDelivery;
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

    [ContextMenu("Test Delivery")]
    public void TestDelivery()
    {
        if(testDeliveryItems.Length > 0)
            CreateDeliveryBox(testDeliveryItems.ToList(),null);
    }

    [ContextMenu("Test Empty Delivery")]
    public void TestEmptyDelivery()
    {
        CreateDeliveryBox(new List<ItemDataSO>());
    }

    public void BuyItem(List<ItemDataSO> itemsToBuy,int cost)
    {
        CurrencyManager.instance.Pay(cost, out List<GameObject> change);
        CreateDeliveryBox(itemsToBuy, change);
    }


    // We need seprate method with seprate logic to be able to send stamped coins as reward
    public void SendItems(ItemDataSO[] newDelivery)
    {
        priorityDelivery = newDelivery;
        StartCoroutine(CreateDeliveryBoxCo(priorityDelivery.ToList(), true,null));
    }
    public bool HasPriorityDelivery() => priorityDelivery != null && priorityDelivery.Length > 0;
    public void CreatePriorityDelivery()
    {
        CreateDeliveryBox(priorityDelivery.ToList());
        priorityDelivery = null;
    }
    public void CreateDeliveryBox(List<ItemDataSO> inBoxDelivery, List<GameObject> extraItems = null)
    {
        StartCoroutine(CreateDeliveryBoxCo(inBoxDelivery, extraItems));
    }

    private IEnumerator CreateDeliveryBoxCo(List<ItemDataSO> inBoxDelivery, List<GameObject> extraItems = null)
    {
        List<GameObject> boxContent = new List<GameObject>();
        Item_DeliveryBox deliveryBox =
            itemManager.CreateItem(deliveryBoxPrefab).GetComponent<Item_DeliveryBox>();


        if (inBoxDelivery != null)
        {
            foreach (var data in inBoxDelivery)
            {
                GameObject newItem = itemManager.CreateItem(data);
                boxContent.Add(newItem);


                yield return null;
            }

        }

        if (extraItems != null)
        {
            foreach (var item in extraItems)
                boxContent.Add(item);
        }

        if (deliveryBox == null)
        {
            Debug.Log("Box is null");
        }



        deliveryBox.SetupBox(boxContent);
        deliveryBox.transform.position = deliveryPoint.position;
        deliveryBox.transform.localScale = Vector3.one * .35f;

        //Audio.PlaySFX("deliveryBox_metal_impact", deliveryBox.transform);


        //yield return new WaitForSeconds(.1f);

        ////deliveryBox.SetVelocity(deliveryPoint.forward * initialSpeed);
        ////deliveryBox.SetVelocity(Vector3.zero);

    }
    private IEnumerator CreateDeliveryBoxCo(List<ItemDataSO> inBoxDelivery, bool stampedCoins, List<GameObject> extraItems = null)
    {
        List<GameObject> boxContent = new List<GameObject>();
        Item_DeliveryBox deliveryBox =
            itemManager.CreateItem(deliveryBoxPrefab).GetComponent<Item_DeliveryBox>();


        if (inBoxDelivery != null)
        {
            foreach (var data in inBoxDelivery)
            {
                GameObject newItem = itemManager.CreateItem(data);
                boxContent.Add(newItem);


                yield return null;
            }

        }

        if (extraItems != null)
        {
            foreach (var item in extraItems)
                boxContent.Add(item);
        }

        if (deliveryBox == null)
        {
            Debug.Log("Box is null");
        }

        if (stampedCoins)
        {
            foreach (var item in boxContent)
            {
                Item_Coin coin = item?.GetComponent<Item_Coin>();

                if (coin != null)
                    coin.EnableStamps(true);
            }
        }


        deliveryBox.SetupBox(boxContent);
        deliveryBox.transform.position = deliveryPoint.position;
        deliveryBox.transform.localScale = Vector3.one * .35f;

        yield return new WaitForSeconds(.1f);

        deliveryBox.SetVelocity(deliveryPoint.forward * initialSpeed);

        priorityDelivery = null;
    }
}
