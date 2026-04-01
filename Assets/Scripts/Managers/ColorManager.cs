using System.Collections.Generic;
using UnityEngine;

public class ColorManager : MonoBehaviour
{
    public static ColorManager instance;

    [Header("Outline Settings")]
    [Range(.5f,10f)]
    public float outlineWith = 2f;
    public Color warningColor;
    public Color highlightColor;

    [Header("Emission Settings")]
    [ColorUsage(true, true)]
    public Color hotEmissionColor = Color.yellow;
    [Range(1f,5f)]
    public float hotEmissionIntensity = 2.5f;

    [Header("Focus Emission Settings")]
    [ColorUsage(true, true)] public Color emissionA = Color.black;
    [ColorUsage(true, true)] public Color emissionB = Color.white;
    public float cycleDuration = 2f;

    private void Awake()
    {
        instance = this;
    }
}
