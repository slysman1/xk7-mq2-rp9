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
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#if UNITY_6000_0_OR_NEWER
using UnityEngine.Rendering.RenderGraphModule;
#endif


public class VolumetricLightingPass : AbstractRendererPass
{

    private static readonly int SHADER_PARAM_RAYMARCH_TEXTURE = Shader.PropertyToID("_RaymarchTexture");
    private static readonly int SHADER_PARAM_SOURCE_TEX_LOW_MIP = Shader.PropertyToID("_SourceTexLowMip");

#if UNITY_6000_0_OR_NEWER
    // Avoid string allocations
    private static readonly string[] MIP_DOWN_NAMES =
    {
            "_VolumetricLightingMipDown0", "_VolumetricLightingMipDown1",
            "_VolumetricLightingMipDown2", "_VolumetricLightingMipDown3",
            "_VolumetricLightingMipDown4", "_VolumetricLightingMipDown5",
            "_VolumetricLightingMipDown6", "_VolumetricLightingMipDown7",
            "_VolumetricLightingMipDown8", "_VolumetricLightingMipDown9",
            "_VolumetricLightingMipDown10", "_VolumetricLightingMipDown11"
        };

    private static readonly string[] MIP_UP_NAMES =
    {
            "_VolumetricLightingMipUp0", "_VolumetricLightingMipUp1",
            "_VolumetricLightingMipUp2", "_VolumetricLightingMipUp3",
            "_VolumetricLightingMipUp4", "_VolumetricLightingMipUp5",
            "_VolumetricLightingMipUp6", "_VolumetricLightingMipUp7",
            "_VolumetricLightingMipUp8", "_VolumetricLightingMipUp9",
            "_VolumetricLightingMipUp10", "_VolumetricLightingMipUp11"
        };

    private class PassData
    {
        internal VolumetricLightingSettings _settings;
        internal int _materialIndex;

        internal TextureHandle _source;
        internal TextureHandle _target;
        internal TextureHandle _raymarchBufferTexture;
        internal TextureHandle _lowMip;
    }
#endif

    private readonly VolumetricLightingSettings _settings;

#if UNITY_6000_0_OR_NEWER
    private TextureHandle[] _downHandles;
    private TextureHandle[] _upHandles;
#endif

    public VolumetricLightingPass(string name, VolumetricLightingSettings settings)
        : base(name)
    {
        this._settings = settings;
        this.renderPassEvent = settings.RenderPassEvent;

#if UNITY_6000_0_OR_NEWER
        requiresIntermediateTexture = false;
#endif

        // NOTE: logically depth should be needed, but it doesn't seem to make an impact. 
        // HOWEVER, when depth is enabled and the SSAO pass gets disabled (as on Medium quality), it breaks 
        // the depth buffer for some reason so Volumetric Fog and water shaders break. Who knows why...
        //
        // ScriptableRenderPassInput modifiedRequirements = ScriptableRenderPassInput.Color | ScriptableRenderPassInput.Depth;

        VoluemtricLightHelpers.ConfigureInput(this, ScriptableRenderPassInput.Color);
    }

    public override void Dispose()
    {
        this.DisposeMips();
        this._colorBuffer?.Release();
    }

#if UNITY_6000_0_OR_NEWER
    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

        RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
        desc.msaaSamples = 1;
        desc.depthBufferBits = 0;
        int originalWidth = desc.width;
        int originalHeight = desc.height;
        RenderTextureFormat originalColorFormat = desc.colorFormat;

        desc.width /= this._settings.Blur.DownSampling;
        desc.height /= this._settings.Blur.DownSampling;
        desc.colorFormat = this._settings.TempTextureFormat;

        int mipCount = this._settings.Blur.Iterations;
        if (this._downHandles?.Length != mipCount || this._upHandles?.Length != mipCount)
        {
            this._downHandles = new TextureHandle[mipCount];
            this._upHandles = new TextureHandle[mipCount];
        }

        TextureHandle last;

