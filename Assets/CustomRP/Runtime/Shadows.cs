using UnityEngine;
using UnityEngine.Rendering;

public class Shadows
{
    public void Setup(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings shadowSettings)
    {
        _context = context;
        _cullingResults = cullingResults;
        _settings = shadowSettings;

        _shadowedDirecLightCount = 0;
    }

    void ExecuteBuffer()
    {
        _context.ExecuteCommandBuffer(_buffer);
        _buffer.Clear();
    }

    public void ReserveDirectionalShadows(Light light, int visibleLightIndex)
    {
        if (_shadowedDirecLightCount < MaxShadowedDirLightCount)
        {
            _shadowedDirectionalLights[_shadowedDirecLightCount++] = new ShadowedDirectionalLight
            {
                VisibleLightIndex = visibleLightIndex
            };
        }
    }

    struct ShadowedDirectionalLight
    {
        public int VisibleLightIndex;
    }
    ShadowedDirectionalLight[] _shadowedDirectionalLights = new ShadowedDirectionalLight[MaxShadowedDirLightCount];

    int _shadowedDirecLightCount;
    
    const string BufferName = "Shadows";
    private CommandBuffer _buffer = new CommandBuffer
    {
        name = BufferName
    };

    ScriptableRenderContext _context;
    CullingResults _cullingResults;
    ShadowSettings _settings;

    const int MaxShadowedDirLightCount = 1;
}
