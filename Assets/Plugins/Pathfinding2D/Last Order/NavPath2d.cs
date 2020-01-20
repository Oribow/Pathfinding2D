using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavPath2d
{
    List<NavPosition2d> pathPoints;

    public NavPath2d()
    {
        this.pathPoints = new List<NavPosition2d>();
    }

    public void Add(NavPosition2d item)
    {
        pathPoints.Add(item);
    }
}