        // raymarch
        using (IRasterRenderGraphBuilder builder =
               renderGraph.AddRasterRenderPass("Volumetric Lighting - Raymarch", out PassData passData))
        {
            passData._settings = this._settings;
            passData._materialIndex = 0;

            passData._source = resourceData.activeColorTexture;
            builder.UseTexture(passData._source);
            builder.UseTexture(resourceData.activeDepthTexture);

            this._downHandles[0] =
                UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, MIP_DOWN_NAMES[0], false);
            this._upHandles[0] =
                UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, MIP_UP_NAMES[0], false);

            passData._target = this._downHandles[0];
            builder.SetRenderAttachment(passData._target, 0, AccessFlags.WriteAll);
            last = passData._target;

            if (resourceData.mainShadowsTexture.IsValid())
                builder.UseTexture(resourceData.mainShadowsTexture);

            builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
        }

        // blur - downsample (gaussian pyramid)
        for (int i = 1; i < mipCount; i++)
        {
            using (IRasterRenderGraphBuilder builder =
                   renderGraph.AddRasterRenderPass("Volumetric Lighting - Blur Horizontal",
                       out PassData passData))
            {
                passData._settings = this._settings;
                passData._materialIndex = 1;

                passData._source = last;
                builder.UseTexture(passData._source);

                this._upHandles[i] =
                    UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, MIP_UP_NAMES[i], false);
                passData._target = this._upHandles[i];
                builder.SetRenderAttachment(passData._target, 0, AccessFlags.ReadWrite);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                    ExecutePass(data, context));
            }

            using (IRasterRenderGraphBuilder builder =
                   renderGraph.AddRasterRenderPass("Volumetric Lighting - Blur Vertical",
                       out PassData passData))
            {
                passData._settings = this._settings;
                passData._materialIndex = 2;

                passData._source = this._upHandles[i];
                builder.UseTexture(passData._source);

                this._downHandles[i] =
                    UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, MIP_DOWN_NAMES[i], false);
                passData._target = this._downHandles[i];
                builder.SetRenderAttachment(passData._target, 0, AccessFlags.ReadWrite);
                last = passData._target;

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                    ExecutePass(data, context));
            }

            desc.width = Mathf.Max(1, desc.width >> 1);
            desc.height = Mathf.Max(1, desc.height >> 1);
        }

        // upsample
        // TODO: this breaks the effect, I think because the first iteration somehow ends up blitting over itself
        // for (int i = mipCount - 2; i >= 0; i--)
        // {
        //     using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass("Volumetric Lighting - Upsample", out PassData passData))
        //     {
        //         passData._settings = this._settings;
        //         passData._materialIndex = 3;
        //         
        //         passData._lowMip = i == mipCount - 2 ? this._downHandles[i + 1] : this._upHandles[i + 1];
        //         builder.UseTexture(passData._lowMip);
        //         
        //         passData._source = this._downHandles[i];
        //         builder.UseTexture(passData._source);
        //     
        //         passData._target = this._upHandles[i];
        //         builder.SetRenderAttachment(passData._target, 0, AccessFlags.WriteAll);
        //         last = passData._target;
        //         
        //         builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
        //     }
        // }

        using (IRasterRenderGraphBuilder builder =
               renderGraph.AddRasterRenderPass("Volumetric Lighting - Composite", out PassData passData))
        {
            passData._settings = this._settings;
            passData._materialIndex = 4;
            passData._raymarchBufferTexture = last;
            builder.UseTexture(passData._raymarchBufferTexture);

            passData._source = resourceData.activeColorTexture;
            builder.UseTexture(passData._source);

            desc.width = originalWidth;
            desc.height = originalHeight;
            desc.colorFormat = originalColorFormat;
            passData._target = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc,
                "_VolumetricLightingColorBuffer", false);
            builder.SetRenderAttachment(passData._target, 0, AccessFlags.WriteAll);
            last = passData._target;

            builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
        }

        using (IRasterRenderGraphBuilder builder =
               renderGraph.AddRasterRenderPass("Volumetric Lighting - Final Blit", out PassData passData))
        {
            passData._source = last;
            builder.UseTexture(passData._source);

            passData._target = resourceData.activeColorTexture;
            builder.SetRenderAttachment(passData._target, 0, AccessFlags.WriteAll);

            builder.SetRenderFunc(
                (PassData data, RasterGraphContext context) => ExecuteFinalBlit(data, context));
        }
    }

    private static void ExecutePass(PassData data, RasterGraphContext context)
    {
        data._settings.Apply();
        if (data._materialIndex == 4)
            data._settings.Material.SetTexture(SHADER_PARAM_RAYMARCH_TEXTURE, data._raymarchBufferTexture);
        if (data._materialIndex == 3)
            data._settings.Material.SetTexture(SHADER_PARAM_SOURCE_TEX_LOW_MIP, data._lowMip);
        Blitter.BlitTexture(context.cmd, data._source, Vector2.one, data._settings.Material, data._materialIndex);
    }

    private static void ExecuteFinalBlit(PassData passData, RasterGraphContext context)
    {
        Blitter.BlitTexture(context.cmd, passData._source, Vector2.one, 0, true);
    }
