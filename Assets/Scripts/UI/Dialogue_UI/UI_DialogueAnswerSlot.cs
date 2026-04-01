using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_DialogueAnswerSlot : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
{
    public static event Action<DialogueNodeSO> DialogeChoiceConfirmed;
    public static event Action OnDeliveryConfirmed;
    public static event Action OnConversationFinished;

    private OrderManager questManager => OrderManager.instance;

    private DialogueNodeSO nodeInSlot;
    public Button button { get ; private set; } 
    private UI ui;
    private UI_Dialogue dialogueUI;
    private TextMeshProUGUI dialugeAnswerText;

    [SerializeField] private Color defaultColor;
    [SerializeField] private Color onSelectColor;
    [SerializeField] private DialogueActionType slotActionType;

    private void Awake()
    {
        button = GetComponent<Button>();
        ui = GetComponentInParent<UI>();
        dialogueUI = GetComponentInParent<UI_Dialogue>();
        dialugeAnswerText = GetComponentInChildren<TextMeshProUGUI>();        
    }

    public void ConfirmDialogueChoice()
    {
        bool endOfConversation = nodeInSlot == null;
        Audio.PlaySFX("ui_confirm_dialogue", ui.player.transform);

        if (endOfConversation)
        {
            EndConversation();
            return;
        }

        if (slotActionType == DialogueActionType.OpenShop)
        {
            if (questManager.HasActiveQuests())
            {
                dialogueUI.StartShopRejectDialogue();
                return;
            }

            OnConversationFinished?.Invoke();
            ui.OpenShopUI();
            return;
        }

        if (slotActionType == DialogueActionType.OpenDecor)
        {
            OnConversationFinished?.Invoke();
            ui.OpenDecorUI();
            return;
        }

        if (slotActionType == DialogueActionType.ConfirmDelivery)
        {
            OnDeliveryConfirmed?.Invoke();
            EndConversation();
            return;
        }

        DialogeChoiceConfirmed?.Invoke(nodeInSlot);
    }

    private void EndConversation()
    {
        OnConversationFinished?.Invoke();
        ui.OpenInGameUI();
    }

    public void SetupSlot(DialogueNodeSO newNode, DialogueActionType dialogueActionType, string slotText, int answerNumber)
    {
        string answerNumberText = answerNumber == 0 ? "" : $"{answerNumber}: ";

        dialugeAnswerText.text = slotText == null ? "" : $"{answerNumberText} \"{slotText}\"";
        slotActionType = dialogueActionType;


        if (newNode == null)
        {
            nodeInSlot = null;
            return;
        }

        nodeInSlot = newNode;
    }

    public DialogueNodeSO GetNodeInSlot() => nodeInSlot;
    public Button GetButton()
    {
        if (button == null)
            button = GetComponent<Button>();

        return button;
    }

    public void OnSelect(BaseEventData eventData)
    {
        ColorAsSelected(true);
        Audio.PlaySFX("ui_on_select_upgrade", ui.player.transform);
        
    }

    public void OnDeselect(BaseEventData eventData)
    {
        ColorAsSelected(false);
    }

    private void OnDisable()
    {
        ColorAsSelected(false);
    }

    private void ColorAsSelected(bool needToColor)
    {
        dialugeAnswerText.color = needToColor ? onSelectColor : defaultColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        EventSystem.current.SetSelectedGameObject(gameObject);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (EventSystem.current.currentSelectedGameObject == gameObject)
        {
            EventSystem.current.SetSelectedGameObject(null);
            ColorAsSelected(false);
        }

    }
}
