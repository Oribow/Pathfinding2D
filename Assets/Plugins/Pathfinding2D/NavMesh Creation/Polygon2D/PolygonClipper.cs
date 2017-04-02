using UnityEngine;
using Utility.ExtensionMethods;
using System.Collections.Generic;
using System;
using NavGraph.Build;

namespace Utility.Polygon2D
{
    public static class PolygonClipper
    {
        static readonly float fudgeFactor = 0.00001f;

        public enum BoolOpType { UNION, DIFFERENCE };
        public enum ResultType { NoOverlap, SuccesfullyClipped, FullyContained, FullyContains };

        enum EdgeType { NORMAL, NON_CONTRIBUTING, SAME_TRANSITION, DIFFERENT_TRANSITION };
        enum PolygonType { SUBJECT, CLIPPING };

        public static ResultType Compute(Contour sp, Contour cp, BoolOpType op, out Contour[] result)
        {
            result = null;

            //Trivial case: At least one polygon is empty
            if (sp.ContainsNoVerts || cp.ContainsNoVerts)
            {
                Debug.Log("One, or both are empty. sp.IsEmpty = " + sp.ContainsNoVerts + ", cp.IsEmpty = " + cp.ContainsNoVerts);
                if (op == BoolOpType.DIFFERENCE)
                    result = new Contour[] { sp };
                else if (op == BoolOpType.UNION)
                    result = new Contour[] { (sp.ContainsNoVerts) ? cp : sp };
                //Return null for INTERSECTION and XOR operations;
                return ResultType.NoOverlap;
            }

            //Trivial case: The polygons cannot intersect each other.
            if (!sp.Bounds.Intersects(cp.Bounds))
            {
                if (op == BoolOpType.DIFFERENCE)
                    result = new Contour[] { sp };
                else if (op == BoolOpType.UNION)
                    result = new Contour[] { sp, cp };
                //Return null for INTERSECTION and XOR operations;
                return ResultType.NoOverlap;
            }

            //Init the event queue with the polygon edges
            //TODO: Better approximation then * 3
            HeapPriorityQueue<SweepEvent> eventQueue = new HeapPriorityQueue<SweepEvent>((sp.VertexCount + cp.VertexCount) * 3);
            InsertPolygon(eventQueue, sp, PolygonType.SUBJECT);
            InsertPolygon(eventQueue, cp, PolygonType.CLIPPING);

            SweepRay sweepRay = new SweepRay(20);
            SweepEvent cEvent;
            float minRightBounds = Math.Min(sp.Bounds.max.x, cp.Bounds.max.x);
            bool changesMade = false;
            Connector connector = new Connector(10);

            while (eventQueue.Count != 0)
            {
                cEvent = eventQueue.Dequeue();

                if (op == BoolOpType.DIFFERENCE && cEvent.p.x > sp.Bounds.max.x + fudgeFactor)
                {
                    //Exit the loop. No more intersections are to be found.
                    // Create a polygon out of the pointchain
                    result = connector.ToArray();
                    return EvaluateResult(op, changesMade, result, sp, cp);
                }
                if (op == BoolOpType.UNION && cEvent.p.x > minRightBounds + fudgeFactor)
                {
                    if (!cEvent.left)
                    {
                        if (cEvent.wrapWiseLeft) connector.Add(cEvent.p, cEvent.other.p); else connector.Add(cEvent.other.p, cEvent.p);
                    }
                    while (eventQueue.Count != 0)
                    {
                        cEvent = eventQueue.Dequeue();
                        if (!cEvent.left)
                        {
                            if (cEvent.wrapWiseLeft) connector.Add(cEvent.p, cEvent.other.p); else connector.Add(cEvent.other.p, cEvent.p);
                        }
                    }
                    result = connector.ToArray();
                    return EvaluateResult(op, changesMade, result, sp, cp);
                }

                if (cEvent.left)
                {
                    // the line segment must be inserted into S
                    int pos = sweepRay.Add(cEvent);
                    SweepEvent prev = sweepRay.Previous(pos);
                    if (prev == null)
                    {
                        cEvent.inside = false;
                        cEvent.inOut = false;
                    }
                    else if (prev.type != EdgeType.NORMAL)
                    {
                        if (pos - 2 < 0)
                        {
                            cEvent.inside = true;
                            cEvent.inOut = false;
                        }
                        else
                        {
                            SweepEvent sliEvent = sweepRay.Previous(pos - 1);
                            if (prev.pl == cEvent.pl)
                            {
                                cEvent.inOut = !prev.inOut;
                                cEvent.inside = !sliEvent.inOut;
                            }
                            else
                            {
                                cEvent.inOut = !sliEvent.inOut;
                                cEvent.inside = !prev.inOut;
                            }
                        }
                    }
                    else if (cEvent.pl == prev.pl)
                    {
                        // previous line segment in S belongs to the same polygon that "cEvent" belongs to
                        cEvent.inside = prev.inside;
                        cEvent.inOut = !prev.inOut;
                    }
                    else
                    {
                        // previous line segment in S belongs to a different polygon that "cEvent" belongs to
                        cEvent.inside = !prev.inOut;
                        cEvent.inOut = prev.inside;
                    }

                    SweepEvent nextEvent = sweepRay.Next(pos);
                    if (nextEvent != null)
                        HandlePossibleIntersection(eventQueue, cEvent, nextEvent, ref changesMade);
                    if (prev != null)
                        HandlePossibleIntersection(eventQueue, cEvent, prev, ref changesMade);
                }
                else
                {// the line segment must be removed from S
                    int pos = sweepRay.Find(cEvent.other);
                    switch (cEvent.type)
                    {
                        case (EdgeType.NORMAL):
                            switch (op)
                            {
                                case (BoolOpType.UNION):
                                    if (!cEvent.other.inside)
                                        if (cEvent.wrapWiseLeft) connector.Add(cEvent.p, cEvent.other.p); else connector.Add(cEvent.other.p, cEvent.p);
                                    break;
                                case (BoolOpType.DIFFERENCE):
                                    if (((cEvent.pl == PolygonType.SUBJECT) && !cEvent.other.inside) || (cEvent.pl == PolygonType.CLIPPING && cEvent.other.inside))
                                        if (cEvent.wrapWiseLeft) connector.Add(cEvent.p, cEvent.other.p); else connector.Add(cEvent.other.p, cEvent.p);
                                    break;
                            }
                            break;
                        case (EdgeType.SAME_TRANSITION):
                            if (op == BoolOpType.UNION)
                                if (cEvent.wrapWiseLeft) connector.Add(cEvent.p, cEvent.other.p); else connector.Add(cEvent.other.p, cEvent.p);
                            break;
                        case (EdgeType.DIFFERENT_TRANSITION):
                            if (op == BoolOpType.DIFFERENCE)
                                if (cEvent.wrapWiseLeft) connector.Add(cEvent.p, cEvent.other.p); else connector.Add(cEvent.other.p, cEvent.p);
                            break;
                    }
                    // delete line segment associated to e from S and check for intersection between the neighbors of "e" in S
                    SweepEvent next = sweepRay.Next(pos), prev = sweepRay.Previous(pos);
                    sweepRay.RemoveAt(pos);
                    if (next != null && prev != null)
                        HandlePossibleIntersection(eventQueue, prev, next, ref changesMade);
                }
            }
            result = connector.ToArray();
            return EvaluateResult(op, changesMade, result, sp, cp);
        }

