using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_OrderList : MonoBehaviour
{
    private UI_OrderSlot[] slots;
    public UI_TextFeedback feedback { get; private set; }
    [SerializeField] private bool hideOnStart;

    private Color bgColor;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI orderListTittle;

    private void Awake()
    {
        slots = GetComponentsInChildren<UI_OrderSlot>(true);
        feedback = GetComponent<UI_TextFeedback>();
        HideOrderList(hideOnStart);
    }

    public void UpdateOrderList(List<OrderDataSO> currentQuests)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (currentQuests.Count <= i)
            {
                slots[i].gameObject.SetActive(false);
                continue;
            }

            if (currentQuests[i] != null)
            {
                slots[i].gameObject.SetActive(true);
                slots[i].UpdateSlot(currentQuests[i]);
            }
        }

        bool hideOrderList = currentQuests.Count <= 0;
        HideOrderList(hideOrderList);
    }

    private void HideOrderList(bool hide)
    {
        if (hide)
        {
            backgroundImage.color = Color.clear;
            orderListTittle.text = "";
        }
        else
        {
            backgroundImage.color = bgColor;
            orderListTittle.text = Localization.GetString("ui_order_list_tittle");
        }
    }
}
