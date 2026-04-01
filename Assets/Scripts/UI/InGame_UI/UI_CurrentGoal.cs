using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Alexdev;

public class UI_CurrentGoal : MonoBehaviour
{
    [SerializeField] private VerticalLayoutGroup parentVerticalLayout;
    [SerializeField] private float parentSpacing = -2f;
    [Space]
    [SerializeField] private Image background;
    [SerializeField] private TextMeshProUGUI[] goalText;
    [SerializeField] private int maxLineLength = 30;
    [SerializeField] private float fadeDuration = 0.25f;

    Coroutine updateRoutine;


    public void UpdateGoal(string text)
    {
        if (updateRoutine != null)
            StopCoroutine(updateRoutine);

        updateRoutine = StartCoroutine(UpdateGoalRoutine(text));
    }

    private IEnumerator UpdateGoalRoutine(string text)
    {
        // fade out
        foreach (var t in goalText)
            StartCoroutine(TweenUtils.ColorTo(t, new Color(t.color.r, t.color.g, t.color.b, 0), fadeDuration));

        yield return new WaitForSeconds(fadeDuration);

        text = InsertLineBreaks(text, maxLineLength);

        foreach (var t in goalText)
            t.text = text;

        // wait for TextMeshPro + layout to update
        yield return null;

        // APPLY spacing AFTER layout updated
        parentVerticalLayout.spacing = parentSpacing;

        // FORCE rebuild
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)parentVerticalLayout.transform);

        // fade in
        foreach (var t in goalText)
            StartCoroutine(TweenUtils.ColorTo(t, new Color(t.color.r, t.color.g, t.color.b, 1), fadeDuration));
    }

    private string InsertLineBreaks(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        int current = 0;

        while (current + maxLength < text.Length)
        {
            int breakIndex = text.LastIndexOf(' ', current + maxLength, maxLength);
            if (breakIndex == -1)
                breakIndex = current + maxLength;

            text = text.Insert(breakIndex, "\n");
            current = breakIndex + 1;
        }

        return text;
    }
}