        private static void InsertPolygon(HeapPriorityQueue<SweepEvent> eventQueue, Contour contour, PolygonType pType)
        {
            SweepEvent se1;
            SweepEvent se2;
            Vector2 cVal;
            Vector2 cPrevVal = contour[contour.VertexCount - 1];
            for (int iVert = 0; iVert < contour.VertexCount; iVert++)
            {
                cVal = contour[iVert];
                if (cVal == cPrevVal)
                    return;

                if (cVal.x < cPrevVal.x)
                {
                    se1 = new SweepEvent(cVal, true, false, pType);
                    se2 = new SweepEvent(cPrevVal, false, true, pType);
                }
                else if (cVal.x > cPrevVal.x)
                {
                    se1 = new SweepEvent(cVal, false, false, pType);
                    se2 = new SweepEvent(cPrevVal, true, true, pType);
                }
                else if (cVal.y < cPrevVal.y)
                {
                    se1 = new SweepEvent(cVal, true, false, pType);
                    se2 = new SweepEvent(cPrevVal, false, true, pType);
                }
                else
                {
                    se1 = new SweepEvent(cVal, false, false, pType);
                    se2 = new SweepEvent(cPrevVal, true, true, pType);
                }
                se1.other = se2;
                se2.other = se1;
                eventQueue.Enqueue(se1);
                eventQueue.Enqueue(se2);
                cPrevVal = cVal;
            }
        }

