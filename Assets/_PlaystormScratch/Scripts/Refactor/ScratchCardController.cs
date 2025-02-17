using UnityEngine;
using ScratchCard.Rendering;
using Scripts.Refactor;

namespace ScratchCard.Core
{
    public class ScratchCardController : MonoBehaviour
    {
        [SerializeField] private ScratchCardView _view;
        [SerializeField] private ScratchCardConfig _config;
        
        [Header("Debug")]
        [SerializeField] private bool _showDebug = true;
        
        private IInputProcessor _inputProcessor;
        private IScratchRenderer _renderer;
        private RenderTexture _renderTexture;
        private Material _brushMaterial;
        private Vector2 _lastPosition;
        private bool _isDrawing;
        private Camera _mainCamera;
        private SpriteRenderer _spriteRenderer;
        private Vector2 _spriteSize;
        private Vector2 _spritePixelSize;

        private void Awake()
        {
            _mainCamera = Camera.main;
            _spriteRenderer = _view.GetComponent<SpriteRenderer>();
            CalculateSpriteSize();
            InitializeComponents();
        }

        private void CalculateSpriteSize()
        {
            if (_spriteRenderer == null || _spriteRenderer.sprite == null) return;
            
            // Get sprite bounds in world units
            _spriteSize = _spriteRenderer.bounds.size;
            
            // Get sprite size in pixels
            _spritePixelSize = new Vector2(
                _spriteRenderer.sprite.texture.width,
                _spriteRenderer.sprite.texture.height
            );
        }

        private void Start()
        {
            InitializeRenderer();
        }

        private void Update()
        {
            _inputProcessor.Update();
        }

        private void OnDestroy()
        {
            Cleanup();
        }

        private void InitializeComponents()
        {
            _inputProcessor = new InputProcessor();
            _renderer = new ScratchRenderer();
            
            _inputProcessor.OnInputMove += HandleInputMove;
            _inputProcessor.OnInputStart += HandleInputStart;
            _inputProcessor.OnInputEnd += HandleInputEnd;

            _brushMaterial = CreateBrushMaterial();
            _renderTexture = CreateRenderTexture();
        }

        private void InitializeRenderer()
        {
            _renderer.Initialize(_brushMaterial, _renderTexture);
            _view.Initialize(_renderTexture);
            _renderer.Clear(Color.clear);
        }

        private bool IsPointInsideSprite(Vector2 worldPoint)
        {
            Bounds bounds = _spriteRenderer.bounds;
            return bounds.Contains(new Vector3(worldPoint.x, worldPoint.y, bounds.center.z));
        }

        private Vector2 ScreenToTextureSpace(Vector2 screenPosition)
        {
            // Convert screen position to world position
            Vector3 worldPoint = _mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -_mainCamera.transform.position.z));

            if (_showDebug)
            {
                Debug.Log($"Screen Position: {screenPosition}");
                Debug.Log($"World Point: {worldPoint}");
            }

            // Early exit if point is outside sprite bounds
            if (!IsPointInsideSprite(worldPoint))
            {
                if (_showDebug) Debug.Log("Point outside sprite bounds");
                return Vector2.zero;
            }

            // Get local position relative to sprite
            Vector3 localPoint = _spriteRenderer.transform.InverseTransformPoint(worldPoint);
            
            // Convert to normalized position (0-1 range)
            Vector2 normalizedPosition = new Vector2(
                (localPoint.x / _spriteSize.x) + 0.5f,
                (localPoint.y / _spriteSize.y) + 0.5f
            );

            // Convert to texture space
            Vector2 texturePosition = new Vector2(
                normalizedPosition.x * _view.TextureSize.x,
                normalizedPosition.y * _view.TextureSize.y
            );

            if (_showDebug)
            {
                Debug.Log($"Local Point: {localPoint}");
                Debug.Log($"Normalized Position: {normalizedPosition}");
                Debug.Log($"Texture Position: {texturePosition}");
            }

            return texturePosition;
        }

        private void HandleInputStart(Vector2 position)
        {
            Vector2 texturePosition = ScreenToTextureSpace(position);
            
            if (texturePosition == Vector2.zero) return;
            
            _lastPosition = texturePosition;
            _isDrawing = true;
            _renderer.RenderScratch(texturePosition, _config.BrushSize);

            if (_showDebug)
            {
                Debug.Log($"Start Drawing at texture position: {texturePosition}");
            }
        }

        private void HandleInputMove(Vector2 position)
        {
            if (!_isDrawing) return;

            Vector2 texturePosition = ScreenToTextureSpace(position);
            
            if (texturePosition == Vector2.zero) return;

            if (Vector2.Distance(_lastPosition, texturePosition) > 0.01f)
            {
                _renderer.RenderLine(_lastPosition, texturePosition, _config.BrushSize);
                _lastPosition = texturePosition;

                if (_showDebug)
                {
                    Debug.Log($"Drawing line from {_lastPosition} to {texturePosition}");
                }
            }
        }

        private void HandleInputEnd()
        {
            if (_showDebug)
            {
                Debug.Log("End Drawing");
            }
            _isDrawing = false;
        }

        private void OnDrawGizmos()
        {
            if (!_showDebug || _spriteRenderer == null) return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(_spriteRenderer.bounds.center, _spriteRenderer.bounds.size);
        }

        private void Cleanup()
        {
            _renderer.Cleanup();
            _renderTexture.Release();
            Destroy(_renderTexture);
            Destroy(_brushMaterial);
        }

        private Material CreateBrushMaterial()
        {
            var material = new Material(_config.BrushShader);
            material.mainTexture = _config.BrushTexture;
            return material;
        }

        private RenderTexture CreateRenderTexture()
        {
            return new RenderTexture(
                (int)_view.TextureSize.x,
                (int)_view.TextureSize.y,
                0,
                RenderTextureFormat.R8
            )
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
        }

        public void Fill()
        {
            _renderer.Clear(Color.white);
        }

        public void Clear()
        {
            _renderer.Clear(Color.clear);
        }
    }
}