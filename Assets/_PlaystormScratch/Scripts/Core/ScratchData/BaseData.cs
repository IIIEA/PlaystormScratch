using UnityEngine;

namespace Scripts.Core.ScratchData
{
  public abstract class BaseData
  {
    private Triangle _triangle;
    
    private Transform Surface { get; }
    
    protected abstract Rect? SpriteRect { get; }
    protected abstract Vector2 Bounds { get; }

    protected virtual bool IsOrthographic => Camera.orthographic;
    
    public abstract Vector2 TextureSize { get; }
    public Camera Camera { protected get; set; }

    protected BaseData(Transform surface, Camera camera)
    {
      Surface = surface;
      Camera = camera;
    }

    protected void InitTriangle()
    {
      var bounds = Bounds;
      
      var position0 = new Vector3(-bounds.x / 2f, -bounds.y / 2f, 0);
      var uv0 = Vector2.zero;

      var position1 = new Vector3(-bounds.x / 2f, bounds.y / 2f, 0);
      var uv1 = Vector2.up;

      var position2 = new Vector3(bounds.x / 2f, bounds.y / 2f, 0);
      var uv2 = Vector2.one;
      
      _triangle = new Triangle(position0, position1, position2, uv0, uv1, uv2);
    }

    protected virtual Vector3 GetClickPosition(Vector2 position)
    {
      return Camera.ScreenToWorldPoint(position);
    }

    public Vector2 GetScratchPosition(Vector2 position)
    {
      var scratchPosition = Vector2.zero;
      
      if (IsOrthographic)
      {
        var bounds = Bounds;
        var clickPosition = GetClickPosition(position);
        var lossyScale = Surface.lossyScale;
        var pointLocal = Vector2.Scale(Surface.InverseTransformPoint(clickPosition), lossyScale) + bounds / 2f;
        
        if (SpriteRect != null)
        {
          var uv = pointLocal / bounds;
          scratchPosition = new Vector2(SpriteRect.Value.width * uv.x, SpriteRect.Value.height * uv.y);
        }
        else
        {
          var textureSize = TextureSize;
          var pixelsPerInch = new Vector2(textureSize.x / bounds.x / lossyScale.x,
            textureSize.y / bounds.y / lossyScale.y);
          scratchPosition = Vector2.Scale(Vector2.Scale(pointLocal, lossyScale), pixelsPerInch);
        }
      }
      else
      {
        var plane = new Plane(Surface.forward, Surface.position);
        var ray = Camera.ScreenPointToRay(position);
        if (plane.Raycast(ray, out var enter))
        {
          var point = ray.GetPoint(enter);
          var pointLocal = Surface.InverseTransformPoint(point);
          var uv = _triangle.GetUV(pointLocal);
          scratchPosition = Vector2.Scale(TextureSize, uv);
        }
      }

      return scratchPosition;
    }
  }
}