using UnityEngine;

namespace Scripts.Core.ScratchData
{
    public class Triangle
    {
        private readonly Vector3 _v0;
        private readonly Vector3 _v1;
        private readonly Vector3 _v2;
        private readonly Vector2 _uv0;
        private readonly Vector2 _uv1;
        private readonly Vector2 _uv2;

        public Triangle(Vector3 vertex0, Vector3 vertex1, Vector3 vertex2, Vector2 uv0, Vector2 uv1, Vector2 uv2)
        {
            _v0 = vertex0;
            _v1 = vertex1;
            _v2 = vertex2;
            
            _uv0 = uv0;
            _uv1 = uv1;
            _uv2 = uv2;
        }

        public Vector2 GetUV(Vector3 point)
        {
            var distance0 = _v0 - point;
            var distance1 = _v1 - point;
            var distance2 = _v2 - point;

            var va = Vector3.Cross(_v0 - _v1, _v0 - _v2);
            var va1 = Vector3.Cross(distance1, distance2);
            var va2 = Vector3.Cross(distance2, distance0);
            var va3 = Vector3.Cross(distance0, distance1);
            
            var area = va.magnitude;

            var a1 = va1.magnitude / area * Mathf.Sign(Vector3.Dot(va, va1));
            var a2 = va2.magnitude / area * Mathf.Sign(Vector3.Dot(va, va2));
            var a3 = va3.magnitude / area * Mathf.Sign(Vector3.Dot(va, va3));
            
            var uv = _uv0 * a1 + _uv1 * a2 + _uv2 * a3;
            
            return uv;
        }
    }
}