        private static int FindIntersection(SweepEvent se1, SweepEvent se2, out Vector2 pA, out Vector2 pB)
        {
            //Assign the resulting points some dummy values
            pA = Vector2.zero;
            pB = Vector2.zero;
            Vector2 se1_Begin = (se1.left) ? se1.p : se1.other.p;
            Vector2 se1_End = (se1.left) ? se1.other.p : se1.p;
            Vector2 se2_Begin = (se2.left) ? se2.p : se2.other.p;
            Vector2 se2_End = (se2.left) ? se2.other.p : se2.p;

            Vector2 d0 = se1_End - se1_Begin;
            Vector2 d1 = se2_End - se2_Begin;
            Vector2 e = se2_Begin - se1_Begin;

            float sqrEpsilon = 0.0000001f; // 0.001 before

            float kross = d0.x * d1.y - d0.y * d1.x;
            float sqrKross = kross * kross;
            float sqrLen0 = d0.sqrMagnitude;
            float sqrLen1 = d1.sqrMagnitude;

            if (sqrKross > sqrEpsilon * sqrLen0 * sqrLen1)
            {
                // lines of the segments are not parallel
                float s = (e.x * d1.y - e.y * d1.x) / kross;
                if ((s < 0) || (s > 1))
                {
                    return 0;
                }
                float t = (e.x * d0.y - e.y * d0.x) / kross;
                if ((t < 0) || (t > 1))
                {
                    return 0;
                }
                // intersection of lines is a point an each segment
                pA = se1_Begin + s * d0;
                if (ApproximatelyEqual(pA, se1_Begin)) pA = se1_Begin;
                if (ApproximatelyEqual(pA, se1_End)) pA = se1_End;
                if (ApproximatelyEqual(pA, se2_Begin)) pA = se2_Begin;
                if (ApproximatelyEqual(pA, se2_End)) pA = se2_End;
                return 1;
            }

            // lines of the segments are parallel
            float sqrLenE = e.sqrMagnitude;
            kross = e.x * d0.y - e.y * d0.x;
            sqrKross = kross * kross;
            if (sqrKross > sqrEpsilon * sqrLen0 * sqrLenE)
            {
                // lines of the segment are different
                return 0;
            }

            // Lines of the segments are the same. Need to test for overlap of segments.
            float s0 = (d0.x * e.x + d0.y * e.y) / sqrLen0;  // so = Dot (D0, E) * sqrLen0
            float s1 = s0 + (d0.x * d1.x + d0.y * d1.y) / sqrLen0;  // s1 = s0 + Dot (D0, D1) * sqrLen0
            float smin = Mathf.Min(s0, s1);
            float smax = Mathf.Max(s0, s1);
            float[] w = new float[2];
            int imax = FindIntersection(0.0f, 1.0f, smin, smax, w);

            if (imax > 0)
            {
                pA = se1_Begin + w[0] * d0;
                if (ApproximatelyEqual(pA, se1_Begin)) pA = se1_Begin;
                if (ApproximatelyEqual(pA, se1_End)) pA = se1_End;
                if (ApproximatelyEqual(pA, se2_Begin)) pA = se2_Begin;
                if (ApproximatelyEqual(pA, se2_End)) pA = se2_End;
                if (imax > 1)
                {
                    pB = se1_Begin + w[1] * d0;
                }
            }
            return imax;
        }

        private static int FindIntersection(float u0, float u1, float v0, float v1, float[] w)
        {
            if ((u1 < v0) || (u0 > v1))
                return 0;
            if (u1 > v0)
            {
                if (u0 < v1)
                {
                    w[0] = (u0 < v0) ? v0 : u0;
                    w[1] = (u1 > v1) ? v1 : u1;
                    return 2;
                }
                else
                {
                    // u0 == v1
                    w[0] = u0;
                    return 1;
                }
            }
            else
            {
                // u1 == v0
                w[0] = u1;
                return 1;
            }
        }

