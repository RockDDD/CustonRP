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
        Tags { "RenderType"="Opaque" "LightMode"="XForwardBase"}
        LOD 100
        Pass 
        {
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex LightPassVertex
			#pragma fragment LightPassFragment
			#include "BlinnPhongLight.hlsl"
            ENDHLSL
        }
    }
}