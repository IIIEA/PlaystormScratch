using _PlaystormScratch.InputData;
using _PlaystormScratch.Tools;
using UnityEngine;

namespace _PlaystormScratch.Core.ScratchModule
{
    public class ScratchSpriteController : MonoBehaviour
    {
        [SerializeField] private ScratchSpriteView _view;
        [SerializeField] private Quality _renderTextureQuality = Quality.HIGH;
        [SerializeField] private Material _brushMaterial;
        [SerializeField] private float _brushSize;

        private PlayerInputs _input;
        private ScratchSpriteRenderer _spriteRenderer;
        private ScratchPositionWrapper _scratchData;
        
        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            _input.TryUpdate();
        }

        private void OnDestroy()
        {
            Cleanup();
        }

        private void Initialize()
        {
            _scratchData = new ScratchPositionWrapper(_view.SpriteRenderer, Camera.main);
            _view.CreateRenderTexture(_renderTextureQuality, _view.SpriteRenderer.sprite.rect.size,
                out var renderTexture, out var renderTargetIdentifier);
            
            _input = new PlayerInputs();
            _spriteRenderer = new ScratchSpriteRenderer(_brushSize, new Vector2(renderTexture.width, renderTexture.height),
                _brushMaterial, renderTargetIdentifier, _renderTextureQuality);
            
            
            SubscribeToEvents();
        }

        private void Cleanup()
        {
            UnsubscribeFromEvents();
            _spriteRenderer?.Release();
        }

        private void SubscribeToEvents()
        {
            _input.OnScratch += _scratchData.GetScratchPosition;
            _input.OnScratchHole += _spriteRenderer.ScratchHole;
            _input.OnScratchLine += _spriteRenderer.ScratchLine;
        }

        private void UnsubscribeFromEvents()
        {
            _input.OnScratch -= _scratchData.GetScratchPosition;
            _input.OnScratchHole -= _spriteRenderer.ScratchHole;
            _input.OnScratchLine -= _spriteRenderer.ScratchLine;
        }
    }
}