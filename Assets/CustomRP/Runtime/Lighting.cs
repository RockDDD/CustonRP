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
    static int
        dirLightColorId = Shader.PropertyToID("_DirectionalLightColor"),
        dirLightDirectionId = Shader.PropertyToID("_DirectionalLightDirection");
    
    CullingResults cullingResults;

    private ShadowCaster _shadowCaster = new ShadowCaster();
    
    public void Setup (ScriptableRenderContext context,CullingResults cullingResults, CustomRenderPipelineAsset asset)
    {
        this.cullingResults = cullingResults;
        this._asset = asset;
        buffer.BeginSample(bufferName);
        
        // shadow setup;
        _shadowCaster.Setup(context,cullingResults,asset);
        
        SetupLights();
        
        _shadowCaster.Render();
        buffer.EndSample(bufferName);
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    void SetupLights()
    {
        NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;
        int mainIndex = -1;
        for (int i = 0; i < visibleLights.Length; ++i)
        {
            VisibleLight visibleLight = visibleLights[i];
            if (visibleLight.lightType == LightType.Directional)
            {
                mainIndex = i;
                break;
            }
        }

        if (mainIndex >= 0)
        {
            SetupDirectionalLight(visibleLights[mainIndex],mainIndex);
        }
    }

    void SetupDirectionalLight(VisibleLight visibleLight, int index)
    {
        Light light = visibleLight.light;
        buffer.SetGlobalVector(dirLightColorId, light.color.linear);
        buffer.SetGlobalVector(dirLightDirectionId, -light.transform.forward);
        _shadowCaster.ReserveDirectionalShadows(light,index);
    }

    public void Cleanup()
    {
        _shadowCaster.Cleanup();
    }
}
