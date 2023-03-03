using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/CustomRenderPipeline")]
public class CustomRenderPipelineAsset : RenderPipelineAsset
{
    [SerializeField]
    private bool _srpBatcher = true;

    public bool enableSrpBatcher
    {
        get{
            return _srpBatcher;
        }
    }
    protected override RenderPipeline CreatePipeline()
    {
        return new CustomRenderPipeline(this);
    }
}
