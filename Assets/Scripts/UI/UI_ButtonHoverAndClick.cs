using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using static Alexdev.TweenUtils;

public class UI_ButtonHoverAndClick : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    private RectTransform rect;
    private UI ui;

    [Header("Hover Feedback")]
    [SerializeField] private bool scaledOnHover = true;
    [SerializeField] private float onHoverScale = 1.1f;
    [SerializeField]
    private float onHoverDur = .25f;

    [Header("Scale Feedback")]
    [SerializeField] private bool scaledOnClick = true;
    [SerializeField] private float onClickFeedbackDur = .1f;

    private Coroutine scaleCo;
    protected virtual void Awake()
    {
        rect = GetComponent<RectTransform>();
        ui = GetComponentInParent<UI>();
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {


        if (scaledOnHover == false)
            return;

        Audio.PlaySFX("ui_on_select_upgrade", ui.player.transform);


        if (scaleCo != null)
            StopCoroutine(scaleCo);

        scaleCo = StartCoroutine(ScaleUI(rect, Vector3.one * onHoverScale, onHoverDur));
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        if (scaledOnHover == false)
            return;

        if (scaleCo != null)
            StopCoroutine(scaleCo);

        scaleCo = StartCoroutine(ScaleUI(rect, Vector3.one, onHoverDur));
    }

    private IEnumerator OnClickFeedbackCo()
    {
        float originalScale = rect.localScale.x;

        yield return StartCoroutine(ScaleUI(rect, Vector3.one, onClickFeedbackDur));
        StartCoroutine(ScaleUI(rect, Vector3.one * originalScale, onClickFeedbackDur));
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (scaledOnClick == false)
            return;

        StopAllCoroutines();
        StartCoroutine(OnClickFeedbackCo());
    }

}
