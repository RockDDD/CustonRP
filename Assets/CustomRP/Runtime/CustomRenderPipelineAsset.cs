using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable]
public class ShadowSettings
{
    [Min(0f)]
    public float maxDistance = 100f;
        
    public enum MapSize 
    {
        _256 = 256, _512 = 512, _1024 = 1024,
        _2048 = 2048, _4096 = 4096, _8192 = 8192
    }

    [System.Serializable]
    public struct Directional
    {
        public MapSize atlasSize;
    }
    public Directional directional = new Directional 
    {
        atlasSize = MapSize._1024
    };
}

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
    [SerializeField]
    public ShadowSettings shadows = default;
    
    protected override RenderPipeline CreatePipeline()
    {
        return new CustomRenderPipeline(this);
    }
}
