using System;

using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Alexdev.TweenUtils;

public class UI_UpgradeSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private UI ui;
    public static event Action<UpgradeDataSO> OnUpgradeBuy;
    private CurrencyManager currencyManager => CurrencyManager.instance;
    private UpgradeManager upgradeManager => UpgradeManager.instance;
    private DeliveryManager deliveryManager => DeliveryManager.instance;
    private Animator anim;


    // FEEDBACK
    private UI_TextFeedback costNameTextFeedback;
    private UI_TextFeedback iconTextFeedback;
    private UI_TextFeedback requirementFeedback;

    [SerializeField] private Button upgradeButton;
    [SerializeField] private Image background;

    [Header("Front Side Details")]
    [SerializeField] private TextMeshProUGUI upgradeTittleName;

    [Header("Back Side Details")]
    [SerializeField] private TextMeshProUGUI upgradeNameText;
    [SerializeField] private TextMeshProUGUI upgradeCostText;
    [SerializeField] private TextMeshProUGUI currenctIconText;
    [SerializeField] private TextMeshProUGUI upgradeDescription;
    [SerializeField] private TextMeshProUGUI upgradeRequirement;
    private string defaultUpgradeInfo;


    [Header("Upgrade Details")]
    [SerializeField] private GameObject[] hideOnUpgrade;


    public bool upgradeUnlocked;
    public UpgradeDataSO upgradeDataInSlot { get; private set; }
    
    private int upgradeIndex;

    [Header("Button Animation")]
    [SerializeField] private float fadeDuration = .2f;
    [SerializeField] private RectTransform normalContent;
    [SerializeField] private float normalDistanceY = 20f;
    [SerializeField] private RectTransform highlightedContent;
    [SerializeField] private float highlightedDistanceY = 20f;

    private CanvasGroup normalCanvGroup;
    private CanvasGroup highlightedCanvGroup;

    private Vector2 highlitedOriginalPos;
    private Vector2 normalOriginalPos;

    private void Awake()
    {
        ui = GetComponentInParent<UI>();
        anim = GetComponent<Animator>();
        costNameTextFeedback = upgradeCostText.GetComponent<UI_TextFeedback>();
        iconTextFeedback = currenctIconText.GetComponent<UI_TextFeedback>();
        requirementFeedback = upgradeRequirement.GetComponent<UI_TextFeedback>();

        if (normalContent != null)
        {
            normalCanvGroup = normalContent.GetComponentInParent<CanvasGroup>();
            normalOriginalPos = normalContent.anchoredPosition;
        }

        if (highlightedContent != null)
        {
            highlightedCanvGroup = highlightedContent.GetComponentInParent<CanvasGroup>();
            highlitedOriginalPos = highlightedContent.anchoredPosition;
        }
    }

    public void SetupSlot(UpgradeDataSO upgradeData)
    {
        upgradeDataInSlot = upgradeData;


        upgradeIndex = upgradeData.GetUpgradeIndex();
        upgradeTittleName.text = upgradeData.GetUpgradeName();
        upgradeNameText.text = upgradeData.GetUpgradeName() + " - ";

        background.sprite = upgradeData.upgradeBackground;
    }

    public void UpdateSlot() => upgradeUnlocked = upgradeManager.UpgradeUnlocked(upgradeDataInSlot);

    public void TryBuyUpgrade()
    {

        if (CanUnlockUpgrade() == false)
            return;

        upgradeManager.UnlockUpgrade(upgradeDataInSlot);
        deliveryManager.BuyItem(upgradeDataInSlot.equipmentToUnlock, upgradeDataInSlot.upgradeCost);
        upgradeUnlocked = true;


        foreach (var obj in hideOnUpgrade)
            obj.SetActive(false);

        Audio.PlayPrioritySFX("ui_buy_upgrade_celebration_music", ui.player.transform);
        Audio.QueSFX("ui_buy_upgrade", ui.player.transform,.1f);
        //Audio.PlaySFX();
        OnUpgradeBuy?.Invoke(upgradeDataInSlot);

        if (upgradeUnlocked)
        {
            upgradeNameText.text = "";
            upgradeDescription.text = "";
            upgradeCostText.text = " " + Localization.GetString("ui_upgrade_unlocked");
        }


        // THIS IS NEEDED FOR TUTORIAL: on first pruchase merchant should close
        // So player can keep doing tutorial
        //if (upgradeDataInSlot.upgradeType == UpgradeType.Rookie_1)
        //UI.instance.OpenInGameUI();

        TutorialIndicator.HighlightTarget<Item_DeliveryBox>();
    }

    private bool CanUnlockUpgrade()
    {
        if (NeededUpgradeUnlocked() == false)
        {
            requirementFeedback.PlayTextFeedback();
            return false;
        }

        if (HasEnoughCredit() == false)
        {
            costNameTextFeedback.PlayTextFeedback();
            iconTextFeedback.PlayTextFeedback();
            return false;
        }

        return true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (upgradeUnlocked)
        {
            eventData.Use();
            return;
        }

        TryBuyUpgrade();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (normalContent == null || highlightedContent == null)
            return;

        StopAllCoroutines();
        anim.enabled = false;

        //string currencyName = $"<sprite index=11>";//Localization.GetString("ui_upgrade_currency_name");

        string requiredUpgrade = $"{Localization.GetString("ui_upgrade_required")}\n{upgradeDataInSlot.GetRequiredUpgradeName()}";
        string requiredCost = $"{upgradeDataInSlot.upgradeCost}";

        upgradeNameText.gameObject.SetActive(NeededUpgradeUnlocked());
        currenctIconText.gameObject.SetActive(NeededUpgradeUnlocked() && upgradeUnlocked == false);
        upgradeCostText.gameObject.SetActive(NeededUpgradeUnlocked());

        upgradeRequirement.gameObject.SetActive(!NeededUpgradeUnlocked());

        upgradeDescription.text = NeededUpgradeUnlocked() ? upgradeDataInSlot.GetUpgradeDescription() : "";
        upgradeCostText.text = requiredCost;
        upgradeRequirement.text = requiredUpgrade;

        if (upgradeUnlocked)
        {
            upgradeNameText.text = "";
            upgradeDescription.text = "";
            upgradeCostText.text = " " + Localization.GetString("ui_upgrade_unlocked");
        }


        // Fade & move
        StartCoroutine(SetCanvasAlphaTo(normalCanvGroup, 0f, fadeDuration));
        StartCoroutine(SetCanvasAlphaTo(highlightedCanvGroup, 1f, fadeDuration));

        Audio.PlaySFX("ui_on_select_upgrade", ui.player.transform);
        // Highlighted goes UP, Normal goes DOWN
        StartCoroutine(MoveUI(highlightedContent, highlitedOriginalPos + new Vector2(0, highlightedDistanceY), fadeDuration));
        StartCoroutine(MoveUI(normalContent, normalOriginalPos - new Vector2(0, normalDistanceY), fadeDuration));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (normalContent == null || highlightedContent == null)
            return;

        StopAllCoroutines();

        // Fade & move back
        StartCoroutine(SetCanvasAlphaTo(normalCanvGroup, 1f, fadeDuration));
        StartCoroutine(SetCanvasAlphaTo(highlightedCanvGroup, 0f, fadeDuration));

        // Highlighted goes DOWN, Normal goes UP
        StartCoroutine(MoveUI(highlightedContent, highlitedOriginalPos, fadeDuration));
        StartCoroutine(MoveUI(normalContent, normalOriginalPos + new Vector2(0, normalDistanceY), fadeDuration));
    }

    public bool NeededUpgradeUnlocked() => upgradeManager.RequiredUpgradeUnlocked(upgradeDataInSlot);
    private bool HasEnoughCredit() => currencyManager.HasCurrencyAmount(upgradeDataInSlot.upgradeCost);
}
