using UnityEngine;
using UnityEngine.Rendering;

namespace _PlaystormScratch.Core.ScratchModule
{
    public class ScratchSpriteView : MonoBehaviour
    {
        [field: SerializeField] public Transform SurfaceTransform { get; private set; }
        [field: SerializeField] public SpriteRenderer SpriteRenderer { get; private set; }
        
        private RenderTexture _renderTexture;
        private RenderTargetIdentifier _renderTarget;
        
        private void OnDestroy()
        {
            ReleaseRenderTexture();
        }

        public void CreateRenderTexture(Quality renderTextureQuality, Vector2 textureSize, out RenderTexture renderTexture, out RenderTargetIdentifier targetIdentifier)
        {
            var qualityRatio = (float)renderTextureQuality;
            var renderTextureSize = new Vector2(textureSize.x / qualityRatio, textureSize.y / qualityRatio);
            var renderTextureFormat = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.R8)
                ? RenderTextureFormat.R8
                : RenderTextureFormat.ARGB32;

            _renderTexture =
                new RenderTexture((int)renderTextureSize.x, (int)renderTextureSize.y, 0, renderTextureFormat);
            SpriteRenderer.material.SetTexture(Constants.MaskShader.MaskTexture, _renderTexture);
            _renderTarget = new RenderTargetIdentifier(_renderTexture);

            renderTexture = _renderTexture;
            targetIdentifier = _renderTarget;
        }

        private void ReleaseRenderTexture()
        {
            if (_renderTexture != null && _renderTexture.IsCreated())
            {
                _renderTexture.Release();
                Destroy(_renderTexture);
                _renderTexture = null;
            }
        }
    }
}