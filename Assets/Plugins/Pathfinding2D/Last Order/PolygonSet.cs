using UnityEngine;
using System.Collections.Generic;
using System;
using ClipperLib;

#if use_int32
  using cInt = Int32;
#else
using cInt = System.Int64;
#endif

namespace NavGraph.Build
{
    /// <summary>
    /// A container that contains a list of polygons and edges, which where probably but not necessarily collected from Colliders.
    /// </summary>
    public class MergedPolygonSet
    {
        public List<Polygon> Polygons { get; }

        Clipper clipper;

        public MergedPolygonSet()
        {
            Polygons = new List<Polygon>(50);

            clipper = new Clipper();
            clipper.ReverseSolution = true;
        }

        private void Add(Polygon newPoly)
        {
            // try to merge the new polygon with the existing once
            for (int iPoly = 0; iPoly < Polygons.Count; iPoly++)
            {
                var poly = Polygons[iPoly];
                // simple bounds check first
                if (!poly.BoundingRect.Overlaps(newPoly.BoundingRect))
                {
                    continue;
                }

                clipper.Clear();
                newPoly.AddToClipper(clipper, PolyType.ptClip);
                poly.AddToClipper(clipper, PolyType.ptSubject);

                PolyTree polyTree = new PolyTree();
                clipper.Execute(ClipType.ctUnion, polyTree);

                if (polyTree.ChildCount > 1)
                {
                    // bounds overlap, but no actual intersection is happening
                    continue;
                }
                var hullNode = polyTree.Childs[0];
                bool holeNodeHasChild = false;
                foreach (var holeNode in hullNode.Childs)
                {
                    if (holeNode.ChildCount != 0)
                    {
                        holeNodeHasChild = true;
                        break;
                    }
                }
                if (holeNodeHasChild)
                {
                    continue;
                }

                // tree can now contains 1 outer polygon and any number of holes.
                // we cant know which outer polygon corresponses to the outer polygon

                //overwrite newPoly

                newPoly.hull.SetVerticies(hullNode.Contour);

                newPoly.holes = new List<Contour>(hullNode.ChildCount);
                foreach (var holeNode in hullNode.Childs)
                {
                    newPoly.holes.Add(new Contour(holeNode.Contour));
                }
                newPoly.UpdateBounds();

                // delete poly
                if (iPoly < Polygons.Count - 1)
                {
                    Polygons[iPoly] = Polygons[Polygons.Count - 1];
                }
                Polygons.RemoveAt(Polygons.Count - 1);
                iPoly--;
            }
            Polygons.Add(newPoly);
        }

        private void Remove(Polygon polyToRemove)
        {
            // try to merge the new polygon with the existing once
            int oldPolyCount = Polygons.Count;
            for (int iPoly = 0; iPoly < oldPolyCount; iPoly++)
            {
                var poly = Polygons[iPoly];
                // simple bounds check first
                if (!poly.BoundingRect.Overlaps(polyToRemove.BoundingRect))
                {
                    continue;
                }

                clipper.Clear();
                polyToRemove.AddToClipper(clipper, PolyType.ptClip);
                poly.AddToClipper(clipper, PolyType.ptSubject);

                PolyTree polyTree = new PolyTree();
                clipper.Execute(ClipType.ctDifference, polyTree);

                var hullNode = polyTree.Childs[0];

                // tree can now contains 1 outer polygon and any number of holes.
                // we cant know which outer polygon corresponses to the outer polygon

                //overwrite newPoly

                newPoly.hull.SetVerticies(hullNode.Contour);

                newPoly.holes = new List<Contour>(hullNode.ChildCount);
                foreach (var holeNode in hullNode.Childs)
                {
                    newPoly.holes.Add(new Contour(holeNode.Contour));
                }
                newPoly.UpdateBounds();

                // delete poly
                if (iPoly < Polygons.Count - 1)
                {
                    Polygons[iPoly] = Polygons[Polygons.Count - 1];
                }
                Polygons.RemoveAt(Polygons.Count - 1);
                iPoly--;
            }
            Polygons.Add(newPoly);
        }

        public IEnumerable<Contour> EnumerateContours()
        {
            for (int iPoly = 0; iPoly < Polygons.Count; iPoly++)
            {
                var poly = Polygons[iPoly];
                yield return poly.hull;
                for (int iHole = 0; iHole < poly.holes.Count; iHole++)
                {
                    yield return poly.holes[iHole];
                }
            }
        }

    }
}
