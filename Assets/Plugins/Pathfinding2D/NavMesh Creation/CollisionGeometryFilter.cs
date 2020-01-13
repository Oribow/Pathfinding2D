using NavGraph.Build;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

//Filters all collider in the scene and returns the ones that should be included in the NavGraph.
//Will also extract the verts from the collider, if requested.
abstract class CollisionGeometryFilter : MonoBehaviour
{
    public abstract PolygonSet GetStaticCollider();
    public abstract PolygonSet GetDynamicCollider();
    public abstract PolygonSet GetHighlyDynamicCollider();
}
