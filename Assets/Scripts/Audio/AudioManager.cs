using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    private Queue<AudioSource> pool = new();



    [SerializeField] private Vector2 pitchRange = new(0.95f, 1.05f);
    [SerializeField] private AudioLibrarySO library;
    [SerializeField] private AudioSource sourcePrefab;
    [SerializeField] private int poolSize = 10;
    private float defaultMinRange;
    private float defaultMaxRange;

    [Header("Music")]
[SerializeField] public float volumeChangeStep = 2f; // in dB
    [SerializeField] private AudioMixerGroup musicGroup;

    [SerializeField] private float musicFadeTime = 1f;

    private AudioSource musicSource;
    private Coroutine musicRoutine;

    [Header("Testing")]
    [SerializeField] private AudioClip currentTestClip;
    private Dictionary<string, int> testIndexes = new();
    private string lastTestId;
    private AudioClip lastTestClip;
    private Dictionary<string, List<AudioSource>> activeSources = new();
    private Dictionary<string, Coroutine> activeLoops = new();

    [SerializeField] private AudioMixer audioMixer;

    [Header("Priority Play")]
    [SerializeField] private AudioMixerGroup priorityGroup;
    [SerializeField] private string[] duckParams;
    [SerializeField] private float duckStrength = -7f;
    private int activePriorityCount = 0;


    private void Awake()
    {
        instance = this;


        defaultMinRange = sourcePrefab.GetComponent<AudioSource>().minDistance;
        defaultMaxRange = sourcePrefab.GetComponent<AudioSource>().maxDistance;


        for (int i = 0; i < poolSize; i++)
            pool.Enqueue(CreateSource());


        musicSource = CreateSource();
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.outputAudioMixerGroup = musicGroup;

        //PlayMusic("background_music_test");
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Y))
        //    RemoveLastTestClip();

        //if (Input.GetKeyDown(KeyCode.T))
        //    SaveLastTestClip();

        HandleVolumeInput();
    }

    void HandleVolumeInput()
    {
        float musicVol, sfxVol;

        // Get current volumes (in dB)
        audioMixer.GetFloat("Music", out musicVol);
        audioMixer.GetFloat("SFX", out sfxVol);

        // Adjust SFX
        if (Input.GetKeyDown(KeyCode.Alpha7)) sfxVol -= volumeChangeStep;
        if (Input.GetKeyDown(KeyCode.Alpha8)) sfxVol += volumeChangeStep;

        // Adjust Music
        if (Input.GetKeyDown(KeyCode.Alpha9)) musicVol -= volumeChangeStep;
        if (Input.GetKeyDown(KeyCode.Alpha0)) musicVol += volumeChangeStep;

        // Clamp so you don't go crazy (-80dB to 20dB is typical)
        musicVol = Mathf.Clamp(musicVol, -80f, 20f);
        sfxVol = Mathf.Clamp(sfxVol, -80f, 20f);

        // Apply back to mixer
        audioMixer.SetFloat("Music", musicVol);
        audioMixer.SetFloat("SFX", sfxVol);
    }

    private IEnumerator MusicLoopCo(AudioData data)
    {
        while (true)
        {
            var clip = data.GetNextClip();
            if (clip == null)
                yield break;

            yield return StartCoroutine(MusicFadeCo(clip, data.volume, false));

            // wait ONLY remaining time (clip already started)
            yield return new WaitForSeconds(clip.length - musicFadeTime);
        }
    }

    public void PlayMusic(string id)
    {
        var data = library.Get(id);
        if (data == null) 
            return;

        if (musicRoutine != null)
            StopCoroutine(musicRoutine);

        musicRoutine = StartCoroutine(MusicLoopCo(data));
    }

    public void StopMusic()
    {
        if (musicRoutine != null)
            StopCoroutine(musicRoutine);

        musicRoutine = StartCoroutine(MusicFadeCo(null, 0f, true));
    }
    private IEnumerator MusicFadeCo(AudioClip newClip, float targetVolume, bool stopOnly)
    {
        float startVol = musicSource.volume;

        // 🔻 Fade OUT
        float t = 0f;
        while (t < musicFadeTime)
        {
            t += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVol, 0f, t / musicFadeTime);
            yield return null;
        }

        musicSource.Stop();

        if (stopOnly)
            yield break;

        // 🔁 Switch clip
        musicSource.clip = newClip;
        musicSource.Play();

        // 🔺 Fade IN
        t = 0f;
        while (t < musicFadeTime)
        {
            t += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, targetVolume, t / musicFadeTime);
            yield return null;
        }

        musicSource.volume = targetVolume;
    }

    public void PlayPrioritySFX(string clipId, Vector3 position,  float fadeTime = 0.2f)
    {
        StartCoroutine(PrioritySfxCo(clipId, position, fadeTime));
    }

    private IEnumerator PrioritySfxCo(string id, Vector3 pos, float fadeTime)
    {
        var data = library.Get(id);
        if (data == null)
            yield break;

        bool isFirst = activePriorityCount == 0;
        activePriorityCount++;

        Dictionary<string, float> original = null;

        if (isFirst)
        {
            // cache ONLY once
            original = new Dictionary<string, float>();

            foreach (var param in duckParams)
            {
                if (audioMixer.GetFloat(param, out float value))
                    original[param] = value;
            }

            // build duck target
            var duckTarget = new Dictionary<string, float>();
            foreach (var kvp in original)
                duckTarget[kvp.Key] = duckStrength;

            yield return FadeMixer(original, duckTarget, fadeTime);
        }

        // 🔊 PLAY
        var clip = data.GetRandomClip();
        if (clip != null)
        {
            var src = pool.Count > 0 ? pool.Dequeue() : CreateSource();

            src.transform.position = pos;
            src.clip = clip;
            src.volume = data.volume <= 0 ? 1f : data.volume;
            src.pitch = 1f;
            src.spatialBlend = 0f;
            src.outputAudioMixerGroup = priorityGroup;

            src.Play();

            yield return new WaitForSeconds(clip.length);

            src.Stop();
            pool.Enqueue(src);
        }

        activePriorityCount--;

        // 🔼 restore ONLY when last one finishes
        if (activePriorityCount == 0 && original != null)
        {
            // get current
            var current = new Dictionary<string, float>();
            foreach (var kvp in original)
            {
                audioMixer.GetFloat(kvp.Key, out float val);
                current[kvp.Key] = val;
            }

            yield return FadeMixer(current, original, fadeTime + .3f);
        }
    }

    private void SaveLastTestClip()
    {
        if (string.IsNullOrEmpty(lastTestId) || lastTestClip == null)
            return;

        var data = library.Get(lastTestId);
        if (data == null)
            return;

        var list = new List<AudioClip>();

        if (data.savedClips != null)
            list.AddRange(data.savedClips);

        if (!list.Contains(lastTestClip))
            list.Add(lastTestClip);

        data.savedClips = list.ToArray();

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(library);
#endif

        Debug.Log("Saved clip: " + lastTestClip.name);
    }

    public void QueSFX(string id, Vector3 position, float delay)
    {
        StartCoroutine(QueSFXCo(id, position, delay));
    }

    private IEnumerator QueSFXCo(string id, Vector3 position, float delay)
    {
        yield return new WaitForSeconds(delay);
        PlaySFX(id, position);
    }

    public void PlaySFX(string id, Vector3 position, float durrationOverride = 0, Vector2? minMaxDistance = null)
    {
        var data = library.Get(id);

        if (data == null)
            return;

        if (data.isTesting)
        {
            PlayTestSound(id, position,data,durrationOverride,minMaxDistance);
            return;
        }

        PlayOneShot(id, position,data,durrationOverride,minMaxDistance);
    }



    public void StopSFX(string id)
    {
        if (!activeSources.ContainsKey(id))
            return;

        foreach (var src in activeSources[id])
        {
            if (src != null)
            {
                src.Stop();
                pool.Enqueue(src);
            }
        }

        activeSources[id].Clear();
    }
    public void PlayLoop(string id, Vector3 position)
    {
        if (activeLoops.ContainsKey(id))
            return;

        var data = library.Get(id);
        if (data == null)
            return;

        var routine = StartCoroutine(LoopCo(id, position, data));
        activeLoops[id] = routine;
    }


    private void RemoveLastTestClip()
    {
        if (string.IsNullOrEmpty(lastTestId) || lastTestClip == null)
            return;

        var data = library.Get(lastTestId);
        if (data == null || data.possibleClips == null || data.possibleClips.Length == 0)
            return;

        var list = new List<AudioClip>(data.possibleClips);

        int removedIndex = list.IndexOf(lastTestClip);
        if (removedIndex < 0)
            return;

        list.RemoveAt(removedIndex);

        data.possibleClips = list.ToArray();

        if (list.Count == 0)
            return;

        // keep same index so next clip shifts into that slot
        if (testIndexes.ContainsKey(lastTestId))
        {
            int currentIndex = testIndexes[lastTestId];

            // if we removed last element, clamp index
            if (currentIndex >= list.Count)
                currentIndex = 0;

            testIndexes[lastTestId] = currentIndex;
        }

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(library);
#endif

        Debug.Log("Removed clip: " + lastTestClip.name);

        lastTestClip = null;
    }


    public void PlayOneShot(string id, Vector3 position,AudioData data, float durrationOverride,Vector2? minMaxDistance = null)
    {
        var src = pool.Count > 0 ? pool.Dequeue() : CreateSource();

        src.transform.position = position;
        src.clip = data.GetRandomClip();
        src.volume = data.volume <= 0 ? 1 : data.volume;
        src.pitch = Random.Range(pitchRange.x, pitchRange.y);
        src.spatialBlend = 1f;

        src.minDistance = defaultMinRange;
        src.maxDistance = defaultMaxRange;


        if (minMaxDistance != null)
        {
            src.minDistance = minMaxDistance.Value.x;
            src.maxDistance = minMaxDistance.Value.y;
        }

        src.Play();
        RegisterActiveSource(id, src);

        StartCoroutine(ReturnToPool(src,durrationOverride));
    }

    public void PlayTestSound(string id, Vector3 position, AudioData data, float durrationOverride, Vector2? minMaxDistance = null)
    {
        library.RebuildCache();

        var clips = data.possibleClips != null && data.possibleClips.Length > 0
            ? data.possibleClips
            : new[] { data.GetRandomClip() };

        if (clips == null || clips.Length == 0)
            return;

        if (!testIndexes.ContainsKey(id))
            testIndexes[id] = 0;

        int index = testIndexes[id];
        var clip = clips[index];

        testIndexes[id] = (index + 1) % clips.Length;


        if (data.canSaveClip)
        {
            lastTestId = id;
            lastTestClip = clip;
        }

        var src = pool.Count > 0 ? pool.Dequeue() : CreateSource();

        src.transform.position = position;
        src.clip = clip;
        src.volume = data.volume <= 0 ? 1f : data.volume;
        src.pitch = 1f;
        src.spatialBlend = 1f;
        src.minDistance = defaultMinRange;
        src.maxDistance = defaultMaxRange;

        if (minMaxDistance != null)
        {
            src.minDistance = minMaxDistance.Value.x;
            src.maxDistance = minMaxDistance.Value.y;
        }

        src.Play();
        StartCoroutine(ReturnToPool(src,durrationOverride));
    }




    private void RegisterActiveSource(string id, AudioSource src)
    {
        if (!activeSources.ContainsKey(id))
            activeSources[id] = new List<AudioSource>();

        activeSources[id].Add(src);
    }
    private void UnregisterSource(AudioSource src)
    {
        foreach (var kvp in activeSources)
        {
            if (kvp.Value.Remove(src))
                break;
        }
    }



    private IEnumerator ReturnToPool(AudioSource src, float overrideDuration)
    {
        if (src.clip != null)
        {
            if (overrideDuration > 0f)
                yield return new WaitForSeconds(overrideDuration);
            else
                yield return new WaitForSeconds(src.clip.length);
        }

        src.Stop();
        UnregisterSource(src);
        pool.Enqueue(src);
    }

    private IEnumerator LoopCo(string id, Vector3 position, AudioData data)
    {
        float fadeTime = 1.5f;

        AudioSource a = pool.Count > 0 ? pool.Dequeue() : CreateSource();
        AudioSource b = pool.Count > 0 ? pool.Dequeue() : CreateSource();

        RegisterActiveSource(id, a);
        RegisterActiveSource(id, b);

        SetupSource(a, data, position);
        SetupSource(b, data, position);

        AudioSource current = a;
        AudioSource next = b;

        current.volume = data.volume;
        current.Play();

        while (true)
        {
            //float waitTime = current.clip.length - fadeTime;
            yield return new WaitForSeconds(fadeTime);

            next.clip = data.GetFirstClip();
            next.transform.position = position;
            next.volume = 0f;
            next.Play();

            float t = 0f;
            while (t < fadeTime)
            {
                t += Time.deltaTime;
                float lerp = t / fadeTime;

                current.volume = Mathf.Lerp(data.volume, 0f, lerp);
                next.volume = Mathf.Lerp(0f, data.volume, lerp);

                yield return null;
            }

            current.Stop();

            // swap
            var temp = current;
            current = next;
            next = temp;
        }
    }

    public void StopLoop(string id)
    {
        if (!activeLoops.ContainsKey(id))
            return;

        StopCoroutine(activeLoops[id]);
        activeLoops.Remove(id);

        StartCoroutine(FadeAllLoopSources(id, .1f));
    }

    private void SetupSource(AudioSource src, AudioData data, Vector3 position)
    {
        src.transform.position = position;
        src.clip = data.GetFirstClip();
        src.volume = data.volume <= 0 ? 1f : data.volume;
        src.pitch = Random.Range(pitchRange.x, pitchRange.y);
        src.spatialBlend = 1f;
        src.loop = false; // important for crossfade system
    }


    private IEnumerator FadeAllLoopSources(string id, float time)
    {
        if (!activeSources.ContainsKey(id))
            yield break;

        foreach (var src in activeSources[id])
        {
            float startVol = src.volume;
            float t = 0f;

            while (t < time)
            {
                t += Time.deltaTime;
                src.volume = Mathf.Lerp(startVol, 0f, t / time);
                yield return null;
            }

            src.Stop();
            pool.Enqueue(src);
        }

        activeSources[id].Clear();
    }

    private AudioSource CreateSource()
    {
        var src = Instantiate(sourcePrefab, transform);
        return src;
    }

    private IEnumerator FadeMixer(Dictionary<string, float> from, Dictionary<string, float> to, float time)
    {
        float t = 0f;

        while (t < time)
        {
            t += Time.deltaTime;
            float lerp = t / time;

            foreach (var kvp in from)
            {
                float start = kvp.Value;
                float target = to[kvp.Key];
                float value = Mathf.Lerp(start, target, lerp);
                audioMixer.SetFloat(kvp.Key, value);
            }

            yield return null;
        }

        // snap final
        foreach (var kvp in to)
            audioMixer.SetFloat(kvp.Key, kvp.Value);
    }
}
