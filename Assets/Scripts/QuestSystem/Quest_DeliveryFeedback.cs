using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Alexdev.TweenUtils; // Assuming you have a TweenUtils class for Lerp functionality

public class Quest_DeliveryFeedback : MonoBehaviour
{

    private ParticleSystem[] flameParticles;

    [Header("Feedback colours")]
    [SerializeField] private Transform fireToScale;
    [Range(0f, 1f)]
    [SerializeField] private float feedbackScaleMultiplier = .2f;
    [SerializeField] private float initialFeedbackDur = .1f;
    [SerializeField] private Color rejectDeliveryColor = Color.red;   // “TurnFireRed”
    [SerializeField] private Color acceptDeliveryColor = Color.blue; // “TurnFireBlue”
    private float feedbackScale => originalScale * (1 + feedbackScaleMultiplier);
    private float originalScale = .1f;

    // ──────────────────────────────────────────────────────────────────────────────
    private Color[] originalColours;          // cached start-colour per particle system
    private Coroutine currentRoutine;
    private Dictionary<ParticleSystem, bool> colourLifetimeStates = new();
    private Dictionary<ParticleSystem, ParticleSystem.Particle[]> particleBuffers
    = new Dictionary<ParticleSystem, ParticleSystem.Particle[]>();


    private void Awake()
    {
        flameParticles = fireToScale.GetComponentsInChildren<ParticleSystem>();
        originalColours = new Color[flameParticles.Length];
        originalScale = fireToScale.transform.localScale.x;

        SetSimulationSpeed(.15f);
        CacheColorChangeOvertime();
        CacheColors();

        // Allocate reusable particle buffers
        foreach (var ps in flameParticles)
        {
            int max = ps.main.maxParticles;
            if (max > 0)
                particleBuffers[ps] = new ParticleSystem.Particle[max];
        }
    }



    public void ShowDeliveryFeedback(bool correct, float duration)
    {
        Color color = correct ? acceptDeliveryColor : rejectDeliveryColor;
        StartFeedback(color, duration);
    }

    private void StartFeedback(Color target, float feedbackDuration)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(FeedbackRoutine(target, feedbackDuration));
    }

    private IEnumerator FeedbackRoutine(Color target, float feedbackDuration)
    {
        EnableClorLifeTimeForParticles(false);

        SetSimulationSpeed(.5f);
        StartCoroutine(ScaleLocal(fireToScale, Vector3.one * feedbackScale, initialFeedbackDur));
        StartCoroutine(LerpColourSet(originalColours, ExpandColour(target), initialFeedbackDur));

        yield return new WaitForSeconds(feedbackDuration);

        StartCoroutine(ScaleLocal(fireToScale, Vector3.one * originalScale, feedbackDuration * .5f));
        StartCoroutine(LerpColourSet(ExpandColour(target), originalColours, feedbackDuration * .5f));


        EnableClorLifeTimeForParticles(true);
        SetSimulationSpeed(.15f);
        currentRoutine = null;
    }

    private void EnableClorLifeTimeForParticles(bool enable)
    {
        if (enable == false)
        {
            foreach (var ps in flameParticles)
            {
                var col = ps.colorOverLifetime;
                col.enabled = false;
            }
        }
        else
        {
            foreach (var kvp in colourLifetimeStates)
            {
                var col = kvp.Key.colorOverLifetime;
                col.enabled = kvp.Value;
            }
        }
    }



    // Lerps every system’s start-colour from A → B over <time>
    // Lerps every system’s colour from A → B over <time>, including already alive particles
    private IEnumerator LerpColourSet(Color[] from, Color[] to, float time)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / time;

            for (int i = 0; i < flameParticles.Length; i++)
            {
                Color step = Color.LerpUnclamped(from[i], to[i], t);

                // Update startColor (future particles)
                SetStartColour(flameParticles[i], step);

                // Update alive particles (reuse buffer)
                ApplyColorToAliveParticles(flameParticles[i], step);
            }

            yield return null;
        }
    }


    /// <summary>
    /// Recolors all currently alive particles in a system.
    /// </summary>
    private void ApplyColorToAliveParticles(ParticleSystem ps, Color c)
    {
        if (!particleBuffers.TryGetValue(ps, out var buffer))
            return;

        int count = ps.GetParticles(buffer);

        for (int i = 0; i < count; i++)
            buffer[i].startColor = c;

        ps.SetParticles(buffer, count);
    }




    private Color[] ExpandColour(Color c)
    {
        Color[] arr = new Color[flameParticles.Length];
        for (int i = 0; i < arr.Length; i++)
            arr[i] = c;
        return arr;
    }



    private static void SetStartColour(ParticleSystem ps, Color c)
    {
        var main = ps.main;                // struct copy
        main.startColor = c;               // apply
    }

    private void CacheColors()
    {
        // Cache each system’s original start colour
        for (int i = 0; i < flameParticles.Length; i++)
            originalColours[i] = flameParticles[i].main.startColor.color;
    }

    private void CacheColorChangeOvertime()
    {
        for (int i = 0; i < flameParticles.Length; i++)
        {
            originalColours[i] = flameParticles[i].main.startColor.color;

            // Save whether Color over Lifetime was enabled
            var col = flameParticles[i].colorOverLifetime;
            colourLifetimeStates[flameParticles[i]] = col.enabled;
        }
    }

    private void SetSimulationSpeed(float speed)
    {
        if (flameParticles.Length == 0) return;

        var main = flameParticles[0].main;
        main.simulationSpeed = speed;
    }
}
