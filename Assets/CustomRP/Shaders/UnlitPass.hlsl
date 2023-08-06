#pragma once

#include "../ShaderLibrary/Common.hlsl"

UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
    UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

struct VSInput
{
    float3 positionOS : POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID      // 对象的InstanceID
};

struct PSInput
{
    float4 positionCS : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

PSInput UnlitPassVertex(VSInput vsIn)
{
    PSInput vsOut;
    // 用宏提取出对象的InstanceID
    UNITY_SETUP_INSTANCE_ID(vsIn);
    // 把InstanceID从VS传到PS
    UNITY_TRANSFER_INSTANCE_ID(vsIn, vsOut);
    float3 positionWS = TransformObjectToWorld(vsIn.positionOS);
    vsOut.positionCS = TransformWorldToHClip(positionWS);

    return vsOut;
}

float4 UnlitPassFragment(PSInput psIn) : SV_TARGET
{
    // 再次提取
    UNITY_SETUP_INSTANCE_ID(psIn);
    // 根据InstanceID来取颜色
    return UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);
}