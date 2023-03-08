#ifndef CUSTOM_LIGHT_INCLUDED
#define CUSTOM_LIGHT_INCLUDED
#include "../ShaderLibrary/Shadows.hlsl"

CBUFFER_START(_CustomLight)
float4 _DirectionalLightColor;
float4 _DirectionalLightDirection;
float4 _DirectionalLightShadowData;
CBUFFER_END

half4 LambertDiffuse(float3 normal)
{
    return max(0,dot(normal,_DirectionalLightDirection)) * _DirectionalLightColor;
}

half4 BlinnPhongSpecular(float3 viewDir, float3 normal, float shininess)
{
    float3 halfDir = normalize((viewDir  + _DirectionalLightDirection));
    float nh = max(0,dot(halfDir,normal));
    return pow(nh,shininess) * _DirectionalLightColor;
}

half4 BlinnPongLight(float3 positionWS,float3 normalWS,float shininess,half4 diffuseColor,half4 specularColor)
{
    float3 viewDir = normalize( _WorldSpaceCameraPos - positionWS);
    return  LambertDiffuse(normalWS) * diffuseColor + BlinnPhongSpecular(viewDir,normalWS,shininess) * specularColor; 
}

DirectionalShadowData GetDirectionalShadowData(int lightIndex)
{
    DirectionalShadowData data;
    data.strength = _DirectionalLightShadowData.x;
    data.tileIndex = _DirectionalLightShadowData.y;
    return data;
}

#endif