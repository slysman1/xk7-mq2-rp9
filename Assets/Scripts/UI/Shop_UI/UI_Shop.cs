using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Alexdev.TweenUtils;

public class UI_Shop : MonoBehaviour
{
    public CanvasGroup canvasGroup { get; private set; }
    private UI_Upgrade upgradeUI;
    private UI_Decor decorUI;

    //[SerializeField] private Transform upgradeBTN;
    [SerializeField] private UI_TextFeedback creddit;
    [SerializeField] private UI_TextFeedback respect;

    [SerializeField] private Transform[] uiElements;
    [SerializeField] private CanvasGroup shopHider;
    [SerializeField] private float transitionDur = .2f;
    [SerializeField] private Scrollbar[] scrollbar;

    [Header("Tab Buttons")]
    [SerializeField] private Transform tabButtons;
    [SerializeField] private TextMeshProUGUI upgradeButtonText;
    [SerializeField] private TextMeshProUGUI decorButtonText;
    [SerializeField] private Color shopActiveColor;
    [SerializeField] private Color shopeIdleColor;

    [Header("Upgrade Details")]
    [SerializeField] private RectTransform upgradeShopContent;
    [SerializeField] private RectTransform upgradeShopClosed;

    public enum TabButtonType { Upgrade,Decor};

    private void Awake()
    {
        upgradeUI = GetComponentInChildren<UI_Upgrade>();
        decorUI = GetComponentInChildren<UI_Decor>();

        scrollbar = GetComponentsInChildren<Scrollbar>(true);
    }

    private void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OpenUI(Transform uiToOpen)
    {
        if (uiToOpen.GetComponent<CanvasGroup>().alpha == 1)
            return;

        foreach (var item in scrollbar)
            item.value = 0;

        StopAllCoroutines();
        StartCoroutine(OpenUICo(uiToOpen));
    }

    public void OpenUpgradeUI()
    {
        UpdateTabButtons(TabButtonType.Upgrade);

        bool allQuestsFinished = OrderManager.instance.HasActiveQuests() == false;

        upgradeShopContent.gameObject.SetActive(allQuestsFinished);
        upgradeShopClosed.gameObject.SetActive(!allQuestsFinished);

        OpenUI(upgradeUI.transform);
    }

    public void OpenDecorUI()
    {
        UpdateTabButtons(TabButtonType.Decor);

        decorUI.SetupDecorIfNeeded();
        OpenUI(decorUI.transform);
    }


    private IEnumerator OpenUICo(Transform uiToOpen)
    {
        yield return StartCoroutine(SetCanvasAlphaTo(shopHider, 1, transitionDur / 2));

        foreach (var uiElement in uiElements)
        {
            if (uiElement == uiToOpen)
                continue;

            var cg = uiElement.GetComponent<CanvasGroup>();

            if (cg != null)
            {
                StartCoroutine(SetCanvasAlphaTo(cg, 0, .01f));
                cg.blocksRaycasts = false;
                cg.interactable = false;
            }
        }

        yield return new WaitForSeconds(.01f);
        var canvasToShow = uiToOpen.GetComponent<CanvasGroup>();

        if (canvasToShow != null)
        {
            StartCoroutine(SetCanvasAlphaTo(canvasToShow, 1, .01f));
            canvasToShow.blocksRaycasts = true;
            canvasToShow.interactable = true;
        }

        StartCoroutine(SetCanvasAlphaTo(shopHider, 0, transitionDur / 2));
    }

    private void UpdateTabButtons(TabButtonType tabButtonType)
    {
        
        bool canShowTabButtons = UpgradeManager.instance.currentUpgrade != null;
        tabButtons.gameObject.SetActive(canShowTabButtons);


        upgradeButtonText.color = shopeIdleColor;
        decorButtonText.color = shopeIdleColor;

        if (tabButtonType == TabButtonType.Upgrade)
            upgradeButtonText.color = shopActiveColor;

        if (tabButtonType == TabButtonType.Decor)
            decorButtonText.color = shopActiveColor;
    }

    public void PlayCreditFeedback() => creddit.PlayTextFeedback();
    public void PlayRespectFeedback() => respect.PlayTextFeedback();


}
