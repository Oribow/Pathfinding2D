using UnityEngine;
using System.Linq;
using UnityEditor;
using System.Collections.Generic;
using NavGraph.Build;

[RequireComponent(typeof(RectTransform))]
public class NavSurface2d : MonoBehaviour
{
    private enum CollectObjectsMethod
    {
        All,
        Volume,
        Children
    }

    [Header("Filter")]
    [SerializeField]
    private CollectObjectsMethod collectObjects = CollectObjectsMethod.All;
    [SerializeField]
    private LayerMask includeLayers = int.MaxValue;

    [Header("Conversion")]
    [SerializeField, Range(4, 100)]
    int circleVertCount = 20;

    public void Bake()
    {
        PolygonSet polygonSet = CollectNavigationPolygons();

    }

    public PolygonSet CollectNavigationPolygons()
    {
        // 1. Collect collider
        IEnumerable<Collider2D> navObjects;
        switch (collectObjects)
        {
            case CollectObjectsMethod.All:
                var allCollider = GameObject.FindObjectsOfType<Collider2D>();
                navObjects = (from item in allCollider
                              where
                              GameObjectUtility.AreStaticEditorFlagsSet(item.gameObject, StaticEditorFlags.NavigationStatic) &&
                              (item.gameObject.layer & includeLayers) > 0
                              select item
                 );

                break;
            case CollectObjectsMethod.Volume:
                var rectTransform = GetComponent<RectTransform>();
                var overlapedColliders = Physics2D.OverlapBoxAll(rectTransform.position, rectTransform.rect.size, rectTransform.eulerAngles.z, includeLayers);
                navObjects = (from item in overlapedColliders
                              where GameObjectUtility.AreStaticEditorFlagsSet(item.gameObject, StaticEditorFlags.NavigationStatic)
                              select item);
                break;
            case CollectObjectsMethod.Children:
                var childCollider = this.GetComponentsInChildren<Collider2D>();
                navObjects = (from item in childCollider
                              where
                              GameObjectUtility.AreStaticEditorFlagsSet(item.gameObject, StaticEditorFlags.NavigationStatic) &&
                              (item.gameObject.layer & includeLayers) > 0
                              select item
                 );
                break;
            default:
                navObjects = Enumerable.Empty<Collider2D>();
                Debug.LogError("Unkown collections method: " + collectObjects);
                break;
        }

        // 2. convert collider to geometry
        PolygonSet polygonSet = new PolygonSet(circleVertCount);
        foreach (var col in navObjects)
        {
            polygonSet.AddCollider(col);
        }
        return polygonSet;
    }
}
