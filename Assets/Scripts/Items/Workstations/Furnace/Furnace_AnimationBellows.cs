using System;
using System.Collections;
using UnityEngine;

public class Furnace_AnimationBellows : MonoBehaviour
{
    private SkinnedMeshRenderer skinnedMeshRenderer;



    [SerializeField][Tooltip("Used to speed up bellows when we speed up production of furnace")]
    private float bellowSpeedMultiplier = 1.5f;
    [SerializeField] private Transform soundSource;
    public float bellowSpeed = 1f;
    public float bellowAnimDuration = 4f;
    public float belowDownSpeed;

    private int blendShapeIndex = 0;
    private Coroutine bellowsRoutine;

    private float baseSpeed;

    protected void Awake()
    {
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        baseSpeed = bellowSpeed;
        belowDownSpeed = bellowSpeed;
    }
    public void SpeedUpAnimation(bool speedUp)
    {
        //StopAllCoroutines();
        float multiplier = speedUp ? bellowSpeedMultiplier : 1;
        StartCoroutine(SmoothSpeed(baseSpeed * multiplier));
    }

    private IEnumerator SmoothSpeed(float target)
    {
        float start = bellowSpeed;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * 3f;
            belowDownSpeed = Mathf.Lerp(start, target, t);
            yield return null;
        }
    }
    public void StartBellows()
    {
        if (bellowsRoutine == null)
            bellowsRoutine = StartCoroutine(BellowsCo());
    }

    public void StopBellows()
    {
        if (bellowsRoutine != null)
        {
            StopCoroutine(bellowsRoutine);
            bellowsRoutine = null;
        }

        if (blendShapeIndex >= 0)
            StartCoroutine(ResetBellowsCo());
    }

    private IEnumerator BellowsCo()
    {
        float weight = 0f;
        bool goingDown = true;

        while (true)
        {
            if (goingDown)
            {
                weight -= Time.deltaTime * bellowSpeed * 100f;
                if (weight <= 0f)
                {
                    weight = 0f;
                    goingDown = false;
                    Audio.QueSFX("bellows_blow", soundSource,.1f);
                }
            }
            else
            {
                weight += Time.deltaTime * belowDownSpeed * 100f;
                if (weight >= 100f)
                {
                    weight = 100f;
                    goingDown = true;
                    Audio.PlaySFX("bellows_reset", soundSource,1.5f);
                }
            }

            skinnedMeshRenderer.SetBlendShapeWeight(blendShapeIndex, weight);
            yield return null;
        }
    }
    private IEnumerator ResetBellowsCo()
    {
        float currentWeight = skinnedMeshRenderer.GetBlendShapeWeight(blendShapeIndex);     

        while (currentWeight > 0f)
        {
            currentWeight -= Time.deltaTime * bellowSpeed * 100f;
            if (currentWeight < 0f) currentWeight = 0f;

            skinnedMeshRenderer.SetBlendShapeWeight(blendShapeIndex, currentWeight);
            yield return null;
        }
    }
}
