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
    }
}
