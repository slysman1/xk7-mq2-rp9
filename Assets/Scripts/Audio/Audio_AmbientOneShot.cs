using System.Collections;
using UnityEngine;

public class Audio_AmbientOneShot : MonoBehaviour
{
    private AudioSource audioSource;
    private Coroutine movingCo;

    [SerializeField] private bool movingSound;
    [SerializeField] private float soundDuration;
    [SerializeField] private float fadeTime = 0.5f;
    [SerializeField] private Transform posA;
    [SerializeField] private Transform posB;
    private float targetVolume;


    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        posA.transform.parent = transform.parent;
        posB.transform.parent = transform.parent;

    }

    public void PlayOneShot()
    {
        targetVolume = audioSource.volume;
        audioSource.volume = 0;
        audioSource.Play();
        StartCoroutine(FadeVolumeCo(targetVolume));

        if (movingSound == false)
            return;

        transform.position = posA.position;


        if (movingCo != null)
            StopCoroutine(movingCo);

        movingCo = StartCoroutine(MoveCo());
    }


    private IEnumerator MoveCo()
    {
        float timer = 0f;

        while (timer < soundDuration)
        {
            timer += Time.deltaTime;
            float t = timer / soundDuration;

            transform.position = Vector3.Lerp(posA.position, posB.position, t);

            yield return null;
        }

        
        StartCoroutine(FadeVolumeCo(0));
    }

    private IEnumerator FadeVolumeCo(float targetVolume)
    {
        float startVolume = audioSource.volume;
        float timer = 0f;

        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            float t = timer / fadeTime;

            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }

        audioSource.volume = targetVolume;

        if (targetVolume == 0)
        {
            audioSource.Stop();
            audioSource.volume = this.targetVolume;
        }
    }

}


