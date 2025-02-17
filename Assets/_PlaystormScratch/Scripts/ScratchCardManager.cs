using Scripts.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Scripts
{
  public class ScratchCardManager : MonoBehaviour
  {
    public ScratchCard _card;

    [SerializeField] private SpriteRenderer _spriteRendererCard;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Sprite _scratchSurfaceSprite;

    [SerializeField] private Texture _brushTexture;
    [SerializeField] private float _brushSize = 1f;

    [SerializeField] private Shader _brushShader;

    private void Awake()
    {
      _mainCamera = Camera.main;
      
      if (!Application.isPlaying)
      {
        InitSurfaceMaterial();
      }

      if (!Application.isPlaying)
      {
        return;
      }

      Init();
    }

    private void Init()
    {
      InitSurfaceMaterial();
      InitBrushMaterial();

      _card.BrushSize = _brushSize;
      _card.SetRenderType(_mainCamera);
    }

    private void InitSurfaceMaterial()
    {
      UpdateCardSprite(_scratchSurfaceSprite);
    }

    private void UpdateCardSprite(Sprite sprite)
    {
      var scratchSurfaceMaterial = _card._surfaceMaterial;

      if (Application.isPlaying)
      {
        if (scratchSurfaceMaterial != null && _scratchSurfaceSprite != null)
        {
          scratchSurfaceMaterial.mainTexture = _scratchSurfaceSprite.texture;
        }
      }
      else if (_card._surfaceMaterial != null && _scratchSurfaceSprite != null)
      {
        _card._surfaceMaterial.mainTexture = _scratchSurfaceSprite.texture;
      }

      UpdateCardOffset();

      if (_card._surfaceMaterial != null)
      {
        _spriteRendererCard.sharedMaterial = _card._surfaceMaterial;
      }

      if (sprite != null)
      {
        _spriteRendererCard.sprite = sprite;
      }
    }

    private void UpdateCardOffset()
    {
      if (_card._surfaceMaterial != null)
      {
        _card._surfaceMaterial.SetVector(Constants.MaskShader.Offset, new Vector4(0, 0, 1, 1));
      }
    }

    private void InitBrushMaterial()
    {
      if (_card.BrushMaterial == null)
      {
        _card.BrushMaterial = new Material(_brushShader);
      }

      _card.BrushMaterial.mainTexture = _brushTexture;
      _card.BrushMaterial.color = Color.white;
      _card.BrushMaterial.SetInt(Constants.BrushShader.BLEND_OP_SHADER_PARAM, (int)BlendOp.Add);
    }

    [ContextMenu("Fill")]
    public void FillScratchCard()
    {
      _card.Fill();
    }

    [ContextMenu("Clear")]
    public void ClearScratchCard()
    {
      _card.Clear();
    }
  }
}