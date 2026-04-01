using UnityEngine;

[CreateAssetMenu(fileName = "ColorConfig", menuName = "Config/ColorConfig")]

public class ColorConfigSO : ScriptableObject
{
    [Header("Outline Settings")]
    [Range(.5f, 10f)] public float outlineWidth = 2f;
    public Color outlineColor;

    [Header("Emission Settings")]
    [ColorUsage(true, true)] public Color hotEmissionColor = Color.yellow;
    [Range(1f, 5f)] public float hotEmissionIntensity = 2.5f;
    [Range(0f, .1f)] public float coldEmissionIntensity = .01f;

    [Header("Focus Emission Settings")]
    [ColorUsage(true, true)] public Color emissionA = Color.black;
    [ColorUsage(true, true)] public Color emissionB = Color.white;
    public float cycleDuration = 2f;
}