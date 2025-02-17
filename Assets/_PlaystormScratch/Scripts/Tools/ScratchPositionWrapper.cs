using UnityEngine;

namespace _PlaystormScratch.Tools
{
    public class ScratchPositionWrapper
    {
        private readonly SpriteRenderer _renderer;
        private readonly Rect _spriteRect;
        private readonly Camera _camera;
        private readonly Vector2 _bounds;

        public ScratchPositionWrapper(SpriteRenderer spriteRenderer, Camera camera)
        {
            _camera = camera;
            _renderer = spriteRenderer;
            _spriteRect = _renderer.sprite.rect;
            _bounds = _renderer.sprite.bounds.size;
        }

        private Vector3 GetClickPosition(Vector2 position)
        {
            return _camera.ScreenToWorldPoint(position);
        }

        public Vector2 GetScratchPosition(Vector2 position)
        {
            var clickPosition = GetClickPosition(position);
            var lossyScale = _renderer.transform.lossyScale;
            var pointLocal = Vector2.Scale(_renderer.transform.InverseTransformPoint(clickPosition), lossyScale) + _bounds / 2f;

            var uv = pointLocal / _bounds;
            var scratchPosition = new Vector2(_spriteRect.width * uv.x, _spriteRect.height * uv.y);

            return scratchPosition;
        }
    }
}