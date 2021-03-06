﻿using UnityEngine;
using System.Collections.Generic;
using System;
using Utility.Polygon2D;
using Utility;

namespace NavGraph.Build
{
    [Serializable]
    public class ContourNode : ICloneable
    { 
        public Contour contour;
        public List<ContourNode> children;
        public bool IsSolid { get { return isSolid; } }

        [SerializeField]
        private bool isSolid;

        public ContourNode(Contour contour, bool isSolid)
        {
            this.contour = contour;
            children = new List<ContourNode>();
            this.isSolid = isSolid;
        }

        public void AddSolidContour(Contour other, ref bool wasContourUsed)
        {
            if (children.Count != 0)
            {
                ContourNode cOther = new ContourNode(other, true);
                int lastMergedWith = -1;
                int oldChildCount = children.Count;
                for (int iChild = 0; iChild < oldChildCount; iChild++)
                {
                    ContourNode cNode = children[iChild];
                    Contour[] clippingResult;
                    PolygonClipper.ResultType resultType = PolygonClipper.Compute(cNode.contour, cOther.contour, (cNode.isSolid) ? PolygonClipper.BoolOpType.UNION : PolygonClipper.BoolOpType.DIFFERENCE, out clippingResult);

                    if (resultType == PolygonClipper.ResultType.NoOverlap)
                        continue;

                    if (resultType == PolygonClipper.ResultType.FullyContained)
                    {
                        bool tmpWasContourUsed = false;
                        cOther.AddSolidContour(cNode.contour, ref tmpWasContourUsed);
                        children.RemoveAt(iChild);
                        iChild--;
                        oldChildCount--;
                        continue;
                    }

                    if (resultType == PolygonClipper.ResultType.FullyContains)
                    {
                        if (cNode.isSolid)
                        {
                            wasContourUsed = true;
                            if (cNode.children.Count == 0)
                                return;
                        }
                        cNode.AddSolidContour(other, ref wasContourUsed);
                        break;
                    }

                    wasContourUsed = true;

                    //SuccesfullyClipped
                    if (cNode.isSolid)
                    {
                        cNode.AddSolidContour(other, ref wasContourUsed);
                        cOther.AddSolidContour(cNode.contour, ref wasContourUsed);
                        for (int iResult = 0; iResult < clippingResult.Length; iResult++)
                        {
                            if (clippingResult[iResult].IsSolid())
                            {
                                cNode.contour = clippingResult[iResult];
                            }
                            else
                            {
                                //Hole creation
                                ContourNode holeNode = new ContourNode(clippingResult[iResult], false);
                                for (int iPrevChild = 0; iPrevChild < iChild; iPrevChild++)
                                {
                                    Contour[] holeClippingResult;
                                    PolygonClipper.ResultType holeResultType = PolygonClipper.Compute(holeNode.contour, children[iPrevChild].contour, PolygonClipper.BoolOpType.DIFFERENCE, out holeClippingResult);
                                    if (holeResultType == PolygonClipper.ResultType.FullyContains)
                                    {
                                        holeNode.children.Add(children[iPrevChild]);
                                        children.RemoveAt(iPrevChild);
                                        iPrevChild--;
                                        iChild--;
                                        oldChildCount--;
                                        if (iPrevChild < lastMergedWith)
                                            lastMergedWith--;
                                    }
                                }
                                cNode.children.Add(holeNode);
                            }
                        }

                        if (lastMergedWith != -1)
                        {
                            cNode.children.AddRange(children[lastMergedWith].children);
                            children.RemoveAt(lastMergedWith);
                            oldChildCount--;
                            iChild--;
                        }
                        cOther = cNode;
                        lastMergedWith = iChild;
                    }
                    else
                    {
                        if (clippingResult.Length == 1)
                        {
                            cNode.contour = clippingResult[0];
                            cNode.AddSolidContour(other, ref wasContourUsed);
                            cOther.AddSolidContour(cNode.contour, ref wasContourUsed);
                        }
                        else
                        {
                            HandleSplitting(clippingResult, cNode, other);

                            children.RemoveAt(iChild);
                            iChild--;
                            oldChildCount--;
                        }
                    }

                }
            }
            if (!wasContourUsed)
            {
                wasContourUsed = true;
                children.Add(new ContourNode(other, true));
            }
        }

