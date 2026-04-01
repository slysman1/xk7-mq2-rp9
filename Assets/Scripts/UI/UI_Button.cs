using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Alexdev.TweenUtils;

[RequireComponent(typeof(Button))]
public class UI_Button : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [Header("On Click")]

    [SerializeField] private float pressDuration = .1f;
    [SerializeField] private float releaseDuration = .05f;

    [Header("On Hover")]
    [SerializeField] private float hoverScaleMultiplier = 1.05f;
    [SerializeField] private float hoverDuration = .15f;

    private Button button;
    private Coroutine hoverCo;
    private bool isHoveredOrSelected;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void PlayFeedback()
    {
        if (!isActiveAndEnabled) // checks both: script enabled AND GameObject active
            return;

        StartCoroutine(ButtonFeedBackCo());
    }


    private IEnumerator ButtonFeedBackCo()
    {
        transform.localScale = Vector3.one;
        yield return StartCoroutine(ScaleLocal(transform, Vector3.one * .95f, pressDuration));

        transform.localScale = Vector3.one * .95f;
        StartCoroutine(ScaleLocal(transform, Vector3.one, releaseDuration));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //ApplyHover(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //ApplyHover(false);
    }

    public void OnSelect(BaseEventData eventData)
    {
        //ApplyHover(true);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        //ApplyHover(false);
    }

    private void ApplyHover(bool state)
    {
        isHoveredOrSelected = state;

        if (hoverCo != null)
            StopCoroutine(hoverCo);

        Vector3 targetScale = state ? Vector3.one * hoverScaleMultiplier : Vector3.one;
        hoverCo = StartCoroutine(ScaleLocal(transform, targetScale, hoverDuration));
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(PlayFeedback);
    }

    private void OnEnable()
    {
        button.onClick.AddListener(PlayFeedback);
    }
}
