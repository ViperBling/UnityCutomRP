using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

partial class CameraRenderer
{
    partial void DrawGizmos();
    partial void DrawUnsupportedShaders();
    partial void PrepareForSceneWindow();
    partial void PrepareBuffer();
#if UNITY_EDITOR
    partial void DrawUnsupportedShaders()
    {
        if (_errorMat == null)
        {
            _errorMat = new Material(Shader.Find("Hidden/InternalErrorShader"));
        }
        var drawSettings = new DrawingSettings(
            _legacyShaderTagIds[0], new SortingSettings(_camera)) { overrideMaterial = _errorMat };
        
        for (int i = 1; i < _legacyShaderTagIds.Length; i++) 
        {
            drawSettings.SetShaderPassName(i, _legacyShaderTagIds[i]);
        }
        
        var filteringSettings = FilteringSettings.defaultValue;
        _context.DrawRenderers(
            _cullingRes, ref drawSettings, ref filteringSettings);
    }

    partial void DrawGizmos()
    {
        if (Handles.ShouldRenderGizmos())
        {
            _context.DrawGizmos(_camera, GizmoSubset.PreImageEffects);
            _context.DrawGizmos(_camera, GizmoSubset.PostImageEffects);
        }
    }

    partial void PrepareForSceneWindow()
    {
        if (_camera.cameraType == CameraType.SceneView)
        {
            ScriptableRenderContext.EmitWorldGeometryForSceneView(_camera);
        }
    }

    partial void PrepareBuffer()
    {
        Profiler.BeginSample("Editor Only");
        _cmdBuffer.name = SampleName = _camera.name;
        Profiler.EndSample();
    }
    
    private static ShaderTagId[] _legacyShaderTagIds =
    {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM")
    };
    static Material _errorMat;
    string SampleName { get; set; }
    
#else
    const string SampleName = BufferName;    
#endif
}


