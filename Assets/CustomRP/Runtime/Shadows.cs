using UnityEngine;
using UnityEngine.Rendering;

/*
 *  阴影类
 */
public class Shadows
{
    public void Setup(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings shadowSettings)
    {
        _context = context;
        _cullingResults = cullingResults;
        _settings = shadowSettings;
        
        _shadowedDirectionalLightCount = 0;
    }

    public void Render()
    {
        if (_shadowedDirectionalLightCount > 0)
        {
            RenderDirectionalShadows();
        }
        else
        {
            _buffer.GetTemporaryRT(_directionalShadowAtlasID, 1, 1, 
                32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
        }
    }

    void RenderDirectionalShadows()
    {
        int atlasSize = (int)_settings.directional.atlasSize;
        // 创建临时RT来渲染阴影贴图
        _buffer.GetTemporaryRT(_directionalShadowAtlasID, atlasSize, atlasSize,
            32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
        // 设置阴影RT资源状态
        _buffer.SetRenderTarget(
            _directionalShadowAtlasID, 
            RenderBufferLoadAction.DontCare, 
            RenderBufferStoreAction.Store);
        _buffer.ClearRenderTarget(true, false, Color.clear);
        _buffer.BeginSample(BufferName);
        ExecuteBuffer();

        int split = _shadowedDirectionalLightCount <= 1 ? 1 : 2;
        int tileSize = atlasSize / split;
        for (int i = 0; i < _shadowedDirectionalLightCount; i++)
        {
            RenderDirectionalShadows(i, atlasSize);
        }
        
        _buffer.EndSample(BufferName);
        ExecuteBuffer();
    }

    void RenderDirectionalShadows(int index, int tileSize)
    {
        ShadowedDirectionalLight light = _shadowedDirectionalLights[index];
        // 新版本API添加了投射矩阵类型参数
        var shadowSettings = new ShadowDrawingSettings(_cullingResults, light.VisibleLightIndex, BatchCullingProjectionType.Orthographic);
        // 根据一个视图和投影矩阵来确定阴影的投射范围
        // 第一个参数是光源索引，第二、三、四用来控制级联阴影
        _cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
            light.VisibleLightIndex, 0, 1, Vector3.zero, tileSize, 0.0f,
            out Matrix4x4 viewMatrix, out Matrix4x4 projMatrix, out ShadowSplitData splitData);
        shadowSettings.splitData = splitData;
        _buffer.SetViewProjectionMatrices(viewMatrix, projMatrix);
        ExecuteBuffer();
        _context.DrawShadows(ref shadowSettings);
    }

    void ExecuteBuffer()
    {
        _context.ExecuteCommandBuffer(_buffer);
        _buffer.Clear();
    }

    // 获取哪些光源会产生阴影
    public void ReserveDirectionalShadows(Light light, int visibleLightIndex)
    {
        // 判断条件：光源可以投射阴影且阴影强度大于0 && GetShadowCasterBounds检查光源是否在阴影投射范围
        if (_shadowedDirectionalLightCount < MaxShadowedDirLightCount &&
            light.shadows != LightShadows.None && light.shadowStrength > 0f &&
            _cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds bounds))
        {
            _shadowedDirectionalLights[_shadowedDirectionalLightCount++] = new ShadowedDirectionalLight
            {
                VisibleLightIndex = visibleLightIndex
            };
        }
    }

    public void Cleanup()
    {
        if (_shadowedDirectionalLightCount > 0)
        {
            _buffer.ReleaseTemporaryRT(_directionalShadowAtlasID);
            ExecuteBuffer();
        }
    }

    struct ShadowedDirectionalLight
    {
        public int VisibleLightIndex;
    }
    ShadowedDirectionalLight[] _shadowedDirectionalLights = new ShadowedDirectionalLight[MaxShadowedDirLightCount];

    // 当前投射阴影的直射光数量
    int _shadowedDirectionalLightCount;
    
    const string BufferName = "Shadows";
    CommandBuffer _buffer = new CommandBuffer
    {
        name = BufferName
    };

    ScriptableRenderContext _context;
    CullingResults _cullingResults;
    ShadowSettings _settings;

    // 最多投射阴影的光源数量
    const int MaxShadowedDirLightCount = 4;

    // 使用_DirectionalShadowAtlas来在shader中引用阴影图集
    static int _directionalShadowAtlasID = Shader.PropertyToID("_DirectionalShadowAtlas");
}
