using ClipperLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ABC
{
    public static class Utility
    {
        public static IntRect CalculateBoundingRect(List<IntPoint> polygon)
        {
            if (polygon.Count == 0)
            {
                return new IntRect(0, 0, 0, 0);
            }

            IntPoint min = polygon[0], max = polygon[0];
            foreach (var vert in polygon)
            {
                min.x = Math.Min(min.x, vert.x);
                min.y = Math.Min(min.y, vert.y);

                max.x = Math.Max(max.x, vert.x);
                max.y = Math.Max(max.y, vert.y);
            }
            return new IntRect(min.x, max.y, max.x, min.y);
        }

        public static Vector2 QuadraticBezierCurve(float t, Vector2 a, Vector2 b, Vector2 c)
        {
            return (1 - t) * (1 - t) * a + 2 * (1 - t) * t * b + t * t * c;
        }

        public static void DrawArrow(Vector2 pos, Vector2 dir)
        {
            Vector2 normal = new Vector2(-dir.y, dir.x).normalized * 0.1f;
            Gizmos.DrawLine(pos - normal, pos + normal);
            Gizmos.DrawLine(pos - normal, pos + dir);
            Gizmos.DrawLine(pos + normal, pos + dir);
        }
    }
}