        private static void HandlePossibleIntersection(HeapPriorityQueue<SweepEvent> eventQueue, SweepEvent e1, SweepEvent e2, ref bool changesMade)
        {

            Vector2 ip1, ip2;  // intersection points
            int nintersections;

            if ((nintersections = FindIntersection(e1, e2, out ip1, out ip2)) == 0)
                return;

            if ((nintersections == 1) && (e1.p == e2.p || e1.other.p == e2.other.p))
            {
                //Debug.Log(" The line segments intersect at an endpoint of both line segments \n" + e1.ToString() + "\n" + e2.ToString());
                return; // the line segments intersect at an endpoint of both line segments
            }

            if (nintersections == 2 && e1.pl == e2.pl)
            {
                return; // the line segments overlap, but they belong to the same polygon
            }

            changesMade = true;
            // The line segments associated to e1 and e2 intersect
            if (nintersections == 1)
            {
                if (e1.p != ip1 && e1.other.p != ip1)  // if ip1 is not an endpoint of the line segment associated to e1 then divide "e1"
                    DivideEdge(eventQueue, e1, ip1);
                if (e2.p != ip1 && e2.other.p != ip1)  // if ip1 is not an endpoint of the line segment associated to e2 then divide "e2"
                    DivideEdge(eventQueue, e2, ip1);
                return;
            }

            // The line segments overlap
            List<SweepEvent> sortedEvents = new List<SweepEvent>(4);
            if (e1.p == e2.p)
            {
                sortedEvents.Add(null);
            }
            else if (e1.CompareTo(e2) > 0)
            {
                sortedEvents.Add(e2);
                sortedEvents.Add(e1);
            }
            else
            {
                sortedEvents.Add(e1);
                sortedEvents.Add(e2);
            }

            if (e1.other.p == e2.other.p)
            {
                sortedEvents.Add(null);
            }
            else if (e1.other.CompareTo(e2.other) > 0)
            {
                sortedEvents.Add(e2.other);
                sortedEvents.Add(e1.other);
            }
            else
            {
                sortedEvents.Add(e1.other);
                sortedEvents.Add(e2.other);
            }

            if (sortedEvents.Count == 2)
            { 
                // are both line segments equal?
                e1.type = e1.other.type = EdgeType.NON_CONTRIBUTING;
                e2.type = e2.other.type = (e1.inOut == e2.inOut) ? EdgeType.SAME_TRANSITION : EdgeType.DIFFERENT_TRANSITION;
                return;
            }
            if (sortedEvents.Count == 3)
            { 
                // the line segments share an endpoint
                sortedEvents[1].type = sortedEvents[1].other.type = EdgeType.NON_CONTRIBUTING;
                if (sortedEvents[0] != null)         // is the right endpoint the shared point?
                    sortedEvents[0].other.type = (e1.inOut == e2.inOut) ? EdgeType.SAME_TRANSITION : EdgeType.DIFFERENT_TRANSITION;
                else                                // the shared point is the left endpoint
                    sortedEvents[2].other.type = (e1.inOut == e2.inOut) ? EdgeType.SAME_TRANSITION : EdgeType.DIFFERENT_TRANSITION;
                DivideEdge(eventQueue, sortedEvents[0] != null ? sortedEvents[0] : sortedEvents[2].other, sortedEvents[1].p);
                return;
            }
            if (sortedEvents[0] != sortedEvents[3].other)
            { 
                // no line segment includes totally the other one
                sortedEvents[1].type = EdgeType.NON_CONTRIBUTING;
                sortedEvents[2].type = (e1.inOut == e2.inOut) ? EdgeType.SAME_TRANSITION : EdgeType.DIFFERENT_TRANSITION;
                DivideEdge(eventQueue, sortedEvents[0], sortedEvents[1].p);
                DivideEdge(eventQueue, sortedEvents[1], sortedEvents[2].p);
                return;
            }
            // one line segment includes the other one
            sortedEvents[1].type = sortedEvents[1].other.type = EdgeType.NON_CONTRIBUTING;
            DivideEdge(eventQueue, sortedEvents[0], sortedEvents[1].p);
            sortedEvents[3].other.type = (e1.inOut == e2.inOut) ? EdgeType.SAME_TRANSITION : EdgeType.DIFFERENT_TRANSITION;
            DivideEdge(eventQueue, sortedEvents[3].other, sortedEvents[2].p);
        }

