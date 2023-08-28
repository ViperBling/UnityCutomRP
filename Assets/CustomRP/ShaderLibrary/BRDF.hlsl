#pragma once

struct BRDF
{
    float3 diffuse;
    float3 specular;
    float roughness;
};

#define MIN_REFLECTIVITY 0.04

float OneMinusReflectivity(float metallic)
{
    float range = 1.0 - MIN_REFLECTIVITY;
    return range - metallic * range;
}

float3 F_Schlik(float3 F0, float F90, float u)
{
    return F0 + (F90 - F0) * pow(1.f - u, 5);
}

float V_SmithGGXCorrelated(float NoL, float NoV, float alphaG)
{
    float alphaG2 = alphaG * alphaG;
    float Lambda_GGXV = NoV * sqrt((-NoV * alphaG2 + NoV) * NoV + alphaG2);
    float Lambda_GGXL = NoV * sqrt((-NoL * alphaG2 + NoL) * NoL + alphaG2);
    return 0.5f / (Lambda_GGXV + Lambda_GGXL);
}

float D_GGX(float m, float NoH)
{
    float m2 = m * m;
    float f = (m2 - 1) * NoH * NoH + 1.0;
    return m2 / (f * f);
}

float Fr_DisneyDiffuse(float NoV, float NoL, float LoH, float perceptualRoughness, out float3 f0, out float fd90)
{
    float energyBias = lerp(0, 0.5, perceptualRoughness);
    float energyFactor = lerp(1.0, 1.0 / 1.51, perceptualRoughness);
    fd90 = energyBias + 2.0 * LoH * LoH * perceptualRoughness;
    f0 = float3(1.0f, 1.0f, 1.0f);

    float lightScatter = F_Schlik(f0, fd90, NoL).r;
    float viewScatter = F_Schlik(f0, fd90, NoV).r;

    return lightScatter * viewScatter * energyFactor;
}

BRDF GetBRDF(Surface surface, bool applyAlphaToDiffuse = false)
{
    BRDF brdf;
    float oneMinusReflectivity = OneMinusReflectivity(surface.metallic);
    brdf.diffuse = surface.color * oneMinusReflectivity;
    if (applyAlphaToDiffuse)
    {
        brdf.diffuse *= surface.alpha;
    }
    brdf.specular = lerp(MIN_REFLECTIVITY, surface.color, surface.metallic);
    float perceptualRoughness = PerceptualSmoothnessToPerceptualRoughness(surface.smoothness);
    brdf.roughness = PerceptualRoughnessToRoughness(perceptualRoughness);

    return brdf;
}

float SpecularBRDF(Surface surface, BRDF brdf, Light light)
{
    float3 h = SafeNormalize(light.direction + surface.viewDirection);
    float NdotH = Square(saturate(dot(surface.normal, h)));
    float LdotH = Square(saturate(dot(light.direction, h)));
    float r2 = Square(brdf.roughness);
    float d2 = Square(NdotH * (r2 - 1.0) + 1.0001);
    float normalization = brdf.roughness * 4.0 + 2.0;

    return r2 / (d2 * max(0.1, LdotH) * normalization);
}

float DirectBRDF(Surface surface, BRDF brdf, Light light)
{
    return SpecularBRDF(surface, brdf, light) * brdf.specular + brdf.diffuse;
}