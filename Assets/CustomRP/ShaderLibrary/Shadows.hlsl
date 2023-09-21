#pragma once

#define MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT 4
#define MAX_CASCADE_COUNT 4

TEXTURE2D_SHADOW(_DirectionalShadowAtlas);
#define SHADOW_SAMPLER sampler_linear_clamp_compare
SAMPLER_CMP(SHADOW_SAMPLER);

CBUFFER_START(_CustomShadows)
    int _CascadeCount;
    float4 _CascadeCullingSpheres[MAX_CASCADE_COUNT];
    float4 _CascadeData[MAX_CASCADE_COUNT];
    // 这里乘上Cascade数量后需要重启Unity进行初始化，因为数组的大小在同一个会话中不会改变
    float4x4 _DirectionalShadowMatrices[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT * MAX_CASCADE_COUNT];
    float4 _ShadowDistanceFade;
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
    float normalBias;
};

// 根据距离衰减(1 - d/m) / f
float FadeShadowStrength(float distance, float scale, float fade)
{
    return saturate((1.0 - distance * scale) * fade);
}

ShadowData GetShadowData(Surface surfaceWS)
{
    ShadowData data;
    data.strength = FadeShadowStrength(surfaceWS.depth, _ShadowDistanceFade.x, _ShadowDistanceFade.y);
    int i;
    for (i = 0; i < _CascadeCount; i++)
    {
        float4 sphere = _CascadeCullingSpheres[i];
        float distanceSqr = DistanceSquare(surfaceWS.position, sphere.xyz);
        if (distanceSqr < sphere.w)
        {
            if (i == _CascadeCount - 1)
            {
                data.strength *= FadeShadowStrength(distanceSqr, _CascadeData[i].x, _ShadowDistanceFade.z);
            }
            break;
        }
    }
    if (i == _CascadeCount) data.strength = 0.0;
    data.cascadeIndex = i;
    return  data;
}

float SampleDirectionalShadowAtlas(float3 positionSTS)
{
    return SAMPLE_TEXTURE2D_SHADOW(_DirectionalShadowAtlas, SHADOW_SAMPLER, positionSTS);
}

float GetDirectionalShadowAttenuation(DirectionalShadowData directionalShadowData, ShadowData globalShadowData, Surface surfaceWS)
{
    if (directionalShadowData.strength <= 0.0) return 1.0;
    float3 normalBias = surfaceWS.normal * (directionalShadowData.normalBias * _CascadeData[globalShadowData.cascadeIndex].y);
    // 把position转换到shadow tile space，然后采样ShadowMap
    float3 positionSTS = mul(
        _DirectionalShadowMatrices[directionalShadowData.tileIndex],
        float4(surfaceWS.position + normalBias, 1.0)).xyz;
    float shadow = SampleDirectionalShadowAtlas(positionSTS);
    // 根据strength插值
    return lerp(1.0, shadow, directionalShadowData.strength);
}