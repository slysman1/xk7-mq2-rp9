using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatHandler : MonoBehaviour
{


    private Color emissionBaseColor => ColorManager.instance.hotEmissionColor;

    private float hotIntensity => ColorManager.instance.hotEmissionIntensity;
    private float coldIntensit = 0.01f;

    private Material[] materialInstances;
    private Item_Base item;
    public Coroutine changeTempretureCo;
    public bool isHot { get; private set; }
    public bool hotAtStart;

    private void Awake()
    {
        item = GetComponentInParent<Item_Base>();
        var renderers = GetComponentsInChildren<MeshRenderer>();

        List<Material> mats = new List<Material>();

        foreach (var r in renderers)
        {
            Material[] instancedMats = r.materials;
            r.materials = instancedMats; // ensure unique instances
            mats.AddRange(instancedMats);
        }

        materialInstances = mats.ToArray();

    }

    private void Start()
    {
        if (hotAtStart)
            TransitionToHot(.1f);
    }

    [ContextMenu("Make Hot")]
    public void MakeHot()
    {
        TransitionToHot(.1f);
    }
    public void TransitionToHot(float duration)
    {
        item.SetCanPickUpTo(false);
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
        isHot = targetIntensity > .01f ? true : false; 
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
