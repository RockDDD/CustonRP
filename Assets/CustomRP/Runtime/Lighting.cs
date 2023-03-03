using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;


public class Lighting
{
    const string bufferName = "Lighting";
    CommandBuffer buffer = new CommandBuffer
    {
        name = bufferName
    };
    static int
        dirLightColorId = Shader.PropertyToID("_DirectionalLightColor"),
        dirLightDirectionId = Shader.PropertyToID("_DirectionalLightDirection");
    
    CullingResults cullingResults;
    
    public void Setup (ScriptableRenderContext context,CullingResults cullingResults)
    {
        this.cullingResults = cullingResults;
        
        buffer.BeginSample(bufferName);
        SetupLights();
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
            SetupDirectionalLight(visibleLights[mainIndex]);
        }
    }

    void SetupDirectionalLight(VisibleLight visibleLight)
    {
        Light light = visibleLight.light;
        buffer.SetGlobalVector(dirLightColorId, light.color.linear);
        buffer.SetGlobalVector(dirLightDirectionId, -light.transform.forward);
    }
}
