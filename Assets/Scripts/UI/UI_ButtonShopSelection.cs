using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_ButtonShopSelection : UI_ButtonHoverAndClick
{

    [SerializeField] private CanvasGroup highlightCanva;
    [SerializeField] private CanvasGroup normalCanva;


    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        HighlightButton(true);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        HighlightButton(false);
    }

    private void HighlightButton(bool highlight)
    {
        if (highlight)
        {
            highlightCanva.alpha = 1.0f;
            normalCanva.alpha = 0f;
        }
        else
        {
            highlightCanva.alpha = 0f;
            normalCanva.alpha = 1f;
        }
    }
}
