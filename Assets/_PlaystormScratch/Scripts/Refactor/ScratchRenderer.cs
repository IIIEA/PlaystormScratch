using Scripts.Refactor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ScratchCard.Rendering
{
  public class ScratchRenderer : IScratchRenderer
  {
    private Material _brushMaterial;
    private RenderTexture _renderTarget;
    private CommandBuffer _commandBuffer;
    private Mesh _quadMesh;
    private Matrix4x4 _textureMatrix;

    public void Initialize(Material brushMaterial, RenderTexture renderTarget)
    {
      _brushMaterial = brushMaterial;
      _renderTarget = renderTarget;
      _commandBuffer = new CommandBuffer { name = "ScratchRenderer" };
      _quadMesh = CreateQuadMesh();

      // Инициализируем матрицу преобразования
      float scaleX = 1.0f / _renderTarget.width;
      float scaleY = 1.0f / _renderTarget.height;
      _textureMatrix = Matrix4x4.TRS(
        Vector3.zero,
        Quaternion.identity,
        new Vector3(scaleX, scaleY, 1)
      );
    }

    private Mesh CreateQuadMesh()
    {
      var mesh = new Mesh
      {
        vertices = new[]
        {
          new Vector3(-0.5f, 0.5f, 0), // top left
          new Vector3(0.5f, 0.5f, 0), // top right
          new Vector3(0.5f, -0.5f, 0), // bottom right
          new Vector3(-0.5f, -0.5f, 0) // bottom left
        },
        uv = new[]
        {
          new Vector2(0, 1), // top left
          new Vector2(1, 1), // top right
          new Vector2(1, 0), // bottom right
          new Vector2(0, 0) // bottom left
        },
        triangles = new[] { 0, 1, 2, 2, 3, 0 }
      };
      mesh.UploadMeshData(false);
      return mesh;
    }

    public void RenderScratch(Vector2 position, float size)
    {
      var brushMatrix = CalculateBrushMatrix(position, size);
      ExecuteRender(brushMatrix);
    }

    public void RenderLine(Vector2 start, Vector2 end, float size)
    {
      Vector2 direction = end - start;
      float distance = direction.magnitude;
      Vector2 normalized = direction / distance;

      int steps = Mathf.Max(1, Mathf.CeilToInt(distance));
      float stepSize = distance / steps;

      for (int i = 0; i < steps; i++)
      {
        Vector2 currentPos = start + normalized * (i * stepSize);
        RenderScratch(currentPos, size);
      }
    }

    private Matrix4x4 CalculateBrushMatrix(Vector2 position, float size)
    {
      // Преобразуем позицию в пространство текстуры (0,0 в центре)
      Vector3 normalizedPos = new Vector3(
        position.x / _renderTarget.width,
        position.y / _renderTarget.height,
        0
      );

      // Создаем матрицу трансформации для кисти
      return Matrix4x4.TRS(
        normalizedPos,
        Quaternion.identity,
        new Vector3(size, size, 1)
      );
    }

    private void ExecuteRender(Matrix4x4 brushMatrix)
    {
      _commandBuffer.Clear();
      _commandBuffer.SetRenderTarget(_renderTarget);

      // Устанавливаем матрицу преобразования
      Matrix4x4 finalMatrix = _textureMatrix * brushMatrix;
      _commandBuffer.DrawMesh(_quadMesh, finalMatrix, _brushMaterial);

      Graphics.ExecuteCommandBuffer(_commandBuffer);
    }

    public void Clear(Color color)
    {
      _commandBuffer.Clear();
      _commandBuffer.SetRenderTarget(_renderTarget);
      _commandBuffer.ClearRenderTarget(false, true, color);
      Graphics.ExecuteCommandBuffer(_commandBuffer);
    }

    public void Cleanup()
    {
      _commandBuffer?.Release();
      Object.Destroy(_quadMesh);
    }
  }
}