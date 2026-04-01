using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatEmission : MonoBehaviour
{
    private Color emissionBaseColor;

    private float hotIntensity;
    private float coldIntensit = 0.01f;

    private Material[] materialInstances;
    public Coroutine changeTempretureCo;
    public bool isHot { get; private set; }

    private void Awake()
    {
        var renderers = GetComponentsInChildren<MeshRenderer>();
        emissionBaseColor = ColorConfig.Get().hotEmissionColor;
        hotIntensity = ColorConfig.Get().hotEmissionIntensity;
        coldIntensit = ColorConfig.Get().coldEmissionIntensity;

        List<Material> mats = new List<Material>();

        foreach (var r in renderers)
        {
            Material[] instancedMats = r.materials;
            r.materials = instancedMats; // ensure unique instances
            mats.AddRange(instancedMats);
        }

        materialInstances = mats.ToArray();

    }

    public void TransitionToHot(float duration)
    {
        StartEmissionRoutine(hotIntensity, duration);
    }

    public void TransitionToCool(float duration)
    {
        StartEmissionRoutine(coldIntensit, duration);
    }

    private void StartEmissionRoutine(float targetIntensity, float duration)
    {
        if (changeTempretureCo != null)
            StopCoroutine(changeTempretureCo);

        changeTempretureCo = StartCoroutine(AnimateEmission(targetIntensity, duration));
    }

    private IEnumerator AnimateEmission(float targetIntensity, float duration)
    {
        if (materialInstances == null || materialInstances.Length == 0)
            yield break;

        float time = 0f;
        float startIntensity = materialInstances[0].GetColor("_EmissionColor").maxColorComponent;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);
            float intensity = Mathf.Lerp(startIntensity, targetIntensity, t);
            SetEmission(emissionBaseColor * intensity);
            yield return null;
        }

        SetEmission(emissionBaseColor * targetIntensity);
        isHot = targetIntensity > coldIntensit;
    }

    private void SetEmission(Color color)
    {
        foreach (var mat in materialInstances)
        {
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.SetColor("_EmissionColor", color);
                mat.EnableKeyword("_EMISSION");
            }
        }
    }
}
