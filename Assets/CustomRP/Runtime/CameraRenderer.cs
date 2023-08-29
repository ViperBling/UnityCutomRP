using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Partial Class可以把一个类切分到不同的文件，这样可以针对Runtime和Editor模式分别实现对应的功能
public partial class CameraRenderer
{
    public void Render(
        ScriptableRenderContext context, Camera camera,
        bool useDynamicBatching, bool useGPUInstancing,
        ShadowSettings shadowSettings)
    {
        this._context = context;
        this._camera = camera;

        PrepareBuffer();
        PrepareForSceneWindow();
        if (!Cull(shadowSettings.maxDistance)) return;
        
        // 在常规渲染之前开始渲染阴影
        _cmdBuffer.BeginSample(SampleName);
        ExecuteBuffer();
        _lighting.Setup(context, _cullingRes, shadowSettings);
        _cmdBuffer.EndSample(SampleName);
        Setup();
        DrawVisibleGeometry(useDynamicBatching, useGPUInstancing);
        DrawUnsupportedShaders();
        DrawGizmos();
        // 清除阴影RT
        _lighting.Cleanup();
        Submit();
    }

    void Setup()
    {
        // 在ClearRenderTarget之前设置相机属性可以更快
        _context.SetupCameraProperties(_camera);
        CameraClearFlags flags = _camera.clearFlags;
        _cmdBuffer.ClearRenderTarget(
            flags <= CameraClearFlags.Depth, 
            flags == CameraClearFlags.Color,
            flags == CameraClearFlags.Color ? _camera.backgroundColor.linear : Color.clear);
        _cmdBuffer.BeginSample(SampleName);
        ExecuteBuffer();
    }

    bool Cull(float maxShadowDistance)
    {
        if (_camera.TryGetCullingParameters(out ScriptableCullingParameters cullingParameters))
        {
            cullingParameters.shadowDistance = Mathf.Min(maxShadowDistance, _camera.farClipPlane);
            _cullingRes = _context.Cull(ref cullingParameters);
            return true;
        }

        return false;
    }
    
    void DrawVisibleGeometry(bool useDynamicBatching, bool useGPUInstancing)
    {
        var sortSettings = new SortingSettings(_camera)
        {
            criteria = SortingCriteria.CommonOpaque
        };
        var drawSettings = new DrawingSettings(
            _unlitShaderTagId, sortSettings
        )
        {
            enableDynamicBatching = useDynamicBatching,
            enableInstancing = useGPUInstancing
        };
        drawSettings.SetShaderPassName(1, _litShaderTagId);
        
        var filterSettings = new FilteringSettings(RenderQueueRange.opaque);
        
        _context.DrawRenderers(
            _cullingRes, ref drawSettings, ref filterSettings);
        
        _context.DrawSkybox(_camera);

        sortSettings.criteria = SortingCriteria.CommonTransparent;
        drawSettings.sortingSettings = sortSettings;
        filterSettings.renderQueueRange = RenderQueueRange.transparent;
        _context.DrawRenderers(_cullingRes, ref drawSettings, ref filterSettings);
    }

    void Submit()
    {
        _cmdBuffer.EndSample(SampleName);
        ExecuteBuffer();
        _context.Submit();
    }

    void ExecuteBuffer()
    {
        _context.ExecuteCommandBuffer(_cmdBuffer);
        _cmdBuffer.Clear();
    }

    ScriptableRenderContext _context;
    Camera _camera;
    CullingResults _cullingRes;

    const string BufferName = "Render Camera";
    private CommandBuffer _cmdBuffer = new CommandBuffer
    {
        name = BufferName
    };

    static ShaderTagId _unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
    static ShaderTagId _litShaderTagId = new ShaderTagId("CustomLit");

    Lighting _lighting = new Lighting();
}


