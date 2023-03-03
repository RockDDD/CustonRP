using UnityEngine;
using UnityEngine.Rendering;

public class ShadowCaster 
{
    const string bufferName = "Shadows";

    CommandBuffer buffer = new CommandBuffer {
        name = bufferName
    };
    ScriptableRenderContext context;

    CullingResults cullingResults;

    CustomRenderPipelineAsset _asset;
    
    const int maxShadowedDirectionalLightCount = 1;
    
    static int dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas");
    
    struct ShadowedDirectionalLight 
    {
        public int visibleLightIndex;
    }

    ShadowedDirectionalLight[] ShadowedDirectionalLights = new ShadowedDirectionalLight[maxShadowedDirectionalLightCount];
    private int ShadowedDirectionalLightCount = 0;
    public void Setup (ScriptableRenderContext context, CullingResults cullingResults, CustomRenderPipelineAsset asset) 
    {
        this.context = context;
        this.cullingResults = cullingResults;
        this._asset = asset;
        ShadowedDirectionalLightCount = 0;
    }

    public void ReserveDirectionalShadows(Light light, int index)
    {
        if (light.shadows != LightShadows.None && light.shadowStrength > 0f && cullingResults.GetShadowCasterBounds(index,out Bounds b))
        {
            ShadowedDirectionalLights[0] = new ShadowedDirectionalLight
            {
                visibleLightIndex = index
            };
            ShadowedDirectionalLightCount++;
        }
    }

    void RenderDirectionalShadows()
    {
        int atlasSize = (int)_asset.shadows.directional.atlasSize;
        buffer.GetTemporaryRT(dirShadowAtlasId, atlasSize, atlasSize, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
        buffer.SetRenderTarget(
            dirShadowAtlasId,
            RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store
        );
        buffer.ClearRenderTarget(true, false, Color.clear);
        buffer.BeginSample(bufferName);
        ExecuteBuffer();

        int split = ShadowedDirectionalLightCount <= 1 ? 1 : 2;
        int tileSize = atlasSize / split;
        for (int i = 0; i < ShadowedDirectionalLightCount; i++) 
        {
            RenderDirectionalShadows(i, split,tileSize);
        }
        buffer.EndSample(bufferName);
        ExecuteBuffer();
    }

    void SetTileViewport(int index, int split, float tileSize)
    {
        Vector2 offset = new Vector2(index % split, index / split);
        buffer.SetViewport(new Rect(offset.x * tileSize, offset.y * tileSize, tileSize,tileSize));
    }

    void RenderDirectionalShadows(int index, int split, int tileSize)
    {
        ShadowedDirectionalLight light = ShadowedDirectionalLights[index];
        var shadowSettings = new ShadowDrawingSettings(cullingResults, light.visibleLightIndex);
        cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
            light.visibleLightIndex, 0, 1, Vector3.zero, tileSize, 0f,
            out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix,
            out ShadowSplitData splitData
        );
        shadowSettings.splitData = splitData;
        buffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
        ExecuteBuffer();
        context.DrawShadows(ref shadowSettings);
    }
    
    public void Render()
    {
        if (ShadowedDirectionalLightCount > 0) 
        {
            RenderDirectionalShadows();
        }
        else
        {
            buffer.GetTemporaryRT(dirShadowAtlasId, 1, 1, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
        }
    }
    void ExecuteBuffer ()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    public void Cleanup()
    {
        buffer.ReleaseTemporaryRT(dirShadowAtlasId);
        ExecuteBuffer();
    }
}