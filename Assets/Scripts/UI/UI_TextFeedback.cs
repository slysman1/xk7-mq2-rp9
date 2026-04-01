using Michsky.UI.Dark;
using System.Collections;
using TMPro;
using UnityEngine;
using static Alexdev.TweenUtils;

public class UI_TextFeedback : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float feedbackDuration = 0.4f;

    [Header("Shake")]
    [SerializeField] private float positionShake = 5f;
    [SerializeField] private float rotationShake = 5f;
    private Vector3 defaultPos;
    private Quaternion defaultRot;

    [Header("Scale")]
    [SerializeField] private float scaleMulitplier = 1f;
    [SerializeField] private float scaleUpDuration = .1f;
    private Vector3 defaultScale;

    [Header("Color")]
    [SerializeField] private Color feedbackColor = Color.red;
    private Color defaultColor;


    private TextMeshProUGUI[] text;
    private RectTransform rect;
    private Coroutine feedbackCo;
    private UIManagerText uiManagerText;
    private bool savedDefaultInfo;

    private void Awake()
    {
        text = GetComponentsInChildren<TextMeshProUGUI>();
        rect = GetComponent<RectTransform>();
        uiManagerText = GetComponent<UIManagerText>();
    }

    public void PlayTextFeedback()
    {
        if (feedbackCo != null)
            StopCoroutine(feedbackCo);

        feedbackCo = StartCoroutine(PlayTextFeedbackCo());
    }

    private IEnumerator PlayTextFeedbackCo()
    {
        InitializeDefaultInfoIfNeeded();

        StartCoroutine(ScaleUI(rect, Vector3.one * scaleMulitplier, scaleUpDuration));

        foreach (var item in text)
            item.color = feedbackColor;

        float timer = 0f;
        while (timer < feedbackDuration)
        {
            timer += Time.unscaledDeltaTime;

            float posOffset = Mathf.Sin(timer * 50f) * positionShake;
            float rotOffset = Mathf.Sin(timer * 40f) * rotationShake;

            rect.localPosition = defaultPos + (Vector3.right * posOffset);
            rect.localRotation = Quaternion.Euler(0f, 0f, rotOffset);

            yield return null;
        }

        // restore
        StartCoroutine(ScaleUI(rect, defaultScale, scaleUpDuration));
        rect.localPosition = defaultPos;
        rect.localRotation = defaultRot;

        foreach (var item in text)
            item.color = defaultColor;

        //uiManagerText.useCustomColor = false;
    }

    private void InitializeDefaultInfoIfNeeded()
    {
        if (savedDefaultInfo == false)
        {

            defaultPos = rect.localPosition;
            defaultRot = rect.localRotation;
            defaultColor = text[0].color;
            defaultScale = rect.localScale;
            savedDefaultInfo = true;
        }
        else
        {
            rect.localPosition = defaultPos;
            rect.localRotation = defaultRot;
            text[0].color = defaultColor;
            rect.localScale = defaultScale;
        }
    }
}
