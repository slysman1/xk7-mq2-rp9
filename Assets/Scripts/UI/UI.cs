//using Michsky.UI.Dark;
using System;
using System.Collections;
using UnityEngine;
using static Alexdev.TweenUtils;



public class UI : MonoBehaviour
{
    public static event Action OnEnableInGameUI;
    public static event Action OnEnableUI;

    public static UI instance;
    public Player player { get; private set; }

    public UI_Shop shopUI { get; private set; }
    public UI_InGame inGameUI { get; private set; }
    public UI_Pointer pointerUI { get; private set; }
    public UI_Upgrade upgradeListUI { get; private set; }
    public UI_TaskIndicator taskIndicator { get; private set; }
    public UI_Notification notification { get; private set; }
    public UI_PauseMenu pauseMenuUI { get; private set; }
    public UI_InputHelp inputHelp { get; private set; }
    public UI_Decor decorUI { get; private set; }
    public UI_Dialogue dialogueUI { get; private set; }

    [SerializeField] private Transform[] uiElements;

    [Header("UI Animations")]
    [SerializeField] private float hideUiDurration = .25f;
    [SerializeField] private float showUiDiration = .25f;

    private void Awake()
    {
        instance = this;

        shopUI = GetComponentInChildren<UI_Shop>(true);
        inGameUI = GetComponentInChildren<UI_InGame>(true);
        pointerUI = GetComponentInChildren<UI_Pointer>(true);
        shopUI = GetComponentInChildren<UI_Shop>(true);
        upgradeListUI = GetComponentInChildren<UI_Upgrade>(true);
        taskIndicator = GetComponentInChildren<UI_TaskIndicator>(true);
        notification = GetComponentInChildren<UI_Notification>(true);
        inputHelp = GetComponentInChildren<UI_InputHelp>(true);
        pauseMenuUI = GetComponentInChildren<UI_PauseMenu>(true);
        decorUI = GetComponentInChildren<UI_Decor>(true);
        dialogueUI = GetComponentInChildren<UI_Dialogue>(true);
    }

    private void Start()
    {
        player = FindFirstObjectByType<Player>();

        player.input.UI.Cancel.performed += ctx =>
        {
            if (inGameUI.IsActive())
                OpenPauseUI();
            else
                OpenInGameUI();
        };

        player.input.UI.OpenMerchant.performed += ctx => OpenShopUI();
        MainNPC.OnDoorKnocked += OpenDialogueUI;

        OpenInGameUI();
    }


    private void DebugUIUnderMouse()
    {
        if (UnityEngine.EventSystems.EventSystem.current == null) return;

        var data = new UnityEngine.EventSystems.PointerEventData(
            UnityEngine.EventSystems.EventSystem.current);

        data.position = Input.mousePosition;

        var results = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
        UnityEngine.EventSystems.EventSystem.current.RaycastAll(data, results);

        if (results.Count == 0) return;

        var hovered = results[0].gameObject;

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Clicked: " + hovered.name);
            UnityEngine.EventSystems.ExecuteEvents.Execute(
                hovered,
                data,
                UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);
        }
    }

    public void OpenUI(Transform uiToOpen)
    {
        if (uiToOpen == inGameUI.transform)
            OnEnableInGameUI?.Invoke();
        else
            OnEnableUI?.Invoke();


        StopAllCoroutines();
        StartCoroutine(OpenUICo(uiToOpen));
    }

    private IEnumerator OpenUICo(Transform uiToOpen)
    {
        foreach (var uiElement in uiElements)
        {
            if (uiElement == uiToOpen)
                continue;

            var cg = uiElement.GetComponent<CanvasGroup>();

            if (cg != null)
            {
                StartCoroutine(SetCanvasAlphaTo(cg, 0, hideUiDurration));
                cg.blocksRaycasts = false;
                cg.interactable = false;
            }
        }

        yield return new WaitForSeconds(hideUiDurration);
        var canvasToShow = uiToOpen.GetComponent<CanvasGroup>();

        if (canvasToShow != null)
        {
            Audio.QueSFX("ui_open_shop", player.transform, showUiDiration - .1f);
            StartCoroutine(SetCanvasAlphaTo(canvasToShow, 1, showUiDiration));
            canvasToShow.blocksRaycasts = true;
            canvasToShow.interactable = true;
        }
    }


    public void NotifyPlayer(string key, bool canRepeatNotification = false)
    {
        //notification.NotifyPlayer(key, canRepeatNotification);
    }

    public void HideNotification() => notification.HideNotification();

    public void OpenShopUI()
    {
        OpenUI(shopUI.transform);
        shopUI.OpenUpgradeUI();
    }

    public void OpenDecorUI()
    {
        OpenUI(shopUI.transform);
        shopUI.OpenDecorUI();
    }

    public void OpenInGameUI() => OpenUI(inGameUI.transform);
    public void OpenPauseUI() => OpenUI(pauseMenuUI.transform);
    public void OpenDialogueUI()
    {
        OpenUI(dialogueUI.transform);
        dialogueUI.SetupDialogueIfNeeded();
    }
}
