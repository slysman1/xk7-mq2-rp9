using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class UI_Dialogue : MonoBehaviour
{
    private UI ui;
    private CanvasGroup dialogueCanvasGroup;
    private DialogueManager dialogueManager => DialogueManager.instance;

    [SerializeField] private TextMeshProUGUI npcText;
    [SerializeField] private UI_DialogueAnswerSlot[] answerSlots;
    [SerializeField] private GameObject closeDialogueBTN;
    private DialogueNodeSO nextPriorityDialogue;



    private void Awake()
    {
        ui = GetComponentInParent<UI>();
        dialogueCanvasGroup = GetComponent<CanvasGroup>();

        answerSlots = GetComponentsInChildren<UI_DialogueAnswerSlot>(true);

    }

    private void Start()
    {
        Player.instance.input.UI.Submit.performed += ctx => ConfirmDialogueChoice();
        Player.instance.input.UI.Navigate.performed += ctx => SelectDialogueChoice();
        UI_DialogueAnswerSlot.DialogeChoiceConfirmed += ShowNextDialogue;
        UI_DialogueAnswerSlot.OnConversationFinished += UpdatePriorityDialogue;
    }

    private void SelectDialogueChoice()
    {
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            SelectSlot(0);
            return;
        }
    }

    public void ConfirmDialogueChoice()
    {
        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;

        if (selectedObject == null)
        {
            SelectSlot(0);
            return;
        }

        //selectedObject.GetComponent<UI_DialogueAnswerSlot>().button.onClick.Invoke();
        selectedObject.GetComponent<Button>().onClick.Invoke();
    }

    public void EnableCloseDialogueBTN(bool enable) => closeDialogueBTN.SetActive(enable);

    private void ShowNextDialogue(DialogueNodeSO dialogueNodeSO)
    {
        ShowDialogueNode(dialogueNodeSO);
    }

    public void SetupDialogueIfNeeded()
    {
        StartDefaultDialogue();
        EnableCloseDialogueBTN(false);
    }


    public void StartDefaultDialogue() => ShowDialogueNode(dialogueManager.GetNextDialogue());
    public void StartShopRejectDialogue() => ShowDialogueNode(dialogueManager.shopRejectDialogue);

    public void ShowDialogueNode(DialogueNodeSO dialogueNode)
    {

        

        nextPriorityDialogue = dialogueNode.followingDialogue;
        npcText.text = $"{Localization.GetString("ui_dialogue_guard_name_guard")} - \"{dialogueNode.GetRandomLine()}\"";

        List<DialogueChoice> choices = dialogueNode.dialogueChoices;

        foreach (var slot in answerSlots)
            slot.gameObject.SetActive(false);

        bool endOfConversation = choices.Count == 0;


        if (endOfConversation == false)
        {
            for (int i = 0; i < choices.Count && i < answerSlots.Length; i++)
            {
                answerSlots[i].gameObject.SetActive(true);
                answerSlots[i].SetupSlot(choices[i].nextNode, choices[i].actionType, choices[i].playersReply, i + 1);
            }
        }
        else
        {
            answerSlots[0].gameObject.SetActive(true);
            answerSlots[0].SetupSlot(null, DialogueActionType.NoAction, null, 0);
            EnableCloseDialogueBTN(true);
        }


        SelectSlot(0);
    }

    public void UpdatePriorityDialogue()
    {
        if (nextPriorityDialogue != null)
        {
            dialogueManager.SetPriorityDialogue(nextPriorityDialogue);
            nextPriorityDialogue = null;
            return;
        }
    }

    public void ResetPriorityDialogue() => nextPriorityDialogue = null;


    private void OnDestroy()
    {
        UI_DialogueAnswerSlot.DialogeChoiceConfirmed -= ShowNextDialogue;
    }

    private void Update()
    {
        SelectSlotIfKeyboardUsed();
    }

    private void SelectSlotIfKeyboardUsed()
    {
        var keyboard = Keyboard.current;

        if (keyboard == null || dialogueCanvasGroup.alpha != 1)
            return;

        for (int i = 0; i < answerSlots.Length; i++)
        {
            var key = keyboard[(Key)((int)Key.Digit1 + i)];

            if (key != null && key.wasPressedThisFrame)
            {
                //answerSlots[i].ConfirmDialogueChoice();
                SelectSlot(i);
            }
        }
    }
    private void SelectSlot(int index)
    {
        if (index < 0 || index >= answerSlots.Length)
            return;

        if (!answerSlots[index].gameObject.activeSelf)
            return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(answerSlots[index].GetButton().gameObject);
    }

    public UI_DialogueAnswerSlot GetFirstDialogueChoice() => answerSlots[0];

}