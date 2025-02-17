using System.Collections.Generic;
using _PlaystormScratch.Tools;
using UnityEngine;
using UnityEngine.Rendering;

namespace _PlaystormScratch.Core.ScratchModule
{
    public class ScratchSpriteRenderer
    {
        private readonly float _brushSize;
        private readonly Vector2 _textureSize;
        private readonly Material _brushMaterial;
        private readonly RenderTargetIdentifier _renderIdentifier;
        private readonly Quality _quality;

        private Mesh _meshHole;
        private Mesh _meshLine;
        private CommandBuffer _commandBuffer;
        private Bounds _localBounds;

        private readonly List<Vector3> _positions = new();
        private readonly List<Color> _colors = new();
        private readonly List<int> _indices = new();
        private readonly List<Vector2> _uv = new();

        public ScratchSpriteRenderer(float brushSize, Vector2 textureSize, Material brushMaterial,
            RenderTargetIdentifier renderIdentifier, Quality quality)
        {
            _brushSize = brushSize;
            _textureSize = textureSize;
            _brushMaterial = brushMaterial;
            _renderIdentifier = renderIdentifier;
            _quality = quality;

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

        private Rect GetRect(Vector3 position)
        {
            var brushTexture = _brushMaterial.mainTexture;

            var positionRect = new Rect(
                (position.x - 0.5f * brushTexture.width * _brushSize) / _textureSize.x,
                (position.y - 0.5f * brushTexture.height * _brushSize) / _textureSize.y,
                brushTexture.width * _brushSize / _textureSize.x,
                brushTexture.height * _brushSize / _textureSize.y);

            return positionRect;
        }

        private void SetCommandBuffer(Mesh targetMesh)
        {
            GL.LoadOrtho();
            _commandBuffer.Clear();
            _commandBuffer.SetRenderTarget(_renderIdentifier);
            _commandBuffer.DrawMesh(targetMesh, Matrix4x4.identity, _brushMaterial);
            Graphics.ExecuteCommandBuffer(_commandBuffer);
        }

        public void ScratchHole(Vector2 position)
        {
            var positionRect = GetRect(position);

            if (IsInBounds(positionRect))
            {
                _meshHole.vertices = new[]
                {
                    new Vector3(positionRect.xMin, positionRect.yMax, 0),
                    new Vector3(positionRect.xMax, positionRect.yMax, 0),
                    new Vector3(positionRect.xMax, positionRect.yMin, 0),
                    new Vector3(positionRect.xMin, positionRect.yMin, 0)
                };

                SetCommandBuffer(_meshHole);
            }
        }

        public void ScratchLine(Vector2 startPosition, Vector2 endPosition)
        {
            _positions.Clear();
            _colors.Clear();
            _indices.Clear();
            _uv.Clear();

            var holesCount = (int)Vector2.Distance(startPosition, endPosition) / (int)_quality;
            holesCount = Mathf.Max(1, holesCount);
            var count = 0;
            
            for (var i = 0; i < holesCount; i++)
            {
                var t = i / (float)Mathf.Clamp(holesCount - 1, 1, holesCount - 1);
                var holePosition = startPosition + (endPosition - startPosition) / holesCount * i;

                var positionRect = GetRect(holePosition);

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
                
                SetCommandBuffer(_meshLine);
            }
        }
    }
}