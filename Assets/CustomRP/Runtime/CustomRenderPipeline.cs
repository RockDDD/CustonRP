using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPipeline : RenderPipeline
{
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
        context.SetupCameraProperties(camera);
        camera.TryGetCullingParameters( out var cullingParams);
        var cullingResults = context.Cull(ref cullingParams);
        
        _lightingManager.Setup(context,cullingResults);
        
        var sortingSetting = new SortingSettings(camera);
        var drawingSettings = new DrawingSettings(_shaderTag, sortingSetting);
        var filteringSettings = new FilteringSettings(RenderQueueRange.all);

        context.DrawRenderers(
            cullingResults, ref drawingSettings, ref filteringSettings
        );
        
        context.DrawSkybox(camera);
        context.Submit();
    }
}