#endif

    #region CompatibilityMode
    private class Mip
    {
        public readonly string Name;
        public RTHandle Handle;

        public Mip(string name)
        {
            this.Name = name;
            this.Handle = RTHandles.Alloc(Shader.PropertyToID(name), name: name);
        }
    }

    private RTHandle _target;
    private RTHandle _colorBuffer;
    private Mip[] _downMips;
    private Mip[] _upMips;

    protected override void Execute(CommandBuffer cmd, ScriptableRenderContext context)
    {
        this._settings.Apply();

        // raymarch
        Blitter.BlitCameraTexture(
            cmd,
            this._target,
            this._downMips[0].Handle,
            RenderBufferLoadAction.DontCare,
            RenderBufferStoreAction.Store,
            this._settings.Material,
            0
        );

        // blur
        int mipCount = this._settings.Blur.Iterations;
        RTHandle lastDst = this._downMips[0].Handle;
        if (mipCount > 1)
        {
            // Downsample - gaussian pyramid
            RTHandle lastDown = this._downMips[0].Handle;
            for (int i = 1; i < mipCount; i++)
            {
                // Classic two pass gaussian blur - use mipUp as a temporary target
                //   First pass does 2x downsampling + 9-tap gaussian
                //   Second pass does 9-tap gaussian using a 5-tap filter + bilinear filtering
                Blitter.BlitCameraTexture(cmd, lastDown, this._upMips[i].Handle, RenderBufferLoadAction.DontCare,
                    RenderBufferStoreAction.Store, this._settings.Material, 1);
                Blitter.BlitCameraTexture(cmd, this._upMips[i].Handle, this._downMips[i].Handle,
                    RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, this._settings.Material, 2);

                lastDown = this._downMips[i].Handle;
            }

            // Upsample 
            for (int i = mipCount - 2; i >= 0; i--)
            {
                RTHandle lowMip = i == mipCount - 2 ? this._downMips[i + 1].Handle : this._upMips[i + 1].Handle;
                RTHandle highMip = this._downMips[i].Handle;
                RTHandle dst = this._upMips[i].Handle;

                cmd.SetGlobalTexture(SHADER_PARAM_SOURCE_TEX_LOW_MIP, lowMip);
                Blitter.BlitCameraTexture(cmd, highMip, dst, RenderBufferLoadAction.DontCare,
                    RenderBufferStoreAction.Store, this._settings.Material, 3);
                lastDst = dst;
            }
        }

        // composite
        cmd.SetGlobalTexture(SHADER_PARAM_RAYMARCH_TEXTURE, lastDst);
        Blitter.BlitCameraTexture(
            cmd,
            this._target,
            this._colorBuffer,
            RenderBufferLoadAction.DontCare,
            RenderBufferStoreAction.Store,
            this._settings.Material,
            4
        );

        RTHandle from = this._settings.DebugRaymarchBuffer ? lastDst : this._colorBuffer;
        Blitter.BlitCameraTexture(cmd, from, this._target);
    }

    public void SetTarget(RTHandle target)
        => this._target = target;

#if UNITY_6000_0_OR_NEWER
    [Obsolete]
#endif
    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData data)
    {
        RenderTextureDescriptor descriptor = data.cameraData.cameraTargetDescriptor;
        descriptor.depthBufferBits = 0;
        descriptor.msaaSamples = 1;
        RenderingUtils.ReAllocateHandleIfNeeded(ref this._colorBuffer, descriptor, name: "_VolumetricLightingColorBuffer");

        descriptor.width /= this._settings.Blur.DownSampling;
        descriptor.height /= this._settings.Blur.DownSampling;
        descriptor.colorFormat = this._settings.TempTextureFormat;

        int mipCount = this._settings.Blur.Iterations;
        if (this._downMips?.Length != mipCount)
        {
            this.DisposeMips();
            this._downMips = new Mip[mipCount];
            this._upMips = new Mip[mipCount];

            for (int i = 0; i < mipCount; i++)
            {
                this._downMips[i] = new Mip("_VolumetricLightingMipDown" + i);
                this._upMips[i] = new Mip("_VolumetricLightingMipUp" + i);
            }
        }

        for (int i = 0; i < mipCount; i++)
        {
            RenderingUtils.ReAllocateHandleIfNeeded(ref this._upMips[i].Handle, descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, name: this._upMips[i].Name);
            RenderingUtils.ReAllocateHandleIfNeeded(ref this._downMips[i].Handle, descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, name: this._downMips[i].Name);
            descriptor.width = Mathf.Max(1, descriptor.width >> 1);
            descriptor.height = Mathf.Max(1, descriptor.height >> 1);
        }

        this.ConfigureTarget(this._target);
    }

    private void DisposeMips()
    {
        for (int i = 0; i < this._downMips?.Length; i++)
        {
            this._downMips[i].Handle.Release();
            this._upMips[i].Handle.Release();
        }
        this._downMips = null;
        this._upMips = null;
    }
    #endregion

}
