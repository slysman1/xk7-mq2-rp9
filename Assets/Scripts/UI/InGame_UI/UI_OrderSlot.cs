using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_OrderSlot : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI slotText;
    

    public void UpdateSlot(OrderDataSO orderData)
    {
        slotText.text = orderData.GetOrderDescription();
    }
}
