using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Alexdev.TweenUtils;

public class UI_ButtonOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Color idleTextColor;
    [SerializeField] private Color selectedTextColor;

    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private Image buttonBg;

    [SerializeField] private float onHoverScale = 1.15f;
    [SerializeField] private float transitionDur = .1f;

    private void Start()
    {
        HighlightButton(false);
    }

    private void HighlightButton(bool isHovered)
    {
        if(buttonText != null)
            buttonText.color = isHovered ? selectedTextColor : idleTextColor;

        if(buttonBg != null)
            buttonBg.gameObject.SetActive(isHovered);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        HighlightButton(true);

        StartCoroutine(ScaleLocal(transform, Vector3.one * onHoverScale, 0.1f));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HighlightButton(false);
        StartCoroutine(ScaleLocal(transform, Vector3.one, 0.1f));
    }
}
