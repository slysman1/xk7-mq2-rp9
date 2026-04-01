using UnityEngine;

public class Audio_EntityImpact : MonoBehaviour
{
    [SerializeField] private AudioClip[] impactClips;
    [SerializeField] private float minVelocity = 0.25f;
    [SerializeField] private float cooldown = 0.25f;
    [SerializeField] private Vector2 pitchRange = new(0.95f, 1.05f);
    [Range(0f, 1f)]
    [SerializeField] private float volume = .25f;

    private AudioSource audioSource;
    private float lastPlayTime;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Time.time - lastPlayTime < cooldown)
            return;

        float impactForce = collision.relativeVelocity.magnitude;

        if (impactForce < minVelocity)
            return;

        PlayImpact();
        lastPlayTime = Time.time;
    }

    private void PlayImpact()
    {
        if (impactClips.Length == 0) 
            return;

        audioSource.volume = volume;
        audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        audioSource.PlayOneShot(impactClips[Random.Range(0, impactClips.Length)]);

    }
}
