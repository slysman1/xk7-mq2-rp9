/**
 * Copyright Kyle Banks - All Rights Reserved
 * 
 * This source code is protected under international copyright law. 
 * All rights reserved and protected by the copyright holders.
 * This file is confidential and only available to authorized individuals with the
 * permission of the copyright holders. If you encounter this file and do not have
 * permission, please contact the copyright holders and delete this file.
 * 
 * Written by Kyle Banks <kyle@kwbanks.com>, 2023 - 2025
 */
using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;


[Serializable]
public class VolumetricLightingSettings
{

    private static readonly int SHADER_PARAM_SCATTERING = Shader.PropertyToID("_Scattering");
    private static readonly int SHADER_PARAM_SCATTERING_POWER = Shader.PropertyToID("_ScatteringPower");
    private static readonly int SHADER_PARAM_MAX_STEPS = Shader.PropertyToID("_MaxSteps");
    private static readonly int SHADER_PARAM_JITTER = Shader.PropertyToID("_Jitter");
    private static readonly int SHADER_PARAM_MAX_DISTANCE = Shader.PropertyToID("_MaxDistance");
    private static readonly int SHADER_PARAM_LIGHT_COLOR = Shader.PropertyToID("_LightColor");
    private static readonly int SHADER_PARAM_LIGHT_INTENSITY = Shader.PropertyToID("_LightIntensity");
    private static readonly int SHADER_PARAM_UPSAMPLE_BLEND = Shader.PropertyToID("_UpsampleBlend");
    private static readonly int SHADER_PARAM_GLOBAL_INTENSITY = Shader.PropertyToID("_GlobalIntensity");

    [Serializable]
    public class RaymarchSettings
    {
        public float Scattering = 0.92f;
        public float ScatteringPower = 1f;
        [Range(5, 500)] public int MaxSteps = 25;
        public float MaxDistance = 75;
        public float Jitter = 250;
    }

    [Serializable]
    public class BlurSettings
    {
        [Range(1, 8)] public int DownSampling = 1;
        [Range(1, 12)] public int Iterations = 4;
        [Range(0f, 1f)] public float Blend = 0.5f;
    }

    [Serializable]
    public class CompositeSettings
    {
        public float LightIntensity = 1;
    }

    [Header("Debugging")]
    public bool DebugRaymarchBuffer;
    public bool ExecuteInSceneView;

    [Header("Pipeline")]
    public RenderPassEvent RenderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    public RenderTextureFormat TempTextureFormat = RenderTextureFormat.R16;

    [Header("Shading")]
    public Material Material;
    public RaymarchSettings Raymarch;
    public BlurSettings Blur;
    public CompositeSettings Composite;

    [Header("Runtime")]
    [Range(0f, 2f)]
    public float GlobalIntensity;
    [ColorUsage(true, true)]
    public Color LightColor;

    public float GetIntensity()
        => this.GlobalIntensity;//.Value;

    public void Apply()
    {
        // raymarch
        this.Material.SetFloat(SHADER_PARAM_SCATTERING_POWER, this.Raymarch.ScatteringPower);
        this.Material.SetFloat(SHADER_PARAM_SCATTERING, this.Raymarch.Scattering);
        this.Material.SetFloat(SHADER_PARAM_MAX_STEPS, this.Raymarch.MaxSteps);
        this.Material.SetFloat(SHADER_PARAM_JITTER, this.Raymarch.Jitter);
        this.Material.SetFloat(SHADER_PARAM_MAX_DISTANCE, this.Raymarch.MaxDistance);

        // Blur
        this.Material.SetFloat(SHADER_PARAM_UPSAMPLE_BLEND, this.Blur.Blend);

        // composite
        this.Material.SetFloat(SHADER_PARAM_LIGHT_INTENSITY, this.Composite.LightIntensity);

        // runtime
        this.Material.SetFloat(SHADER_PARAM_GLOBAL_INTENSITY, this.GetIntensity());
        this.Material.SetColor(SHADER_PARAM_LIGHT_COLOR, this.LightColor);
    }

    public void SetBlurSettings(BlurSettings settings)
        => this.Blur = settings;

}