        private static void DivideEdge(HeapPriorityQueue<SweepEvent> eventQueue, SweepEvent e, Vector2 p)
        {
            // "Right event" of the "left line segment" resulting from dividing e (the line segment associated to e)
            SweepEvent r = new SweepEvent(p, false, e.other.wrapWiseLeft, e.pl, e.type);
            r.other = e;

            // "Left event" of the "right line segment" resulting from dividing e (the line segment associated to e)
            SweepEvent l = new SweepEvent(p, true, e.wrapWiseLeft, e.pl, e.other.type);
            l.other = e.other;

            if (l.CompareTo(e.other) > 0)
            { 
                // avoid a rounding error. The left event would be processed after the right event
                Debug.LogWarning("Oops");
                e.other.left = true;
                l.left = false;
            }
            if (e.CompareTo(r) > 0)
            { 
                // avoid a rounding error. The left event would be processed after the right event
                Debug.LogWarning("Oops2");
            }
            e.other.other = l;
            e.other = r;
            eventQueue.Enqueue(r);
            eventQueue.Enqueue(l);
        }

        private static ResultType EvaluateResult(BoolOpType op, bool edgesIntersect, Contour[] result, Contour sp, Contour cp)
        {

            switch (op)
            {
                case BoolOpType.DIFFERENCE:
                    if (edgesIntersect)
                        return ResultType.SuccesfullyClipped;
                    else if (result.Length == 1)
                    {
                        return ResultType.NoOverlap;
                    }
                    if (sp.Bounds.ContainsBounds(cp.Bounds))
                    {
                        return ResultType.FullyContains;
                    }
                    else
                        return ResultType.FullyContained;
                case BoolOpType.UNION:
                    if (edgesIntersect)
                    {
                        return ResultType.SuccesfullyClipped;
                    }
                    else if (result.Length > 1)
                    {
                        return ResultType.NoOverlap;
                    }
                    Bounds expandedBoundsSP = sp.Bounds;
                    expandedBoundsSP.Expand(new Vector3(fudgeFactor, fudgeFactor, 0));
                    if (result[0].Bounds.ContainsBounds(expandedBoundsSP))
                    {
                        return ResultType.FullyContained;
                    }
                    else
                        return ResultType.FullyContains;
            }
            //Should never come here
            throw new System.Exception("Unknown operation type: " + op);
        }

        public static bool ApproximatelyEqual(Vector2 v1, Vector2 v2)
        {
            return (v1 - v2).sqrMagnitude < 0.0000000001f;
        }

        public static bool Approximately(float f1, float f2)
        {
            return Math.Abs(f1 - f2) < 0.00001f;
        }

        class SweepEvent : PriorityQueueNode
        {
            public Vector2 p;           // point associated with the event
            public bool left;         // is the point the left endpoint of the segment (p, other.p)?
            public PolygonType pl;    // Polygon to which the associated segment belongs to
            public SweepEvent other; // Event associated to the other endpoint of the segment
                                     /**  Does the segment (p, other.p) represent an inside-outside transition in the polygon for a vertical ray from (p.x, -infinite) that crosses the segment? */
            public bool inOut;// { get { return wrapWiseLeft != left; } }
            public EdgeType type;
            public bool inside; // Only used in "left" events. Is the segment (p, other.p) inside the other polygon?

            public bool wrapWiseLeft;

            /** Class constructor */
            public SweepEvent(Vector2 point, bool left, bool wrapWiseLeft, PolygonType pTyp, EdgeType t = EdgeType.NORMAL)
            {
                p = point;
                this.left = left;
                this.wrapWiseLeft = wrapWiseLeft;
                pl = pTyp;
                type = t;
            }

