using System.Collections.Generic;
using Scripts.Tools;
using UnityEngine;
using UnityEngine.Rendering;

namespace Scripts.Core
{
  public class ScratchCardRenderer
  {
    private readonly ScratchCard _scratchCard;
    private Mesh _meshHole;
    private Mesh _meshLine;
    private CommandBuffer _commandBuffer;
    private Bounds _localBounds;

    private readonly List<Vector3> _positions = new();
    private readonly List<Color> _colors = new();
    private readonly List<int> _indices = new();
    private readonly List<Vector2> _uv = new();

    public ScratchCardRenderer(ScratchCard card)
    {
      _scratchCard = card;
      _localBounds = new Bounds(Vector2.one / 2f, Vector2.one);
      _commandBuffer = new CommandBuffer { name = "ScratchCardRenderer" };
      _meshHole = MeshGenerator.GenerateQuad(Vector3.zero, Vector2.zero);
    }

    public void Release()
    {
      if (_commandBuffer != null)
      {
        _commandBuffer.Release();
        _commandBuffer = null;
      }

      if (_meshHole != null)
      {
        Object.Destroy(_meshHole);
        _meshHole = null;
      }

      if (_meshLine != null)
      {
        Object.Destroy(_meshLine);
        _meshLine = null;
      }
    }

    private bool IsInBounds(Rect rect)
    {
      return _localBounds.Intersects(new Bounds(rect.center, rect.size));
    }

    public void ScratchHole(Vector2 position)
    {
      var brushTexture = _scratchCard.BrushMaterial.mainTexture;
      
      var positionRect = new Rect(
        (position.x - 0.5f * brushTexture.width * _scratchCard.BrushSize) /
        _scratchCard.ScratchData.TextureSize.x,
        (position.y - 0.5f * brushTexture.height * _scratchCard.BrushSize) /
        _scratchCard.ScratchData.TextureSize.y,
        brushTexture.width * _scratchCard.BrushSize /
        _scratchCard.ScratchData.TextureSize.x,
        brushTexture.height * _scratchCard.BrushSize /
        _scratchCard.ScratchData.TextureSize.y);

      if (IsInBounds(positionRect))
      {
        _meshHole.vertices = new[]
        {
          new Vector3(positionRect.xMin, positionRect.yMax, 0),
          new Vector3(positionRect.xMax, positionRect.yMax, 0),
          new Vector3(positionRect.xMax, positionRect.yMin, 0),
          new Vector3(positionRect.xMin, positionRect.yMin, 0)
        };

        GL.LoadOrtho();
        _commandBuffer.Clear();
        _commandBuffer.SetRenderTarget(_scratchCard.RenderTarget);
        _commandBuffer.DrawMesh(_meshHole, Matrix4x4.identity, _scratchCard.BrushMaterial);
        Graphics.ExecuteCommandBuffer(_commandBuffer);
      }
    }

    public void ScratchLine(Vector2 startPosition, Vector2 endPosition)
    {
      _positions.Clear();
      _colors.Clear();
      _indices.Clear();
      _uv.Clear();

      var holesCount = (int)Vector2.Distance(startPosition, endPosition) / (int)_scratchCard.RenderTextureQuality;
      holesCount = Mathf.Max(1, holesCount);
      var count = 0;
      for (var i = 0; i < holesCount; i++)
      {
        var t = i / (float)Mathf.Clamp(holesCount - 1, 1, holesCount - 1);
        var holePosition = startPosition + (endPosition - startPosition) / holesCount * i;

        var positionRect = new Rect(
          (holePosition.x - 0.5f * _scratchCard.BrushMaterial.mainTexture.width * _scratchCard.BrushSize) /
          _scratchCard.ScratchData.TextureSize.x,
          (holePosition.y - 0.5f * _scratchCard.BrushMaterial.mainTexture.height * _scratchCard.BrushSize) /
          _scratchCard.ScratchData.TextureSize.y,
          _scratchCard.BrushMaterial.mainTexture.width * _scratchCard.BrushSize /
          _scratchCard.ScratchData.TextureSize.x,
          _scratchCard.BrushMaterial.mainTexture.height * _scratchCard.BrushSize /
          _scratchCard.ScratchData.TextureSize.y);

        if (IsInBounds(positionRect))
        {
          _positions.Add(new Vector3(positionRect.xMin, positionRect.yMax, 0));
          _positions.Add(new Vector3(positionRect.xMax, positionRect.yMax, 0));
          _positions.Add(new Vector3(positionRect.xMax, positionRect.yMin, 0));
          _positions.Add(new Vector3(positionRect.xMin, positionRect.yMin, 0));

          _colors.Add(Color.white);
          _colors.Add(Color.white);
          _colors.Add(Color.white);
          _colors.Add(Color.white);

          _uv.Add(Vector2.up);
          _uv.Add(Vector2.one);
          _uv.Add(Vector2.right);
          _uv.Add(Vector2.zero);

          _indices.Add(0 + count * 4);
          _indices.Add(1 + count * 4);
          _indices.Add(2 + count * 4);
          _indices.Add(2 + count * 4);
          _indices.Add(3 + count * 4);
          _indices.Add(0 + count * 4);

          count++;
        }
      }

      if (_positions.Count > 0)
      {
        if (_meshLine != null)
        {
          _meshLine.Clear(false);
        }
        else
        {
          _meshLine = new Mesh();
        }

        _meshLine.vertices = _positions.ToArray();
        _meshLine.uv = _uv.ToArray();
        _meshLine.triangles = _indices.ToArray();
        _meshLine.colors = _colors.ToArray();
        GL.LoadOrtho();
        _commandBuffer.Clear();
        _commandBuffer.SetRenderTarget(_scratchCard.RenderTarget);
        _commandBuffer.DrawMesh(_meshLine, Matrix4x4.identity, _scratchCard.BrushMaterial);
        Graphics.ExecuteCommandBuffer(_commandBuffer);
      }
    }

    public void FillRenderTextureWithColor(Color color)
    {
      _commandBuffer.SetRenderTarget(_scratchCard.RenderTarget);
      _commandBuffer.ClearRenderTarget(false, true, color);
      Graphics.ExecuteCommandBuffer(_commandBuffer);
    }
  }
}