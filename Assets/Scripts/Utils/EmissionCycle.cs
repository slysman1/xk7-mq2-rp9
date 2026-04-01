using System.Collections;
using UnityEngine;
using static Alexdev.TweenUtils;

[RequireComponent(typeof(Renderer))]
public class EmissionCycle : MonoBehaviour
{
    private Color emissionA;
    private Color emissionB;
    private float cycleDuration;
    [SerializeField] private bool pingPong = true;
    [SerializeField] private bool startCycleByDefault;
    private Material mat;
    private Coroutine cycleCoroutine;
    private Coroutine transitionCo;

    private void Start()
    {
        emissionA = ColorConfig.Get().emissionA;
        emissionB = ColorConfig.Get().emissionB;
        cycleDuration = ColorConfig.Get().cycleDuration;
        Renderer rend = GetComponent<Renderer>();
        mat = new Material(rend.material);
        rend.material = mat;
        mat.EnableKeyword("_EMISSION");
        EnableEmissionCycle(startCycleByDefault);
    }

    public void EnableEmissionCycle(bool enable)
    {
        if (enable)
        {
            if (cycleCoroutine != null)
                StopCoroutine(cycleCoroutine);
            cycleCoroutine = StartCoroutine(CycleCo());
        }
        else
        {
            if (cycleCoroutine != null)
                StopCoroutine(cycleCoroutine);
            cycleCoroutine = null;
            mat.SetColor("_EmissionColor", emissionA);
        }
    }

    private IEnumerator CycleCo()
    {
        float timer = 0f;
        while (true)
        {
            timer += Time.deltaTime;
            float t = timer / cycleDuration;
            t = pingPong ? Mathf.PingPong(t, 1f) : t % 1f;
            mat.SetColor("_EmissionColor", Color.Lerp(emissionA, emissionB, t));
            yield return null;
        }
    }

    public void TransitionToPeak(float duration = 0.2f)
    {
        if (transitionCo != null)
            StopCoroutine(transitionCo);

        if (cycleCoroutine != null)
            StopCoroutine(cycleCoroutine);
        cycleCoroutine = null;

        transitionCo = StartCoroutine(EmissionTo(mat, emissionB, duration));
    }
}