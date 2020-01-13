using NavGraph.Build;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class ContourGenerationController
{
    CollisionGeometryFilter filter;

    ContourNode[] staticContours;


    public ContourGenerationController(CollisionGeometryFilter filter)
    {
        this.filter = filter;
    }

    public ContourNode[] GetContours()
    {
        if (staticContours == null)
        {
            staticContours = GenerateContourNodes(filter.GetStaticCollider());
        }
        return staticContours;
    }

    ContourNode[] GenerateContourNodes(PolygonSet set)
    {
        var tree = ContourTree.Build(set);
        ContourNode[] result = new ContourNode[tree.ContourCount()];

        int iNode = 0;
        foreach (var node in tree)
        {
            result[iNode] = node;
            iNode++;
        }
        return result;
    }
}
