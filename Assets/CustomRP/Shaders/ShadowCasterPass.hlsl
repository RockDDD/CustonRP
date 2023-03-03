#ifndef CUSTOM_SHADOW_CASTER_PASS_INCLUDED
#define CUSTOM_SHADOW_CASTER_PASS_INCLUDED
#include "../ShaderLibrary/Common.hlsl"
TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);

CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;
float4 _BaseColor;
float _Shininess;
CBUFFER_END

struct Attributes
{
    float3 positionOS : POSITION;
    float2 baseUV : TEXCOORD0;
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float2 baseUV : VAR_BASE_UV;
};

Varyings ShadowCasterPassVertex (Attributes input)
{
    Varyings output;
    float3 positionWS = TransformObjectToWorld(input.positionOS);
    output.positionCS = TransformWorldToHClip(positionWS);
    return output;
}

void ShadowCasterPassFragment (Varyings input)
{

}

#endif