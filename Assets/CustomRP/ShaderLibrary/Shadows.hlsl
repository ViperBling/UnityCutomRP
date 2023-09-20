#pragma once

#define MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT 4
#define MAX_CASCADE_COUNT 4

TEXTURE2D_SHADOW(_DirectionalShadowAtlas);
#define SHADOW_SAMPLER sampler_linear_clamp_compare
SAMPLER_CMP(SHADOW_SAMPLER);

CBUFFER_START(_CustomShadows)
    int _CascadeCount;
    float4 _CascadeCullingSpheres[MAX_CASCADE_COUNT];
    // 这里乘上Cascade数量后需要重启Unity进行初始化，因为数组的大小在同一个会话中不会改变
    float4x4 _DirectionalShadowMatrices[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT * MAX_CASCADE_COUNT];
    float _ShadowDistance;
CBUFFER_END

struct ShadowData
{
    int cascadeIndex;
    float strength;
};

struct DirectionalShadowData
{
    float strength;
    int tileIndex;
};

ShadowData GetShadowData(Surface surfaceWS)
{
    ShadowData data;
    data.strength = surfaceWS.depth < _ShadowDistance ? 1.0 : 0.0;
    int i;
    for (i = 0; i < _CascadeCount; i++)
    {
        float4 sphere = _CascadeCullingSpheres[i];
        float distanceSqr = DistanceSquare(surfaceWS.position, sphere.xyz);
        if (distanceSqr < sphere.w) break;
    }
    if (i == _CascadeCount) data.strength = 0.0;
    data.cascadeIndex = i;
    return  data;
}

float SampleDirectionalShadowAtlas(float3 positionSTS)
{
    return SAMPLE_TEXTURE2D_SHADOW(_DirectionalShadowAtlas, SHADOW_SAMPLER, positionSTS);
}

float GetDirectionalShadowAttenuation(DirectionalShadowData data, Surface surfaceWS)
{
    if (data.strength <= 0.0) return 1.0;
    // 把position转换到shadow tile space，然后采样ShadowMap
    float3 positionSTS = mul(
        _DirectionalShadowMatrices[data.tileIndex],
        float4(surfaceWS.position, 1.0)).xyz;
    float shadow = SampleDirectionalShadowAtlas(positionSTS);
    // 根据strength插值
    return lerp(1.0, shadow, data.strength);
}