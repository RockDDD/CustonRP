#ifndef CUSTOM_LIGHT_INCLUDED
#define CUSTOM_LIGHT_INCLUDED
#include "../ShaderLibrary/Shadows.hlsl"

#define MAX_OTHER_LIGHT_COUNT 64

CBUFFER_START(_CustomLight)
float4 _DirectionalLightColor;
float4 _DirectionalLightDirection;
float4 _DirectionalLightShadowData;

int _OtherLightCount;
float4 _OtherLightColors[MAX_OTHER_LIGHT_COUNT];
float4 _OtherLightPositions[MAX_OTHER_LIGHT_COUNT];
float4 _OtherLightDirections[MAX_OTHER_LIGHT_COUNT];
float4 _OtherLightSpotAngles[MAX_OTHER_LIGHT_COUNT];
CBUFFER_END

struct Light
{
    float3 color;
    float3 direction;
    float attenuation;
};

int GetOtherLightCount()
{
    return _OtherLightCount;
}

float3 LambertDiffuse(Surface surfaceWS, Light light)
{
    return max(0,dot(surfaceWS.normal,light.direction)) * light.color * surfaceWS.color;
}

float3 BlinnPhongSpecular(Surface surfaceWS, Light light)
{
    float3 halfDir = normalize((surfaceWS.viewDirection  + light.direction));
    float nh = max(0,dot(halfDir,surfaceWS.normal));
    return pow(nh,surfaceWS.smoothness) * light.color;
}

float3 BlinnPongLight(Surface surfaceWS, Light light)
{
    return  (LambertDiffuse(surfaceWS,light) + BlinnPhongSpecular(surfaceWS,light)) * light.attenuation; 
}

DirectionalShadowData GetDirectionalShadowData(ShadowData shadowData)
{
    DirectionalShadowData data;
    data.strength = _DirectionalLightShadowData.x * shadowData.strength;
    data.tileIndex = _DirectionalLightShadowData.y + shadowData.cascadeIndex;
    data.normalBias = _DirectionalLightShadowData.z;
    return data;
}

Light GetDirectionalLight(Surface surfaceWS, ShadowData shadowData)
{
    DirectionalShadowData dirShadowData = GetDirectionalShadowData(shadowData);
    float attenuation = GetDirectionalShadowAttenuation(dirShadowData, shadowData, surfaceWS);
    Light light;
    light.color = _DirectionalLightColor;
    light.direction = _DirectionalLightDirection;
    light.attenuation = attenuation;
    return light;
}

Light GetOtherLight(int index, Surface surfaceWS, ShadowData shadowData)
{
    Light light;
    light.color = _OtherLightColors[index].rgb;
    float3 ray = _OtherLightPositions[index].xyz - surfaceWS.position;
    light.direction = normalize(ray);
    float distanceSqr = max(dot(ray,ray),0.00001);
    float rangeAttenuation = Square(saturate(1.0 - Square(distanceSqr * _OtherLightPositions[index].w)));
    float4 spotAngles = _OtherLightSpotAngles[index];
    float spotAttenuation = Square(
            saturate(dot(_OtherLightDirections[index].xyz, light.direction) *
            spotAngles.x + spotAngles.y)
        );
    light.attenuation = spotAttenuation  * rangeAttenuation / distanceSqr;
    return light;
}

float3 GetLighting(Surface surfaceWS)
{
    ShadowData shadowData = GetShadowData(surfaceWS.position);
    float3 color = 0.0;
    // direction light
    Light light = GetDirectionalLight(surfaceWS,shadowData);
    color += BlinnPongLight(surfaceWS, light);

    #if defined(_LIGHTS_PER_OBJECT)
    int count = min(unity_LightData.y,8);
    for (int j = 0; j < count; j++)
    {
        int lightIndex = unity_LightIndices[(uint)j / 4][(uint)j % 4];
        Light light = GetOtherLight(lightIndex, surfaceWS, shadowData);
        color += BlinnPongLight(surfaceWS, light);
    }
    #else
    for (int j = 0; j < GetOtherLightCount(); ++j)
    {
        Light light = GetOtherLight(j, surfaceWS, shadowData);
        color += BlinnPongLight(surfaceWS, light);
    }
    #endif


    return color;
}

#endif