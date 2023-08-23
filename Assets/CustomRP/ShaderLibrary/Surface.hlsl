#pragma once

struct Surface
{
    float3 normal;
    float3 viewDirection;
    float3 color;
    float alpha;

    float metallic;
    float smoothness;
};