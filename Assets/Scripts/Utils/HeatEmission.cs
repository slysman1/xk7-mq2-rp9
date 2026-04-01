using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Alexdev.TweenUtils;

public class HeatEmission : MonoBehaviour
{
    private Color emissionBaseColor;
    private float hotIntensity;
    private float coldIntensity;
    private Material[] materialInstances;
    public Coroutine changeTempretureCo;
    public bool isHot { get; private set; }

    private void Awake()
    {
        emissionBaseColor = ColorConfig.Get().hotEmissionColor;
        hotIntensity = ColorConfig.Get().hotEmissionIntensity;
        coldIntensity = ColorConfig.Get().coldEmissionIntensity;

        List<Material> mats = new List<Material>();
        foreach (var r in GetComponentsInChildren<MeshRenderer>())
        {
            Material[] instancedMats = r.materials;
            r.materials = instancedMats;
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
        StartEmissionRoutine(coldIntensity, duration);
    }

    private void StartEmissionRoutine(float targetIntensity, float duration)
    {
        if (changeTempretureCo != null)
            StopCoroutine(changeTempretureCo);

        changeTempretureCo = StartCoroutine(EmissionRoutine(targetIntensity, duration));
    }

    private IEnumerator EmissionRoutine(float targetIntensity, float duration)
    {
        if (materialInstances == null || materialInstances.Length == 0)
            yield break;

        yield return StartCoroutine(EmissionTo(materialInstances, emissionBaseColor * targetIntensity, duration));
        isHot = targetIntensity > coldIntensity;
    }
}