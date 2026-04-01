using UnityEngine;
public static class Audio
{
    public static void PlaySFX(string id, Transform targetTransform, float durrationOverride = 0)
    {
        AudioManager.instance.PlaySFX(id, targetTransform.position,durrationOverride);
    }

    public static void PlaySFX(string id, Vector3 targetPosition, float durrationOverride = 0)
    {
        AudioManager.instance.PlaySFX(id, targetPosition,durrationOverride);
    }

    public static void QueSFX(string id, Transform target,float delay)
    {
        AudioManager.instance.QueSFX(id, target.position,delay);
    }

    public static void StopSFX(string id)
    {
        AudioManager.instance.StopSFX(id);
    }

    public static void LoopSFX(string id, Transform targetTransform)
    {
        AudioManager.instance.PlayLoop(id, targetTransform.position);
    }

    public static void StopLoopSFX(string id)
    {
        AudioManager.instance.StopLoop(id);
    }

    public static void PlaySFXFar(string id, Vector3 target,Vector2 minMaxDistance)
    {
        AudioManager.instance.PlaySFX(id, target, 0, minMaxDistance);
    }

    public static void PlayPrioritySFX(string id, Transform targetTransform)
    {
        AudioManager.instance.PlayPrioritySFX(id, targetTransform.position);
    }
}

