using UnityEngine;
using static Alexdev.TweenUtils;
using System.Collections;
using UnityEngine.EventSystems;

public class UI_ButtonOnClockFeedback : MonoBehaviour, IPointerDownHandler
{
    private RectTransform rect;

    [SerializeField] private float feedbackScale = 1.1f;
    [SerializeField] private float onClickFeedbackDur = .1f;
    private Coroutine feedbackCo;

    private void Start()
    {
        rect = GetComponent<RectTransform>();
    }


    private IEnumerator OnClickFeedbackCo()
    {
        float originalScale = rect.localScale.x;

        yield return StartCoroutine(ScaleUI(rect, Vector3.one * feedbackScale, onClickFeedbackDur));
        StartCoroutine(ScaleUI(rect, Vector3.one * originalScale, onClickFeedbackDur));
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(feedbackCo != null)
            StopCoroutine(feedbackCo);

        feedbackCo = StartCoroutine(OnClickFeedbackCo());
    }
}
