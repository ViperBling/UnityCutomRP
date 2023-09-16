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
            RenderDirectionalShadows(i, split, tileSize);
        }
        _buffer.SetGlobalMatrixArray(_directionalShadowMatricesID, _directionalShadowMatrices);
        _buffer.EndSample(BufferName);
        ExecuteBuffer();
    }

    void RenderDirectionalShadows(int index, int split, int tileSize)
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
        // 根据每个光源的不同矩阵来分配
        _directionalShadowMatrices[index] = ConvertToAtlasMatrix(
            projMatrix * viewMatrix, SetTileViewport(index, split, tileSize), split);
        _buffer.SetViewProjectionMatrices(viewMatrix, projMatrix);
        ExecuteBuffer();
        _context.DrawShadows(ref shadowSettings);
    }

    Vector2 SetTileViewport(int index, int split, float tileSize)
    {
        Vector2 offset = new Vector2(index % split, index / split);
        _buffer.SetViewport(new Rect(
            offset.x * tileSize, offset.y * tileSize, tileSize, tileSize));
        return offset;
    }

    Matrix4x4 ConvertToAtlasMatrix(Matrix4x4 mat, Vector2 offset, int split)
    {
        if (SystemInfo.usesReversedZBuffer) {
            mat.m20 = -mat.m20;
            mat.m21 = -mat.m21;
            mat.m22 = -mat.m22;
            mat.m23 = -mat.m23;
        }
        float scale = 1f / split;
        mat.m00 = (0.5f * (mat.m00 + mat.m30) + offset.x * mat.m30) * scale;
        mat.m01 = (0.5f * (mat.m01 + mat.m31) + offset.x * mat.m31) * scale;
        mat.m02 = (0.5f * (mat.m02 + mat.m32) + offset.x * mat.m32) * scale;
        mat.m03 = (0.5f * (mat.m03 + mat.m33) + offset.x * mat.m33) * scale;
        mat.m10 = (0.5f * (mat.m10 + mat.m30) + offset.y * mat.m30) * scale;
        mat.m11 = (0.5f * (mat.m11 + mat.m31) + offset.y * mat.m31) * scale;
        mat.m12 = (0.5f * (mat.m12 + mat.m32) + offset.y * mat.m32) * scale;
        mat.m13 = (0.5f * (mat.m13 + mat.m33) + offset.y * mat.m33) * scale;
        mat.m20 = 0.5f * (mat.m20 + mat.m30);
        mat.m21 = 0.5f * (mat.m21 + mat.m31);
        mat.m22 = 0.5f * (mat.m22 + mat.m32);
        mat.m23 = 0.5f * (mat.m23 + mat.m33);
        
        return mat;
    }

    void ExecuteBuffer()
    {
        _context.ExecuteCommandBuffer(_buffer);
        _buffer.Clear();
    }

    // 获取哪些光源会产生阴影
    public Vector2 ReserveDirectionalShadows(Light light, int visibleLightIndex)
    {
        // 判断条件：光源可以投射阴影且阴影强度大于0 && GetShadowCasterBounds检查光源是否在阴影投射范围
        if (_shadowedDirectionalLightCount < MaxShadowedDirLightCount &&
            light.shadows != LightShadows.None && light.shadowStrength > 0f &&
            _cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds bounds))
        {
            _shadowedDirectionalLights[_shadowedDirectionalLightCount] = new ShadowedDirectionalLight
            {
                VisibleLightIndex = visibleLightIndex
            };
            // 根据返回对应的shadowStrength和offset
            return new Vector2(
                light.shadowStrength, _shadowedDirectionalLightCount++);
        }
        return Vector2.zero;
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
    // 每个光源的阴影矩阵
    static int _directionalShadowMatricesID = Shader.PropertyToID("_DirectionalShadowMatrices");
    static Matrix4x4[] _directionalShadowMatrices = new Matrix4x4[MaxShadowedDirLightCount];
}
