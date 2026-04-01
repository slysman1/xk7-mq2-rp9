using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CoinTemplate_TemperPoint : FocusPoint
{
    [Header("Target")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Transform targetIndicator;
    [SerializeField] private float indicatorRotSpeed = 2f;

    [Header("Glow Settings")]
    [SerializeField] private float minPower = 0.2f;
    [SerializeField] private float maxPower = 1.0f;
    [SerializeField] private float speed = 2f;

    [Header("Runes")]
    [SerializeField] private Texture2D[] possibleRunes;
    [SerializeField] private Renderer[] rendereres;

    [SerializeField] private List<Material> mats = new ();


    private int edgeFadeID;
    private Coroutine glowRoutine;
    private Dictionary<GameObject, int> originalLayers = new Dictionary<GameObject, int>();
    private string cameraLayer = "ItemInHand";


    private void Awake()
    {
        CacheOriginalLayers();
        edgeFadeID = Shader.PropertyToID("_EdgeFadePow");

        foreach(var rend in rendereres)
            mats.Add(rend.material);

        AssignRandomRune();
        //ShowRune();
        StartRuneGlow();
    }

    private void AssignRandomRune()
    {
        if (possibleRunes == null || possibleRunes.Length == 0)
            return;

        Texture2D chosen = possibleRunes[Random.Range(0, possibleRunes.Length)];

        if (chosen == null)
            return;

        foreach(var mat in mats)
            mat.SetTexture("_MainTex", chosen);
    }


    public void ShowRune(bool enable)
    {
        if(targetIndicator != null)
            targetIndicator.gameObject.SetActive(enable);

        if(enable)
        {
            StopRuneGlow();
            StartCoroutine(SpinForever(new Vector3(0, 1, 0)));

            foreach(var mat in mats)
                mat.SetFloat(edgeFadeID, 0);
        }
        else
        {
            StopAllCoroutines();
            StartRuneGlow();

            foreach (var mat in mats)
                mat.SetFloat(edgeFadeID, maxPower * 10);
        }

    }

    public void StartRuneGlow()
    {
        if (glowRoutine != null)
            StopCoroutine(glowRoutine);

        glowRoutine = StartCoroutine(GlowRoutine());
    }

    public void StopRuneGlow()
    {
        if (glowRoutine != null)
        {
            StopCoroutine(glowRoutine);
            glowRoutine = null;
        }

        foreach (var mat in mats)
            mat.SetFloat(edgeFadeID, minPower);
    }

    private IEnumerator GlowRoutine()
    {
        while (true)
        {
            float t = (Mathf.Sin(Time.time * speed) + 1f) * 0.5f;
            float value = Mathf.Lerp(minPower, maxPower, t);

            foreach (var mat in mats)
                mat.SetFloat(edgeFadeID, value);

            yield return null;

        }
    }

    private IEnumerator SpinForever(Vector3 axis)
    {
        if (targetIndicator == null)
            yield break;

        while (true)
        {
            // Rotate around axis at "speed" degrees per second
            targetIndicator.Rotate(axis, indicatorRotSpeed * Time.deltaTime, Space.Self);
            yield return null; // wait for next frame
        }
    }


    public void EnableCamPriority(bool enable)
    {
        if (enable)
        {
            foreach (var kvp in originalLayers)
                kvp.Key.layer = LayerMask.NameToLayer(cameraLayer);
        }
        else
        {
            foreach (var kvp in originalLayers)
                kvp.Key.layer = kvp.Value; // restore saved
        }
    }
    private void CacheOriginalLayers()
    {
        originalLayers.Clear();

        // include self + all children
        foreach (Transform t in GetComponentsInChildren<Transform>(true))
        {
            if (!originalLayers.ContainsKey(t.gameObject))
                originalLayers.Add(t.gameObject, t.gameObject.layer);
        }
    }

}
