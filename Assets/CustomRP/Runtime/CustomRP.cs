using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPipeline : RenderPipeline
{
    public CustomRenderPipeline(
        bool useDynamicBatching,
        bool useGPUInstancing,
        bool useSRPBatcher)
    {
        this._useDynamicBatching = useDynamicBatching;
        this._useGPUInstancing = useGPUInstancing;
        GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
        GraphicsSettings.lightsUseLinearIntensity = true;
    }
    
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (Camera cam in cameras)
        {
            _renderer.Render(context, cam, _useDynamicBatching, _useGPUInstancing);
        }
    }
    
    CameraRenderer _renderer = new CameraRenderer();

    bool _useDynamicBatching;
    bool _useGPUInstancing;
}