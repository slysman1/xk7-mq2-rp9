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
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public static class VoluemtricLightHelpers
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsMainCamera(ref RenderingData data)
        => data.cameraData.cameraType == CameraType.Game && data.cameraData.camera.CompareTag("MainCamera");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSceneCamera(ref RenderingData data)
        => data.cameraData.cameraType == CameraType.SceneView;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSecondaryCamera(ref RenderingData data)
        => data.cameraData.cameraType == CameraType.Game && !IsMainCamera(ref data);

    public static void ConfigureInput(ScriptableRenderPass pass, ScriptableRenderPassInput input)
    {
        // Adapted from FullScreenPassRendererFeature.cs in the core URP library.            
        ScriptableRenderPassInput modifiedRequirements = input;
        bool requiresColor = (modifiedRequirements & ScriptableRenderPassInput.Color) != 0;
        bool injectedBeforeTransparent = pass.renderPassEvent <= RenderPassEvent.BeforeRenderingTransparents;
        if (requiresColor && !injectedBeforeTransparent)
            modifiedRequirements ^= ScriptableRenderPassInput.Color;
        pass.ConfigureInput(modifiedRequirements);
    }
}
