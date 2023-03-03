#ifndef CUSTOM_COMMON_INCLUDED
#define CUSTOM_COMMON_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "UnityInput.hlsl"

#define UNITY_MATRIX_M     unity_ObjectToWorld
#define UNITY_MATRIX_I_M   unity_WorldToObject
#define UNITY_MATRIX_V     unity_MatrixV
#define UNITY_MATRIX_I_V   unity_MatrixInvV
#define UNITY_MATRIX_P     glstate_matrix_projection
#define UNITY_MATRIX_I_P   unity_MatrixInvP
#define UNITY_MATRIX_VP    unity_MatrixVP
#define UNITY_MATRIX_I_VP  unity_MatrixInvVP
#define UNITY_MATRIX_MV    mul(UNITY_MATRIX_V, UNITY_MATRIX_M)
#define UNITY_MATRIX_T_MV  transpose(UNITY_MATRIX_MV)
#define UNITY_MATRIX_IT_MV transpose(mul(UNITY_MATRIX_I_M, UNITY_MATRIX_I_V))
#define UNITY_MATRIX_MVP   mul(UNITY_MATRIX_VP, UNITY_MATRIX_M)
#define UNITY_PREV_MATRIX_M   unity_MatrixPreviousM
#define UNITY_PREV_MATRIX_I_M unity_MatrixPreviousMI

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"

#endif