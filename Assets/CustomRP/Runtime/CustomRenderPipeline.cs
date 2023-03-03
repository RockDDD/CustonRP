using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPipeline : RenderPipeline
{
    const string bufferName = "Render Camera";
    CommandBuffer buffer = new CommandBuffer 
    {
        name = bufferName
    };
    ScriptableRenderContext context;
    
    private ShaderTagId _shaderTag = new ShaderTagId("XForwardBase");
    private CustomRenderPipelineAsset _asset;
    public CustomRenderPipeline(CustomRenderPipelineAsset asset)
    {
        _asset = asset;
        GraphicsSettings.useScriptableRenderPipelineBatching = _asset.enableSrpBatcher;
    }

    private Lighting _lightingManager = new Lighting();
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (var camera in cameras)
        {
            RenderPerCamera(context,camera);
        }
    }
    
    private void RenderPerCamera(ScriptableRenderContext context,Camera camera)
    {
        this.context = context;
        camera.TryGetCullingParameters( out var cullingParams);
        cullingParams.shadowDistance = Mathf.Min(_asset.shadows.maxDistance,camera.farClipPlane);
        var cullingResults = context.Cull(ref cullingParams);
        
        buffer.BeginSample(bufferName);
        ExecuteBuffer();
        _lightingManager.Setup(context,cullingResults,_asset);
        buffer.EndSample(bufferName);
        
        context.SetupCameraProperties(camera);
        CameraClearFlags flags = camera.clearFlags;
        buffer.ClearRenderTarget(
            flags <= CameraClearFlags.Depth,
            flags == CameraClearFlags.Color,
            flags == CameraClearFlags.Color ?
                camera.backgroundColor.linear : Color.clear
        );
   

        buffer.BeginSample(bufferName);
        ExecuteBuffer();
        var sortingSetting = new SortingSettings(camera);
        var drawingSettings = new DrawingSettings(_shaderTag, sortingSetting);
        var filteringSettings = new FilteringSettings(RenderQueueRange.all);

        context.DrawRenderers(
            cullingResults, ref drawingSettings, ref filteringSettings
        );
        
        context.DrawSkybox(camera);
        
        _lightingManager.Cleanup();
        
        buffer.EndSample(bufferName);
        ExecuteBuffer();
        
        context.Submit();
    }
    
    void ExecuteBuffer () {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

}