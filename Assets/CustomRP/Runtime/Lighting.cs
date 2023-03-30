using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;


public class Lighting
{
    private CustomRenderPipelineAsset _asset;
    const string bufferName = "Lighting";
    CommandBuffer buffer = new CommandBuffer
    {
        name = bufferName
    };

    private static int
        dirLightColorId = Shader.PropertyToID("_DirectionalLightColor"),
        dirLightDirectionId = Shader.PropertyToID("_DirectionalLightDirection"),
        dirLightShadowDataId = Shader.PropertyToID("_DirectionalLightShadowData");
    
    CullingResults cullingResults;

    private ShadowCaster _shadowCaster = new ShadowCaster();

    private const int maxOtherLightCount = 64;
    private static int otherLightCountId = Shader.PropertyToID("_OtherLightCount");
    private static int otherLightColorsId = Shader.PropertyToID("_OtherLightColors");
    private static int otherLightPositionsId = Shader.PropertyToID("_OtherLightPositions");
    private static int otherLightDirectionsId = Shader.PropertyToID("_OtherLightDirections");
    private static int otherLightSpotAnglesId = Shader.PropertyToID("_OtherLightSpotAngles");
    private static int otherLightShadowDataId = Shader.PropertyToID("_OtherLightShadowData");

    private static Vector4[] otherLightColors = new Vector4[maxOtherLightCount];
    private static Vector4[] otherLightPositions = new Vector4[maxOtherLightCount];
    private static Vector4[] otherLightDirections = new Vector4[maxOtherLightCount];
    private static Vector4[] otherLightSpotAngles = new Vector4[maxOtherLightCount];
    private static Vector4[] otherLightShadowData = new Vector4[maxOtherLightCount];
    
    public void Setup (ScriptableRenderContext context,CullingResults cullingResults, CustomRenderPipelineAsset asset)
    {
        this.cullingResults = cullingResults;
        this._asset = asset;
        buffer.BeginSample(bufferName);
        
        // shadow setup;
        _shadowCaster.Setup(context,cullingResults,asset);
        
        SetupLights(asset.enableLightsPerObject);
        
        _shadowCaster.Render();
        buffer.EndSample(bufferName);
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }
    static string lightsPerObjectKeyword = "_LIGHTS_PER_OBJECT";
    void SetupLights(bool useLightsPerObject)
    {
        NativeArray<int> indexMap = useLightsPerObject? cullingResults.GetLightIndexMap(Allocator.Temp) : default;
        
        NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;
        int otherLightCount = 0;
        int i;
        for (i = 0; i < visibleLights.Length; ++i)
        {
            int newIndex = -1;
            VisibleLight visibleLight = visibleLights[i];

            switch (visibleLight.lightType)
            {
                case LightType.Directional:
                {
                    SetupDirectionalLight(0, i,ref visibleLight);
                    break;
                }
                case LightType.Point:
                {
                    if (otherLightCount < maxOtherLightCount)
                    {
                        newIndex = otherLightCount;
                        SetupPointLight( otherLightCount++, i, ref visibleLight);
                    }
                    break;
                }
                case LightType.Spot:
                {
                    if (otherLightCount < maxOtherLightCount) 
                    {
                        newIndex = otherLightCount;
                        SetupSpotLight(otherLightCount++, i, ref visibleLight);
                    }
                    break;
                }
            }
            if (useLightsPerObject) 
            {
                indexMap[i] = newIndex;
            }
        }
        
        if (useLightsPerObject)
        {
            for (; i < indexMap.Length; i++) 
            {
                indexMap[i] = -1;
            }
            cullingResults.SetLightIndexMap(indexMap);
            indexMap.Dispose();
            Shader.EnableKeyword(lightsPerObjectKeyword);
        }
        else
        {
            Shader.DisableKeyword(lightsPerObjectKeyword);
        }
        
        buffer.SetGlobalInt(otherLightCountId, otherLightCount);
        if (otherLightCount > 0)
        {
            buffer.SetGlobalVectorArray(otherLightColorsId, otherLightColors);
            buffer.SetGlobalVectorArray(otherLightPositionsId, otherLightPositions);
            buffer.SetGlobalVectorArray(otherLightDirectionsId, otherLightDirections);
            buffer.SetGlobalVectorArray(otherLightSpotAnglesId,otherLightSpotAngles);
            buffer.SetGlobalVectorArray(otherLightShadowDataId, otherLightShadowData);
        }
    }

    void SetupDirectionalLight(int index, int visibleIndex, ref VisibleLight visibleLight)
    {
        Light light = visibleLight.light;
        buffer.SetGlobalVector(dirLightColorId, light.color.linear);
        buffer.SetGlobalVector(dirLightDirectionId, -light.transform.forward);
        Vector3 shadowData = _shadowCaster.ReserveDirectionalShadows(light,visibleIndex);
        buffer.SetGlobalVector(dirLightShadowDataId,shadowData);
    }

    void SetupPointLight(int index, int visibleIndex, ref VisibleLight visibleLight)
    {
        otherLightColors[index] = visibleLight.finalColor;
        Vector4 position = visibleLight.localToWorldMatrix.GetColumn(3);
        position.w = 1f / Mathf.Max(visibleLight.range * visibleLight.range, 0.0001f);
        otherLightPositions[index] = position;
        otherLightSpotAngles[index] = new Vector4(0f, 1f);
        Light light = visibleLight.light;
        otherLightShadowData[index] = _shadowCaster.ReserveOtherShadows(light, visibleIndex);
    }

    void SetupSpotLight(int index, int visibleIndex, ref VisibleLight visibleLight)
    {
        otherLightColors[index] = visibleLight.finalColor;
        Vector4 position = visibleLight.localToWorldMatrix.GetColumn(3);
        position.w = 1f / Mathf.Max(visibleLight.range * visibleLight.range, 0.00001f);
        otherLightPositions[index] = position;
        otherLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2);
        Light light = visibleLight.light;
        float innerCos = Mathf.Cos(Mathf.Deg2Rad * 0.5f * light.innerSpotAngle);
        float outerCos = Mathf.Cos(Mathf.Deg2Rad * 0.5f * visibleLight.spotAngle);
        float angleRangeInv = 1f / Mathf.Max(innerCos - outerCos, 0.001f);
        otherLightSpotAngles[index] = new Vector4(angleRangeInv, -outerCos * angleRangeInv);
        
        otherLightShadowData[index] = _shadowCaster.ReserveOtherShadows(light, visibleIndex);
    }

    public void Cleanup()
    {
        _shadowCaster.Cleanup();
    }
}
