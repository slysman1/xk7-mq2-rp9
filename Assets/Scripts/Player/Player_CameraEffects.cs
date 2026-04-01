using UnityEngine;
using System.Collections;

public class Player_CameraEffects : MonoBehaviour
{
    private Transform cameraTransform;
    private Player_MoveAndAim playerMoveAndAim;

    [Header("Breathing Settings")]
    [Range(0, 0.05f)]
    [SerializeField] private float amplitude = 0.02f;
    [SerializeField] private float frequency = 1.5f;
    [SerializeField] private float smoothSpeed = 5f;

    [Header("Shake Settings")]
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private Vector3 shakeStrength = new Vector3(0.15f, 0.15f, 0f);
    [SerializeField] private AnimationCurve shakeCurve = AnimationCurve.Linear(0, 1, 1, 0);

    private Vector3 defaultLocalPos;
    private float timeCounter;
    private Coroutine shakeRoutine;

    void Awake()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        if (playerMoveAndAim == null)
            playerMoveAndAim = GetComponentInParent<Player_MoveAndAim>();

        defaultLocalPos = cameraTransform.localPosition;
    }

    void Update()
    {
        if (shakeRoutine == null)
            BreathWhenIdle();
    }

    private void BreathWhenIdle()
    {
        if (!playerMoveAndAim.IsLooking)
        {
            timeCounter += Time.deltaTime * frequency;

            float offsetY = Mathf.Sin(timeCounter) * amplitude;
            Vector3 targetPos = defaultLocalPos + new Vector3(0, offsetY, 0);

            cameraTransform.localPosition = Vector3.Lerp(
                cameraTransform.localPosition,
                targetPos,
                Time.deltaTime * smoothSpeed
            );
        }
        else
        {
            cameraTransform.localPosition = Vector3.Lerp(
                cameraTransform.localPosition,
                defaultLocalPos,
                Time.deltaTime * smoothSpeed
            );
            timeCounter = 0f;
        }
    }


    // === Public API ===
    public void Shake()
    {
        StartShake(shakeDuration, shakeStrength);
    }

    public void Shake(float duration, Vector3 strength)
    {
        StartShake(duration, strength);
    }


    private void StartShake(float duration, Vector3 strength)
    {
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(ShakeRoutine(duration, strength));
    }

    private IEnumerator ShakeRoutine(float duration, Vector3 strength)
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float delta = Mathf.Clamp01(timer / duration);

            Vector3 randomVec = new Vector3(
                (Random.value - 0.5f) * 2f,
                (Random.value - 0.5f) * 2f,
                (Random.value - 0.5f) * 2f
            );

            Vector3 offset = Vector3.Scale(randomVec, strength) * shakeCurve.Evaluate(delta);

            cameraTransform.localPosition = defaultLocalPos + offset;

            yield return null;
        }

        // reset after shake
        cameraTransform.localPosition = defaultLocalPos;
        shakeRoutine = null;
    }

    

}
