using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio Data/Audio Library")]
public class AudioLibrarySO : ScriptableObject
{
    public List<AudioData> uiSounds;
    public List<AudioData> anvilSounds;
    public List<AudioData> scrollSounds;
    public List<AudioData> deliveryDoorSounds;
    public List<AudioData> coinStorageSounds;
    public List<AudioData> waterBarrelSounds;
    public List<AudioData> coinSounds;
    public List<AudioData> woodcutSounds;
    public List<AudioData> deliverySounds;
    public List<AudioData> coinCutSounds;
    public List<AudioData> furnaceSounds;
    public List<AudioData> broomSounds;
    public List<AudioData> torchSounds;

    public List<AudioData> defaultSounds; 
    public List<AudioData> musicSounds;

    private Dictionary<string, AudioData> cache;

    private void OnEnable()
    {
        RebuildCache();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        RebuildCache();
    }
#endif

    public void RebuildCache()
    {
        cache = new Dictionary<string, AudioData>();
        AddToCache(defaultSounds);
        AddToCache(coinSounds);
        AddToCache(woodcutSounds);
        AddToCache(deliverySounds);
        AddToCache(coinCutSounds);
        AddToCache(furnaceSounds);
        AddToCache(waterBarrelSounds);
        AddToCache(broomSounds);
        AddToCache(torchSounds);
        AddToCache(coinStorageSounds);
        AddToCache(deliveryDoorSounds);
        AddToCache(scrollSounds);
        AddToCache(anvilSounds);
        AddToCache(uiSounds);
        AddToCache(musicSounds);
    }

    private void AddToCache(List<AudioData> list)
    {
        if (list == null) return;

        foreach (var sound in list)
        {
            if (sound == null || string.IsNullOrEmpty(sound.id))
                continue;

            if (!cache.ContainsKey(sound.id))
                cache.Add(sound.id, sound);
            else
                Debug.LogWarning($"Duplicate audio id: {sound.id}");
        }
    }

    public AudioData Get(string id)
    {
        if (cache == null)
            RebuildCache();

        if (!cache.TryGetValue(id, out var data))
        {
            Debug.LogError($"Audio ID not found: {id}", this);
            return null;
        }

        return data;
    }

}

[System.Serializable]
public class AudioData
{
    public string id;
    public AudioClip[] clip;
    [Range(0f, 1f)] public float volume = 1f;
    public bool isTesting;
    public bool canSaveClip;
    public AudioClip[] possibleClips;
    public AudioClip[] savedClips;
    private int musicIndex;

    public AudioClip GetRandomClip()
    {
        return clip[Random.Range(0, clip.Length)];
    }

    public AudioClip GetFirstClip()
    {
        return clip[0];
    }


    public AudioClip GetNextClip()
    {
        var c = clip[musicIndex];
        musicIndex = (musicIndex + 1) % clip.Length;
        return c;
    }
}

