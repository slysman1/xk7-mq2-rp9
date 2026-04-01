using System.Collections.Generic;
using UnityEngine;

public class UI_TaskIndicator : MonoBehaviour
{
    [Header("Indicator Pools (same size)")]
    [SerializeField] private int amountOfIndicators = 10;
    [SerializeField] private GameObject uiIndicatorPrefab;
    [SerializeField] private GameObject worldIndicatorPrefab;
    [SerializeField] private RectTransform[] uiIndicators;
    [SerializeField] private UI_OnObjectIndicator[] worldIndicators;

    [Header("References")]
    [SerializeField] private Camera cam;

    [Header("Edge Settings")]
    [Tooltip("0 = on the screen edge, higher = move inward")]
    [SerializeField] private float edgeDistance = 0f;

    // Internal mapping: target -> indicator index
    private readonly Dictionary<Transform, int> activeTargets = new();

    private void Awake()
    {
        if (cam == null)
            cam = Camera.main;

        uiIndicators = new RectTransform[amountOfIndicators];
        worldIndicators = new UI_OnObjectIndicator[amountOfIndicators];

        for (int i = 0; i < amountOfIndicators; i++)
        {
            // UI (screen edge)
            var uiObj = Instantiate(uiIndicatorPrefab, transform);
            var uiLogic = uiObj.GetComponent<UI_OnScreenIndicator>();

            uiIndicators[i] = uiLogic.GetComponent<RectTransform>();
            uiIndicators[i].gameObject.SetActive(false);

            // World (on object)
            var worldObj = Instantiate(worldIndicatorPrefab, transform);
            var worldLogic = worldObj.GetComponent<UI_OnObjectIndicator>();

            worldIndicators[i] = worldLogic;
            worldIndicators[i].EnableIndicator(false);
        }
    }


    private void Update()
    {
        foreach (var pair in activeTargets)
        {
            Transform target = pair.Key;
            int index = pair.Value;

            UpdateIndicator(
                uiIndicators[index],
                target,
                worldIndicators[index]
            );
        }
    }

    // =====================================================
    // PUBLIC API
    // =====================================================

    public void AddTarget(Transform newTarget)
    {
        Transform target = newTarget;

        if (target == null)
        {
            Debug.Log("No indicator target given;");
            return;
        }

        if (activeTargets.ContainsKey(target))
        {
            Debug.Log("Target is already highlited.");
            return;
        }

        int freeIndex = GetFreeIndicatorIndex();
        if (freeIndex == -1)
        {
            Debug.LogWarning("No free indicators available");
            return;
        }


        TutorialIndicator_Positioner indicatorPositioner = target.GetComponentInChildren<TutorialIndicator_Positioner>();

        if(indicatorPositioner != null)
            target = indicatorPositioner.transform;

        bool hasPositioner = indicatorPositioner != null;
            

        
        activeTargets.Add(target, freeIndex);
        worldIndicators[freeIndex].AttachTo(target,hasPositioner);
    }

    public void RemoveTarget(Transform target)
    {
        if (!activeTargets.TryGetValue(target, out int index))
            return;

        uiIndicators[index].gameObject.SetActive(false);
        worldIndicators[index].Detach(transform);

        activeTargets.Remove(target);
    }

    public void ClearAll()
    {
        foreach (var pair in activeTargets)
        {
            int index = pair.Value;
            uiIndicators[index].gameObject.SetActive(false);
            worldIndicators[index].Detach(transform);
        }

        activeTargets.Clear();
    }

    // =====================================================
    // INTERNAL HELPERS
    // =====================================================

    private int GetFreeIndicatorIndex()
    {
        for (int i = 0; i < uiIndicators.Length; i++)
        {
            if (!activeTargets.ContainsValue(i))
                return i;
        }
        return -1;
    }

    private void UpdateIndicator(RectTransform edgeIndicator, Transform target, UI_OnObjectIndicator worldIndicator)
    {
        if (edgeIndicator == null || target == null)
            return;

        // ---------- VISIBILITY ----------
        Vector3 vp = cam.WorldToViewportPoint(target.position);
        bool visible =
            vp.z > 0f &&
            vp.x > 0f && vp.x < 1f &&
            vp.y > 0f && vp.y < 1f;

        edgeIndicator.gameObject.SetActive(!visible);
        worldIndicator.EnableIndicator(visible);

        if (visible)
            return;

        // ---------- DIRECTION ----------
        Vector3 localDir = cam.transform.InverseTransformDirection(
            target.position - cam.transform.position);

        Vector2 dir = new(localDir.x, localDir.y);
        if (dir.sqrMagnitude < 0.0001f)
            return;

        dir.Normalize();

        // ---------- ROTATION ----------
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        edgeIndicator.rotation = Quaternion.Euler(0f, 0f, angle);

        // ---------- POSITION ----------
        Vector2 center = new(Screen.width * 0.5f, Screen.height * 0.5f);

        float halfW = edgeIndicator.rect.width * 0.5f;
        float halfH = edgeIndicator.rect.height * 0.5f;

        float maxX = Screen.width * 0.5f - edgeDistance - halfW;
        float maxY = Screen.height * 0.5f - edgeDistance - halfH;

        Vector2 pos = center + new Vector2(dir.x * maxX, dir.y * maxY);
        pos.x = Mathf.Clamp(pos.x, halfW, Screen.width - halfW);
        pos.y = Mathf.Clamp(pos.y, halfH, Screen.height - halfH);

        edgeIndicator.position = pos;
    }
}
