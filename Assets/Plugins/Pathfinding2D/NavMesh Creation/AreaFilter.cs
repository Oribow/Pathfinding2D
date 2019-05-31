using NavGraph.Build;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
class AreaFilter : CollisionGeometryFilter
{
    [SerializeField]
    LayerMask colliderMask;
    [SerializeField, Range(4, 100)]
    int circleVertCount = 20;
    [SerializeField, Range(0, 15)]
    int decimalPlacesOfCoords = 3;

    RectTransform areaCenter;

    GeometrySetBuilder geometrySetBuilder;

    GeometrySet staticColliderSet;
    GeometrySet dynamicColliderSet;

    void Awake ()
    {
        this.areaCenter = GetComponent<RectTransform>();
        this.geometrySetBuilder = new GeometrySetBuilder(circleVertCount, decimalPlacesOfCoords);
    }
    /*
    public GeometrySet GetDynamicCollider()
    {
        Collider2D[] collider = Physics2D.OverlapBoxAll(areaCenter.position, areaCenter.sizeDelta, areaCenter.eulerAngles.z, normalColliderMask);
        collider = collider.Where((Collider2D col) => { return !col.gameObject.isStatic; }).ToArray<Collider2D>();
        dynamicColliderSet = geometrySetBuilder.Build(collider, collider.Length);
        return dynamicColliderSet;
    }

    public GeometrySet GetHighlyDynamicCollider()
    {
        Collider2D[] collider = Physics2D.OverlapBoxAll(areaCenter.position, areaCenter.sizeDelta, areaCenter.eulerAngles.z, highlyDynamicColliderMask);
        GeometrySet highlyDynamicColliderSet = geometrySetBuilder.Build(collider, collider.Length);
        return highlyDynamicColliderSet;
    }*/

    public override GeometrySet GetStaticCollider()
    {
        if (staticColliderSet == null || areaCenter.hasChanged)
        {
            Collider2D[] collider = Physics2D.OverlapBoxAll(areaCenter.position, areaCenter.sizeDelta, areaCenter.eulerAngles.z, colliderMask);
            collider = collider.Where((Collider2D col) => { return col.gameObject.isStatic; }).ToArray<Collider2D>();
            staticColliderSet = geometrySetBuilder.Build(collider, collider.Length);
            areaCenter.hasChanged = false;
        }
        return staticColliderSet;
    }

    /*public override GeometrySet GetStaticCollider()
    {
        throw new NotImplementedException();
    }*/

    public override GeometrySet GetDynamicCollider()
    {
        throw new NotImplementedException();
    }

    public override GeometrySet GetHighlyDynamicCollider()
    {
        throw new NotImplementedException();
    }
}
