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
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


public abstract class AbstractRendererPass : ScriptableRenderPass
{

    private readonly ProfilingSampler _sampler;

    protected AbstractRendererPass(string name)
    {
        this._sampler = new ProfilingSampler(name);
    }

    /// <summary>
    /// Clean up any allocated resources
    /// </summary>
    public abstract void Dispose();

    /// <summary>
    /// Execute with the given CommandBuffer
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="context"></param>
    protected abstract void Execute(CommandBuffer cmd, ScriptableRenderContext context);

#if UNITY_6000_0_OR_NEWER
    [Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
#endif
    public override void Execute(ScriptableRenderContext context, ref RenderingData data)
    {
        CommandBuffer cmd = CommandBufferPool.Get();
        try
        {
            using (new ProfilingScope(cmd, this._sampler))
                this.Execute(cmd, context);
            context.ExecuteCommandBuffer(cmd);
        }
        finally
        {
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
    }
}
