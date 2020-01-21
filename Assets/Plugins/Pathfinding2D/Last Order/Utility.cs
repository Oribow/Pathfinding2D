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
            Vector2 normal = new Vector2(-dir.y, dir.x).normalized * 0.2f;
            Gizmos.DrawLine(pos - normal, pos + normal);
            Gizmos.DrawLine(pos - normal, pos + dir);
            Gizmos.DrawLine(pos + normal, pos + dir);
        }

        public static void DrawBezierConnection(Vector2 start, Vector2 end, bool biDirectional)
        {
            Vector2 cp;
            var tangent = (end - start);
            var normal = new Vector2(-tangent.y, tangent.x).normalized;
            cp = start + tangent * 0.5f + normal;

            Vector2 prev = start;
            const float numberOfSegments = 5;
            for (float t = 1; t <= numberOfSegments; t++)
            {
                Vector2 v = QuadraticBezierCurve(t / numberOfSegments, start, cp, end);
                Gizmos.DrawLine(prev, v);
                prev = v;
            }

            //draw arrows
            Vector2 p = end + (QuadraticBezierCurve((numberOfSegments - 1) / numberOfSegments, start, cp, end) - end).normalized * 0.3f;
            ABC.Utility.DrawArrow(p, end - p);
            if (biDirectional)
            {
                p = start + (QuadraticBezierCurve(1 / numberOfSegments, start, cp, end) - start).normalized * 0.3f;
                DrawArrow(p, start - p);
            }
        }

        public static Color LinearBlendFromGreenToYellowToRed(float value)
        {
            if (value < 1)
            {
                return Color.Lerp(Color.green, Color.yellow, value);
            }
                return Color.Lerp(Color.yellow, Color.red, value - 1);
        }
    }
}