            /** Is the line segment (p, other.p) below point x */
            public bool IsBelow(Vector2 o) { return (left) ? ExtendedGeometry.SignedAreaDoubledTris(p, other.p, o) > 0 : ExtendedGeometry.SignedAreaDoubledTris(other.p, p, o) > 0; }
            /** Is the line segment (p, other.p) above point x */
            public bool IsAbove(Vector2 o) { return !IsBelow(o); }

            // Return true(1) means that e1 is placed at the event queue after e2, i.e,, e1 is processed by the algorithm after e2
            // -1|0|1
            public override int CompareTo(PriorityQueueNode other)
            {
                if (other.GetType() == typeof(SweepEvent))
                {
                    SweepEvent so = (SweepEvent)other;
                    if (p.x < so.p.x)
                    {
                        return -1;
                    }
                    else if (p.x > so.p.x)
                    {
                        return 1;
                    }
                    else if (p != so.p)
                    {
                        if (p.y < so.p.y)
                            return -1;
                        return 1;
                    }
                    if (left != so.left)
                    {
                        if (left)
                            return 1;
                        return -1;
                    }
                    if (IsAbove(so.other.p))
                        return 1;
                    return -1;
                }
                return base.CompareTo(other);

            }

            public static bool operator ==(SweepEvent se1, SweepEvent se2)
            {
                if (object.ReferenceEquals(se1, null))
                    return object.ReferenceEquals(se2, null);
                if (object.ReferenceEquals(se2, null))
                    return object.ReferenceEquals(se1, null);
                return (se1.p == se2.p && se1.left == se2.left && se1.other.p == se2.other.p && se1.pl == se2.pl);
            }

            public static bool operator !=(SweepEvent se1, SweepEvent se2)
            {
                return !(se1 == se2);
            }

            public override string ToString()
            {
                return "SE (p = " + p + ", l = " + left + ", pl = " + pl + ", inOut = " + ((left) ? inOut : other.inOut) + ", inside = " + ((left) ? inside : other.inside) + ", other.p = " + other.p + ")";
            }
        }

        class SweepRay
        {
            public List<SweepEvent> s;

            public SweepRay(int capacity)
            {
                s = new List<SweepEvent>(capacity);
            }

            public int Add(SweepEvent e)
            {
                for (int i = 0; i < s.Count; i++)
                {
                    SweepEvent se = s[i];
                    if (IsEventOneMoreImportant(se, e))
                        continue;
                    s.Insert(i, e);
                    return i;
                }
                s.Add(e);
                return s.Count - 1;
            }

            public int Find(SweepEvent e)
            {
                return s.IndexOf(e);
            }

            public void RemoveAt(int index)
            {
                s.RemoveAt(index);
            }

            public SweepEvent Next(int index)
            {
                index++;
                if (index < s.Count)
                    return s[index];
                return null;
            }

            public SweepEvent Previous(int index)
            {
                index--;
                if (index >= 0)
                    return s[index];
                return null;
            }

            public override string ToString()
            {
                string result = "[" + s.Count + "] ";
                foreach (SweepEvent se in s)
                    result += ((se.left) ? "l" : "r") + se.ToString() + ", ";
                return result;
            }

            private bool IsEventOneMoreImportant(SweepEvent se1, SweepEvent se2)
            {
                if (se1 == se2)
                    return false;

                if (ExtendedGeometry.SignedAreaDoubledTris(se1.p, se1.other.p, se2.p) != 0 || ExtendedGeometry.SignedAreaDoubledTris(se1.p, se1.other.p, se2.other.p) != 0)
                {
                    // Segments are not collinear
                    // If they share their left endpoint use the right endpoint to sort
                    if (se1.p == se2.p)
                        return se1.IsBelow(se2.other.p);

                    if (se1.CompareTo(se2) < 0)// has the segment associated to e1 been sorted in evp before the segment associated to e2?
                        return se1.IsBelow(se2.p);
                    // The segment associated to e2 has been sorted in evp before the segment associated to e1
                    return se2.IsAbove(se1.p);
                }
                // Segments are collinear. Just a consistent criterion is used
                if (se1.p == se2.p)
                    return se1.GetHashCode() < se2.GetHashCode(); //Not sure here. Seems like lines exactly overlap each other. Didnt found the < operator though.

                return se1.CompareTo(se2) < 0;
            }
        }
    }
}