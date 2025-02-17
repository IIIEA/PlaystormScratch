using UnityEngine;

namespace Scripts.Refactor
{
  public interface IScratchRenderer
  {
    void Initialize(Material brushMaterial, RenderTexture renderTarget);
    void RenderScratch(Vector2 position, float size);
    void RenderLine(Vector2 start, Vector2 end, float size);
    void Clear(Color color);
    void Cleanup();
  }
}