        public bool TryCombineAsNew(Contour other, out ContourNode result)
        {
            ContourNode cOther = new ContourNode(other, true);

            Contour[] clippingResult;
            PolygonClipper.ResultType resultType = PolygonClipper.Compute(contour, cOther.contour, (isSolid) ? PolygonClipper.BoolOpType.UNION : PolygonClipper.BoolOpType.DIFFERENCE, out clippingResult);
            bool wasContourUsed = true;

            if (resultType == PolygonClipper.ResultType.NoOverlap)
            {
                result = null;
                return false;
            }
            result = (ContourNode)this.Clone();
            if (resultType == PolygonClipper.ResultType.FullyContained)
            {
                wasContourUsed = false;
                cOther.AddSolidContour(result.contour, ref wasContourUsed);
                result = cOther;
                return true;
            }
            if (resultType == PolygonClipper.ResultType.FullyContains)
            {
                if (!isSolid)
                {
                    throw new Exception("RootNode should always be solid");
                }
                result.AddSolidContour(other, ref wasContourUsed);
                return true;
            }

            //SuccesfullyClipped
            
            if (isSolid)
            {
                result.AddSolidContour(other, ref wasContourUsed);
                cOther.AddSolidContour(result.contour, ref wasContourUsed);
                for (int iResult = 0; iResult < clippingResult.Length; iResult++)
                {
                    if (clippingResult[iResult].IsSolid())
                    {
                        result.contour = clippingResult[iResult];
                    }
                    else
                    {
                        //Hole creation
                        ContourNode holeNode = new ContourNode(clippingResult[iResult], false);
                        result.children.Add(holeNode);
                    }
                }
            }
            else
            {
                if (clippingResult.Length == 1)
                {
                    result.contour = clippingResult[0];
                    result.AddSolidContour(other, ref wasContourUsed);
                    cOther.AddSolidContour(result.contour, ref wasContourUsed);
                }
                else
                {
                    HandleSplitting(clippingResult, result, other);
                }
            }
            return true;
        }

        private void HandleSplitting(Contour[] holes, ContourNode src, Contour other)
        {
            for (int iResult = 0; iResult < holes.Length; iResult++)
            {
                ContourNode holeNode = new ContourNode(holes[iResult], false);
                for (int iOldChild = 0; iOldChild < src.children.Count; iOldChild++)
                {
                    Contour[] holeClippingResult;
                    PolygonClipper.ResultType holeResultType = PolygonClipper.Compute(holeNode.contour, src.children[iOldChild].contour, PolygonClipper.BoolOpType.DIFFERENCE, out holeClippingResult);

                    if (holeResultType == PolygonClipper.ResultType.FullyContains)
                    {
                        holeNode.children.Add(src.children[iOldChild]);
                        src.children.RemoveAt(iOldChild);
                        iOldChild--;
                    }
                    else if (holeResultType == PolygonClipper.ResultType.SuccesfullyClipped)
                    {
                        if (holeClippingResult.Length == 1)
                        {
                            holeNode.contour = holeClippingResult[0];

                        }
                        else
                        {
                            HandleSplitting(holeClippingResult, holeNode, other);
                        }

                        bool consumed2 = true;
                        src.children[iOldChild].AddSolidContour(other, ref consumed2);
                        children.AddRange(src.children[iOldChild].children);
                        src.children[iOldChild].children.Clear();
                    }
                }
                children.Add(holeNode);
            }
        }

        public object Clone()
        {
            return OribowsUtilitys.DeepCopy<ContourNode>(this,
               new OribowsUtilitys.SerializationSurrogateContainer(new BoundsSerializationSurrogate(), typeof(Bounds), new System.Runtime.Serialization.StreamingContext(System.Runtime.Serialization.StreamingContextStates.All)),
               new OribowsUtilitys.SerializationSurrogateContainer(new Vector2SerializationSurrogate(), typeof(Vector2), new System.Runtime.Serialization.StreamingContext(System.Runtime.Serialization.StreamingContextStates.All)));
        }
    }
}
