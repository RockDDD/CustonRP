Shader "Unlit/BlinphongShader"
{
    Properties 
    {
        _BaseMap("Texture", 2D) = "white" {}
		_BaseColor("Color", Color) = (0.5, 0.5, 0.5, 1.0)
        _Shininess("Shininess",Range(10,128)) = 50
    }

    SubShader
    {
        LOD 100
        Pass 
        {
        	Name "BlinnPhongLightShader"
            Tags { "RenderType"="Opaque" "LightMode"="XForwardBase"}
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex LightPassVertex
			#pragma fragment LightPassFragment
			#include "BlinnPhongLight.hlsl"
            ENDHLSL
        }
        Pass
        {
        	Name "BlinnPhongShaderShadowCaster"
            Tags { "RenderType"="Opaque" "LightMode"="ShadowCaster"}
            ColorMask 0

			HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex ShadowCasterPassVertex
			#pragma fragment ShadowCasterPassFragment
			#include "ShadowCasterPass.hlsl"
			ENDHLSL
        }
    }
}