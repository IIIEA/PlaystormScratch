using UnityEngine;

namespace Scripts.Refactor
{
  public class ScratchCardView : MonoBehaviour
  {
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Material _maskMaterial;

    public Vector2 TextureSize => _spriteRenderer.sprite.rect.size;

    public void Initialize(RenderTexture maskTexture)
    {
      _maskMaterial.SetTexture("_MaskTex", maskTexture);
      _spriteRenderer.material = _maskMaterial;
    }
  }
}