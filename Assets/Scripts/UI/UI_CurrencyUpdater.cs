using TMPro;
using UnityEngine;

public enum CurrencyType { Credit,Respect}

public class UI_CurrencyUpdater : MonoBehaviour
{
    [SerializeField] private CurrencyType currencyType;
    [SerializeField] private TextMeshProUGUI iconTextMesh;
    [SerializeField] private TextMeshProUGUI amountTextMesh; 

    void Start()
    {
        if(currencyType == CurrencyType.Respect)
            CurrencyManager.OnRespectUpdate += UpdateRespect;

        if(currencyType == CurrencyType.Credit)
            CurrencyManager.OnCreditUpdate += UpdateCredit;
    }


    private void UpdateRespect(int currentRespect)
    {
        iconTextMesh.text = $"<sprite index=6>";
        amountTextMesh.text = $"{currentRespect}";
    }

    private void UpdateCredit(int currentCredit)
    {
        iconTextMesh.text = $"<sprite index=11>";
        amountTextMesh.text = $"{currentCredit}";
    }

}
