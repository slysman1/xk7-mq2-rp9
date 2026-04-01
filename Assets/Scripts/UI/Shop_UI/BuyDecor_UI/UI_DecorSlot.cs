using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Alexdev.TweenUtils;

public class UI_DecorSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static event Action OnDecorPurchase;
    private CurrencyManager currencyManager => CurrencyManager.instance;
    private DeliveryManager deliveryManager => DeliveryManager.instance;
    private UpgradeManager upgradeManager => UpgradeManager.instance;
    private ItemManager itemManager => ItemManager.instance;

    private UI ui;
    private RectTransform rect;


    private UnlockDecorDataSO decorInSlot;
    private bool isLocked;

    [SerializeField] private Image decorIcon;
    private int decorFavourCost;
    private int maxLimit;

    [Header("Button Animation")]
    [SerializeField] private float fadeDuration = .2f;

    [Header("Content Details")]
    [SerializeField] private RectTransform unlockedContentParent;
    [SerializeField] private RectTransform namePriceHolder;
    [SerializeField] private CanvasGroup contentCanva;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private float distanceY = 20f;
    [SerializeField] private TextMeshProUGUI itemAmountText;
    private UI_TextFeedback itemAmountTextFeedback;
    private UI_TextFeedback[] namePriceFeedback;
    private Vector2 originalPos;


    [Header("Locked Content Details")]
    [SerializeField] private RectTransform lockedContentParent;
    [SerializeField] private TextMeshProUGUI lockedContentRequiredText;
    [SerializeField] private TextMeshProUGUI lockedContentUpgraedRequiredNameText;
    private UI_TextFeedback[] lockedTextFeedback;

    [Header("On Click Feedback")]
    [SerializeField] private float feedbackScale = 1.1f;
    [SerializeField] private float onClickFeedbackDur = .1f;
    private Coroutine feedbackCo;

    private void Start()
    {
        ui = GetComponentInParent<UI>();
        rect = GetComponent<RectTransform>();
        namePriceFeedback = namePriceHolder.GetComponentsInChildren<UI_TextFeedback>();
        lockedTextFeedback = lockedContentParent.GetComponentsInChildren<UI_TextFeedback>();
        itemAmountTextFeedback = itemAmountText.GetComponent<UI_TextFeedback>();

        if (namePriceHolder != null)
        {
            originalPos = namePriceHolder.anchoredPosition + new Vector2(0, -distanceY);
            StartCoroutine(MoveUI(namePriceHolder, originalPos, .1f));
            StartCoroutine(SetCanvasAlphaTo(contentCanva, 0f, .1f));
        }
    }


    public void UpdateItemSlot()
    {
        int currentAmount = itemManager.GetItemAmountInGame(decorInSlot.itemData);

        itemAmountText.text = Localization.GetString("ui_decor_item_limit") + $"{currentAmount}/{decorInSlot.GetMaxLimit()}";
        decorFavourCost = decorInSlot.GetCurrentPrice(currentAmount);
        priceText.text = $"{decorFavourCost}";

    }



    public void TryBuyDecor()
    {
        if (isLocked)
        {
            foreach (var item in lockedTextFeedback)
                item.PlayTextFeedback();

            return;
        }

        if (decorInSlot.LimitReached(itemManager.GetItemAmountInGame(decorInSlot.itemData)))
        {
            itemAmountTextFeedback.PlayTextFeedback();
            return;
        }

        List<ItemDataSO> decorItemList = new List<ItemDataSO>();
        decorItemList.Add(decorInSlot.itemData);

        if (currencyManager.HasFavourAmout(decorFavourCost) == false)
        {
            foreach (var item in namePriceFeedback)
                item.PlayTextFeedback();

            return;
        }

        if (feedbackCo != null)
            StopCoroutine(feedbackCo);

        feedbackCo = StartCoroutine(OnClickFeedbackCo());

        currencyManager.RemoveFavour(decorFavourCost);
        deliveryManager.CreateDeliveryBox(decorItemList);
        UpdateItemSlot();

        OnDecorPurchase?.Invoke();
    }


    public void SetupSlot(UnlockDecorDataSO decorItem, bool unlocked)
    {
        isLocked = !unlocked;
        lockedContentParent.gameObject.SetActive(unlocked == false);
        unlockedContentParent.gameObject.SetActive(unlocked);

        decorInSlot = decorItem;
        decorIcon.sprite = decorItem.decorIcon;

        lockedContentRequiredText.text = $"{Localization.GetString("ui_upgrade_required")}";
        lockedContentUpgraedRequiredNameText.text = upgradeManager.GetUpgradeNameUnlockingDecor(decorInSlot);
        UpdateItemSlot() ;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isLocked)
        {
            foreach (var item in lockedTextFeedback)
                StartCoroutine(ScaleUI(item.GetComponent<RectTransform>(), Vector3.one * 1.1f, onClickFeedbackDur));

            return;
        }




        if (namePriceHolder == null)// || contentIsLocked)
            return;

        StopAllCoroutines();

        priceText.text = $"{decorFavourCost}";
        Audio.PlaySFX("ui_on_select_upgrade", ui.player.transform);

        StartCoroutine(SetCanvasAlphaTo(contentCanva, 1f, fadeDuration));
        StartCoroutine(MoveUI(namePriceHolder, originalPos + new Vector2(0, distanceY), fadeDuration));

    }

    public void OnPointerExit(PointerEventData eventData)
    {

        if (isLocked)
        {
            foreach (var item in lockedTextFeedback)
                StartCoroutine(ScaleUI(item.GetComponent<RectTransform>(), Vector3.one, onClickFeedbackDur));

            return;
        }


        if (namePriceHolder == null )//|| contentIsLocked)
            return;

        StopAllCoroutines();

        StartCoroutine(SetCanvasAlphaTo(contentCanva, 0f, fadeDuration));
        StartCoroutine(MoveUI(namePriceHolder, originalPos, fadeDuration));
    }

    private IEnumerator OnClickFeedbackCo()
    {
        float originalScale = rect.localScale.x;

        yield return StartCoroutine(ScaleUI(rect, Vector3.one * feedbackScale, onClickFeedbackDur));
        StartCoroutine(ScaleUI(rect, Vector3.one * originalScale, onClickFeedbackDur));
    }

    
}
