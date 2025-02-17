using UnityEngine;

namespace Scripts.Refactor
{
  [CreateAssetMenu(fileName = "ScratchCardConfig", menuName = "ScratchCard/Config")]
  public class ScratchCardConfig : ScriptableObject
  {
    public float BrushSize = 1f;
    public Texture BrushTexture;
    public Shader BrushShader;
  }
}