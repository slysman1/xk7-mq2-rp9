using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_InputHelpSlot : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI iconCode;
    [SerializeField] private TextMeshProUGUI description;

    public void SetupSlot(string iconCode, string slotText)
    {
        this.iconCode.text = iconCode;
        description.text = slotText;
    }
}
