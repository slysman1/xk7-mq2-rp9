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

using UnityEngine.Rendering.Universal;


public class VolumetricLightingFeature : ScriptableRendererFeature
{

    public VolumetricLightingSettings Settings = new();

    private VolumetricLightingPass _pass;

    public override void Create()
        => this._pass = new VolumetricLightingPass(this.name, this.Settings);

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData data)
    {
        if (!VoluemtricLightHelpers.IsMainCamera(ref data) && (!this.Settings.ExecuteInSceneView || !VoluemtricLightHelpers.IsSceneCamera(ref data)))
            return;

        if (this.Settings.GetIntensity() < 0.01f)
            return;

        renderer.EnqueuePass(this._pass);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData data)
    {
#if !UNITY_6000_0_OR_NEWER
            this._pass.SetTarget(renderer.cameraColorTargetHandle);
#endif
    }

    protected override void Dispose(bool disposing)
        => this._pass?.Dispose();

    public void SetBlurSettings(VolumetricLightingSettings.BlurSettings blur)
        => this.Settings.SetBlurSettings(blur);


}