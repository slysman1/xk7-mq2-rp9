using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum KeyType { LMB, LMB_Hold, RMB, RMB_Hold, F, E }

public class UI_InputHelp : MonoBehaviour
{
    private UI_TextFeedback feedback;
    private Player_Inventory inventory;
    private Player player;
    private CanvasGroup canvasGroup;
    private UI_InputHelpSlot[] helpSlots;

    private readonly List<InputUI> priorityInputs = new();
    public List<InputUI> activeInputs = new();

    

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        feedback = GetComponentInChildren<UI_TextFeedback>();
        helpSlots = GetComponentsInChildren<UI_InputHelpSlot>(true);
        player = FindFirstObjectByType<Player>();

    }

    private void Start()
    {
        inventory = player.inventory;
        inventory.OnItemAmountUpdate += () => 
        {
            AddInputForItemsInHand();
            Refresh();
        };
        //inventory.OnItemAmountUpdate += AddInputForItemsInHand;

        Refresh();
    }


    public void AddInput(KeyType keyType, string inputKey, bool forceInput = false)
    {
        if (activeInputs.Exists(i => i.keyType == keyType) && forceInput == false)
            return;

        if (forceInput)
            activeInputs.RemoveAll(i => i.keyType == keyType);

        InputUI input = new InputUI
        {
            inputIcon = GetInputIconString(keyType),
            inputDescription = Localization.GetString(inputKey),
            orderIndex = ((int)keyType),
            keyType = keyType,
        };


        activeInputs.Add(input);
        Refresh();
    }


    public void RemoveInputByKey(KeyType keyType)
    {
        activeInputs.RemoveAll(i => i.keyType == keyType);
     //   Refresh();
    }

    public void RemoveInput()
    {
        activeInputs.Clear();
       // Refresh();
    }

    public void Refresh()
    {
        foreach (var slot in helpSlots)
            slot.gameObject.SetActive(false);

        List<InputUI> sourceList;

        if (priorityInputs.Count > 0)
        {
            sourceList = priorityInputs;
        }
        else
        {
            AddInputForItemsInHand();
            sourceList = activeInputs;
        }

        if (sourceList.Count == 0)
        {
            canvasGroup.alpha = 0;
            return;
        }

        sourceList.Sort((a, b) => a.orderIndex.CompareTo(b.orderIndex));

        for (int i = 0; i < sourceList.Count && i < helpSlots.Length; i++)
        {
            helpSlots[i].gameObject.SetActive(true);
            helpSlots[i].SetupSlot(sourceList[i].inputIcon, sourceList[i].inputDescription);
        }

        canvasGroup.alpha = 1;
    }

    public void AddInputForItemsInHand()
    {
        int itemsAmount = inventory.GetCarriedItems().Count;

        //if (itemsAmount == 1)
        //    AddInput(KeyType.RMB, "input_drop_click");

        bool needToShowInput = itemsAmount > 0 && player.previewHandler.isPlacingItem == false;

        if (needToShowInput)
        {
            AddInput(KeyType.RMB, "input_drop_click");
            AddInput(KeyType.RMB_Hold, "input_drop_hold_aim");
        }
        else
        {
            //if(activeInputs.Contains(new InputUI( KeyType.RMB, "input_drop_click"))

            //if(activeInputs.Contains(KeyType.RMB_Hold, "input_drop_hold_aim"))
            RemoveInputByKey(KeyType.RMB);
            RemoveInputByKey(KeyType.RMB_Hold);
        }
    }


    private string GetInputIconString(KeyType keyType)
    {
        int index = keyType switch
        {
            KeyType.LMB => 0,
            KeyType.LMB_Hold => 1,
            KeyType.RMB => 2,
            KeyType.RMB_Hold => 3,
            KeyType.E => 4,
            KeyType.F => 5,
            _ => 0
        };

        return $"<sprite index={index}>";
    }

    public void ShowInputHelpFeedback() => feedback.PlayTextFeedback();
}

[System.Serializable]
public class InputUI
{
    public string inputIcon;
    public string inputDescription;
    public int orderIndex;
    public KeyType keyType;
}
