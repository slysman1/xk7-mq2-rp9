using System.Collections;
using UnityEngine;

public class Audio_NoiseLooper : MonoBehaviour
{

    [Header("Sources")]
    [SerializeField] private AudioSource sourceA;
    [SerializeField] private AudioSource sourceB;

    [Header("Settings")]
    [SerializeField, Range(0f, 1f)] private float volume = 1f;
    [SerializeField, Range(0f, 1.5f)] private float pitch = 1f;
    [SerializeField] private float fadeDuration = 0.1f;
    [SerializeField] private AudioClip whiteNoiseClip;

    private AudioSource current;
    private AudioSource next;

    private void Start()
    {
        ApplyVolumeImmediate();

        current = sourceA;
        next = sourceB;

        current.Play();
        StartCoroutine(LoopRoutine());
    }

    private IEnumerator LoopRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(current.clip.length - fadeDuration);

            next.volume = 0f;
            next.Play();

            StartCoroutine(Fade(current, 0f));
            StartCoroutine(Fade(next, volume));

            SwapSources();
        }
    }

    private IEnumerator Fade(AudioSource source, float target)
    {
        float start = source.volume;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            source.volume = Mathf.Lerp(start, target, time / fadeDuration);
            yield return null;
        }

        source.volume = target;

        if (target == 0f)
            source.Stop();
    }

    private void SwapSources()
    {
        var temp = current;
        current = next;
        next = temp;
    }

    private void ApplyVolumeImmediate()
    {
        if (sourceA)
        {
            sourceA.volume = volume;
            sourceA.pitch = pitch;

            if(sourceA.clip == null)
                sourceA.clip = whiteNoiseClip;
        }

        if (sourceB)
        {
            sourceB.volume = volume;
            sourceB.pitch = pitch;

            if(sourceB.clip == null)
                sourceB.clip = whiteNoiseClip;
        }


    }

    private void OnValidate()
    {
        ApplyVolumeImmediate();
    }
}
