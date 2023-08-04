using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPipeline : RenderPipeline
{
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (Camera cam in cameras)
        {
            _renderer.Render(context, cam);
        }
    }
    CameraRenderer _renderer = new CameraRenderer();
}