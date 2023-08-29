using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/CustomRP")]
public class CustomRPAsset : RenderPipelineAsset
{
    protected override RenderPipeline CreatePipeline()
    {
        return new CustomRenderPipeline(useDynamicBatching, useGPUInstancing, useSRPBatcher, shadows);
    }

    [SerializeField] 
        bool useDynamicBatching = true,
        useGPUInstancing = true, 
        useSRPBatcher = true;

    [SerializeField] 
    ShadowSettings shadows = default;
}