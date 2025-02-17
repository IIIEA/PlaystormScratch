using UnityEngine;

namespace Scripts.Refactor
{
  public static class MeshFactory
  {
    public static Mesh CreateQuad()
    {
      return new Mesh
      {
        vertices = new[]
        {
          new Vector3(0, 1, 0),
          new Vector3(1, 1, 0),
          new Vector3(1, 0, 0),
          new Vector3(0, 0, 0)
        },
        uv = new[]
        {
          new Vector2(0, 1),
          new Vector2(1, 1),
          new Vector2(1, 0),
          new Vector2(0, 0)
        },
        triangles = new[] { 0, 1, 2, 2, 3, 0 },
        colors = new[]
        {
          Color.white,
          Color.white,
          Color.white,
          Color.white
        }
      };
    }
  }
}