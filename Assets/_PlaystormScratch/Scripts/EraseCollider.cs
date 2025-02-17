using System.Collections.Generic;
using ClipperLib;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Scripts
{
  public class EraseCollider : MonoBehaviour
  {
    public float _eraseRadius = 0.5f;
    public int _resolution = 20;
    private PolygonCollider2D _polygonCollider;
    private Vector2 _previousMousePosition;

    private void Start()
    {
      _polygonCollider = GetComponent<PolygonCollider2D>();
      _previousMousePosition = GetMouseWorldPosition();
    }

    private void Update()
    {
      if (Mouse.current.leftButton.isPressed)
      {
        Vector2 currentMousePosition = GetMouseWorldPosition();
        InterpolateAndErase(_previousMousePosition, currentMousePosition);

        _previousMousePosition = currentMousePosition;
      }
      else
      {
        _previousMousePosition = GetMouseWorldPosition();
      }
    }

    private void InterpolateAndErase(Vector2 start, Vector2 end)
    {
      float distance = Vector2.Distance(start, end);
      int steps = Mathf.CeilToInt(distance / (_eraseRadius * 0.5f));

      for (int i = 0; i < steps; i++)
      {
        float t = (float)i / steps;
        Vector2 interpolatedPosition = Vector2.Lerp(start, end, t);

        Vector2 localPosition = transform.InverseTransformPoint(interpolatedPosition);

        ModifyCollider(localPosition);
      }
    }

    private void ModifyCollider(Vector2 position)
    {
      List<Vector2[]> paths = new List<Vector2[]>();

      for (int i = 0; i < _polygonCollider.pathCount; i++)
      {
        paths.Add(_polygonCollider.GetPath(i));
      }

      Vector2[] eraseCircle = CreateCirclePoints(position, _eraseRadius, _resolution);
      List<Vector2[]> newPaths = SubtractShapeFromPolygon(paths, eraseCircle);

      _polygonCollider.pathCount = newPaths.Count;

      for (int i = 0; i < newPaths.Count; i++)
      {
        _polygonCollider.SetPath(i, newPaths[i]);
      }
    }

    private Vector2[] CreateCirclePoints(Vector2 center, float radius, int resolution)
    {
      var createCircleJob = new CreateCircleJob
      {
        Center = new float2(center.x, center.y),
        Radius = radius,
        Resolution = resolution,
        Points = new NativeArray<float2>(resolution, Allocator.TempJob)
      };

      createCircleJob.Schedule().Complete();

      Vector2[] circlePoints = new Vector2[resolution];

      for (int i = 0; i < resolution; i++)
      {
        circlePoints[i] = new Vector2(createCircleJob.Points[i].x, createCircleJob.Points[i].y);
      }

      createCircleJob.Points.Dispose();

      return circlePoints;
    }

    private List<Vector2[]> SubtractShapeFromPolygon(List<Vector2[]> paths, Vector2[] shape)
    {
      Clipper clipper = new Clipper();

      foreach (var path in paths)
      {
        List<IntPoint> intPath = ConvertToIntPoints(path);
        clipper.AddPath(intPath, PolyType.ptSubject, true);
      }

      List<IntPoint> shapePoints = ConvertToIntPoints(shape);
      clipper.AddPath(shapePoints, PolyType.ptClip, true);

      List<List<IntPoint>> solution = new List<List<IntPoint>>();
      clipper.Execute(ClipType.ctDifference, solution);

      List<Vector2[]> newPaths = new List<Vector2[]>();

      foreach (var path in solution)
      {
        newPaths.Add(ConvertFromIntPoints(path));
      }

      return newPaths;
    }

    private List<IntPoint> ConvertToIntPoints(Vector2[] points)
    {
      List<IntPoint> intPoints = new List<IntPoint>();

      foreach (Vector2 point in points)
      {
        intPoints.Add(new IntPoint((long)(point.x * 1000), (long)(point.y * 1000)));
      }

      return intPoints;
    }

    private Vector2[] ConvertFromIntPoints(List<IntPoint> intPoints)
    {
      Vector2[] points = new Vector2[intPoints.Count];

      for (int i = 0; i < intPoints.Count; i++)
      {
        points[i] = new Vector2((float)intPoints[i].X / 1000, (float)intPoints[i].Y / 1000);
      }

      return points;
    }

    private Vector2 GetMouseWorldPosition()
    {
      return Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    }

    [BurstCompile]
    private struct CreateCircleJob : IJob
    {
      public float2 Center;
      public float Radius;
      public int Resolution;
      public NativeArray<float2> Points;

      public void Execute()
      {
        float angleStep = 360f / Resolution;
        for (int i = 0; i < Resolution; i++)
        {
          float angle = i * angleStep * math.PI / 180f;
          Points[i] = Center + new float2(math.cos(angle), math.sin(angle)) * Radius;
        }
      }
    }
  }
}