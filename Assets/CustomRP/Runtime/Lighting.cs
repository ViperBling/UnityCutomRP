using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;

public class Lighting
{
    public void Setup(ScriptableRenderContext context, CullingResults  cullingResults, ShadowSettings shadowSettings)
    {
        _cullingResults = cullingResults;
        _buffer.BeginSample(BufferName);
        _shadows.Setup(context, cullingResults, shadowSettings);
        SetupLights();
        // 光源设置好了就可以开始渲染阴影贴图
        _shadows.Render();
        _buffer.EndSample(BufferName);
        context.ExecuteCommandBuffer(_buffer);
        _buffer.Clear();
    }

    void SetupLights()
    {
        NativeArray<VisibleLight> visibleLights = _cullingResults.visibleLights;
        int dirLightCount = 0;
        
        for (int i = 0; i < visibleLights.Length; i++)
        {
            VisibleLight visibleLight = visibleLights[i];
            if (visibleLight.lightType == LightType.Directional)
            {
                SetupDirectionalLight(dirLightCount++, ref visibleLight);
                if (dirLightCount >= MaxDirLightCount) break;
            }
        }
        _buffer.SetGlobalInt(_dirLightCountID, visibleLights.Length);
        _buffer.SetGlobalVectorArray(_dirLightColorsID, _dirLightColors);
        _buffer.SetGlobalVectorArray(_dirLightDirectionsID, _dirLightDirections);
        _buffer.SetGlobalVectorArray(_dirLightShadowDataID, _dirLightShadowData);
    }

    void SetupDirectionalLight(int index, ref VisibleLight visibleLight)
    {
        _dirLightColors[index] = visibleLight.finalColor;
        _dirLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2);
        _dirLightShadowData[index] = _shadows.ReserveDirectionalShadows(visibleLight.light, index);
    }

    public void Cleanup()
    {
        _shadows.Cleanup();
    }

    const int MaxDirLightCount = 4;
    const string BufferName = "Lighting";

    private CommandBuffer _buffer = new CommandBuffer
    {
        name = BufferName
    };

    static int _dirLightCountID = Shader.PropertyToID("_DirectionalLightCount");
    static int _dirLightColorsID = Shader.PropertyToID("_DirectionalLightColors");
    static int _dirLightDirectionsID = Shader.PropertyToID("_DirectionalLightDirections");
    static int _dirLightShadowDataID = Shader.PropertyToID("_DirectionalLightShadowData");

    static Vector4[] _dirLightColors = new Vector4[MaxDirLightCount];
    static Vector4[] _dirLightDirections = new Vector4[MaxDirLightCount];
    static Vector4[] _dirLightShadowData = new Vector4[MaxDirLightCount];
    
    CullingResults _cullingResults;

    // Light类要跟踪阴影，对阴影进行Setup
    Shadows _shadows = new Shadows();
}
