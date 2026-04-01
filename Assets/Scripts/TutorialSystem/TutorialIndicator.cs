using UnityEngine;

public static class TutorialIndicator
{
    private static UI_OnObjectIndicator activeIndicator;

    public static void HighlightTarget<T>(bool usePriority = false) where T : Component
    {
        // Try priority target first
        if (usePriority)
        {
            var targets = Object.FindObjectsByType<T>(FindObjectsSortMode.None);

            foreach (var t in targets)
            {
                IndicatorPriority indicatorPriority = t.GetComponent<IndicatorPriority>();
                if (indicatorPriority != null && indicatorPriority.hasPriority == true)
                {
                    UI.instance.taskIndicator.AddTarget(t.transform);
                    return;
                }
            }

            Debug.LogWarning($"Priority {typeof(T)} not found, falling back.");
        }

        // Fallback
        T target = Object.FindFirstObjectByType<T>();
        if (target == null)
        {
            Debug.LogWarning($"Tutorial target {typeof(T)} not found");
            return;
        }

        UI.instance.taskIndicator.AddTarget(target.transform);
    }


    public static void HighlightAllTargets<T>() where T : Component
    {

        T[] targets = Object.FindObjectsByType<T>(FindObjectsInactive.Include,FindObjectsSortMode.None);

        if (targets.Length == 0)
        {
            Debug.Log($"No tutorial targets of type {typeof(T)} found");
            return;
        }

        foreach (var target in targets)
            UI.instance.taskIndicator.AddTarget(target.transform);
    }

    public static void HighlightTargets<T>(int amount) where T : Component
    {
        if (amount <= 0)
            return;

        T[] targets = Object.FindObjectsByType<T>(FindObjectsSortMode.None);

        if (targets.Length == 0)
        {
            Debug.LogWarning($"No tutorial targets of type {typeof(T)} found");
            return;
        }

        int count = Mathf.Min(amount, targets.Length);

        for (int i = 0; i < count; i++)
        {
            if (targets[i] == null)
                continue;

            UI.instance.taskIndicator.AddTarget(targets[i].transform);
        }
    }

    public static void HighlightTargetList(System.Collections.Generic.List<Transform> targets)
    {
        if (targets == null || targets.Count == 0)
        {
            Debug.LogWarning("No tutorial targets provided");
            return;
        }

        foreach (var target in targets)
        {
            if (target == null)
                continue;

            UI.instance.taskIndicator.AddTarget(target);
        }
    }

    public static void HighlightTargetTransform(Transform target)
    {
        if (target == null)
        {
            Debug.LogWarning("Tutorial target is null");
            return;
        }

        UI.instance.taskIndicator.AddTarget(target);
    }

    public static void Clear()
    {
        UI.instance.taskIndicator.ClearAll();
    }
}
