using Scripts.Core;
using Scripts.Core.InputData;
using Scripts.Core.ScratchData;
using UnityEngine;
using UnityEngine.Rendering;

namespace Scripts
{
  public class ScratchCard : MonoBehaviour
  {
    [SerializeField] private Transform _surfaceTransform;
    [SerializeField] public Material _surfaceMaterial;
    
    [field: SerializeField] public Material BrushMaterial { get; set;  }
    [field: SerializeField] public Quality RenderTextureQuality { get; private set; } = Quality.HIGH;

    private RenderTexture _renderTexture;
    
    private ScratchCardInput _input;
    private ScratchCardRenderer _cardRenderer;

    public RenderTargetIdentifier RenderTarget { get; private set; }
    public BaseData ScratchData { get; private set; }

    public float BrushSize;
    
    private void Start()
    {
      Init();
    }

    private void Update()
    {
      _input.TryUpdate();
    }

    private void OnDestroy()
    {
      UnsubscribeFromEvents();
      ReleaseRenderTexture();
      
      _cardRenderer?.Release();
    }

    private void Init()
    {
      UnsubscribeFromEvents();
      _input = new ScratchCardInput();
      SubscribeToEvents();
      _cardRenderer?.Release();
      _cardRenderer = new ScratchCardRenderer(this);
      
      ReleaseRenderTexture();
      CreateRenderTexture();
      
      _cardRenderer.FillRenderTextureWithColor(Color.clear);
    }

    public void SetRenderType(Camera mainCamera)
    {
      ScratchData = new SpriteRendererData(_surfaceTransform, mainCamera);
    }

    private void SubscribeToEvents()
    {
      UnsubscribeFromEvents();
      
      _input.OnScratch += ScratchData.GetScratchPosition;
      _input.OnScratchHole += TryScratchHole;
      _input.OnScratchLine += TryScratchLine;
    }

    private void UnsubscribeFromEvents()
    {
      if (_input == null)
        return;

      _input.OnScratch -= ScratchData.GetScratchPosition;
      _input.OnScratchHole -= TryScratchHole;
      _input.OnScratchLine -= TryScratchLine;
    }

    private void CreateRenderTexture()
    {
      var qualityRatio = (float)RenderTextureQuality;
      var renderTextureSize =
        new Vector2(ScratchData.TextureSize.x / qualityRatio, ScratchData.TextureSize.y / qualityRatio);
      var renderTextureFormat = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.R8)
        ? RenderTextureFormat.R8
        : RenderTextureFormat.ARGB32;
      _renderTexture = new RenderTexture((int)renderTextureSize.x, (int)renderTextureSize.y, 0, renderTextureFormat);
      _surfaceMaterial.SetTexture(Constants.MaskShader.MaskTexture, _renderTexture);
      RenderTarget = new RenderTargetIdentifier(_renderTexture);
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
    
    private void TryScratchHole(Vector2 position)
    {
      _cardRenderer.ScratchHole(position);
    }

    private void TryScratchLine(Vector2 startPosition, Vector2 endPosition)
    {
      _cardRenderer.ScratchLine(startPosition, endPosition);
    }

    public void Fill()
    {
      _cardRenderer.FillRenderTextureWithColor(Color.white);
    }

    public void Clear()
    {
      _cardRenderer.FillRenderTextureWithColor(Color.clear);
    }
  }
}