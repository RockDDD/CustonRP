#ifndef CUSTOM_BLINNPHONG_LIGHT_PASS_INCLUDED
#define CUSTOM_BLINNPHONG_LIGHT_PASS_INCLUDED

#include "../ShaderLibrary/Common.hlsl"
#include "../ShaderLibrary/Surface.hlsl"
#include "../ShaderLibrary/Light.hlsl"
struct Attributes
{
    float3 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float2 baseUV : TEXCOORD0;
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float3 positionWS : VAR_POSITION;
    float3 normalWS : VAR_NORMAL;
    float2 baseUV : VAR_BASE_UV;
};

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);

CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;
float4 _BaseColor;
float _Shininess;
CBUFFER_END

Varyings LightPassVertex (Attributes input)
{
    Varyings output;
    output.positionWS = TransformObjectToWorld(input.positionOS);
    output.positionCS = TransformWorldToHClip(output.positionWS);
    output.normalWS = TransformObjectToWorldNormal(input.normalOS);
    float4 baseST = _BaseMap_ST;
    output.baseUV = input.baseUV * baseST.xy + baseST.zw;
    return output;
}

half4 LightPassFragment (Varyings input) : SV_TARGET
{
    float4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.baseUV);
    float4 baseColor = baseMap * _BaseColor;
    Surface surface;
    surface.color = baseColor.xyz;
    surface.alpha = baseColor.w;
    surface.position = input.positionWS;
    surface.normal = input.normalWS;
    surface.smoothness = _Shininess;
    surface.metallic = 0;
    surface.viewDirection = normalize(_WorldSpaceCameraPos - input.positionWS);
    surface.depth = -TransformWorldToView(input.positionWS).z;
    float3 color = GetLighting(surface);
    return half4(color,1.0f);
    //half4 color = BlinnPongLight(positionWS,normalWS,_Shininess,baseColor,half4(1,1,1,1));
    //ShadowData shadowData = GetShadowData(positionWS);
    //DirectionalShadowData dirShadowData = GetDirectionalShadowData(0,shadowData);
    //float attenuation = GetDirectionalShadowAttenuation(dirShadowData, shadowData, normalWS, positionWS);
    //return color * attenuation;
}
#endif