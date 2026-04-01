using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class EmissionCycle : MonoBehaviour
{
    [Header("Emission Settings")]
    private Color emissionA;
    private Color emissionB;
    private float cycleDuration;

    [SerializeField] private bool pingPong = true;
    [SerializeField] private bool startCycleByDefault;
    private bool emissionCycleActive;

    private Material mat;
    private float timer;

    private void Start()
    {
        emissionA = ColorConfig.Get().emissionA;
        emissionB = ColorConfig.Get().emissionB;
        cycleDuration = ColorConfig.Get().cycleDuration;

        Renderer rend = GetComponent<Renderer>();

        // Create a unique material instance so we don't modify the shared one
        mat = new Material(rend.material);
        rend.material = mat;

        mat.EnableKeyword("_EMISSION");
        EnableEmissionCycle(startCycleByDefault);
    }

    private void Update()
    {
        if (emissionCycleActive == false)
            return;

        timer += Time.deltaTime;
        float t = timer / cycleDuration;

        if (pingPong)
            t = Mathf.PingPong(t, 1f);
        else
            t %= 1f;

        // Interpolate HDR colors directly (color + intensity are both encoded in HDR color)
        Color newEmission = Color.Lerp(emissionA, emissionB, t);
        mat.SetColor("_EmissionColor", newEmission);
    }

    public void EnableEmissionCycle(bool enable)
    {
        emissionCycleActive = enable;

        if(enable == false)
            mat.SetColor("_EmissionColor", emissionA);
    }

    //private void OnDestroy()
    //{
    //    if (Application.isPlaying && mat != null)
    //        Destroy(mat);
    //}
}
