using UnityEngine;
using System.Linq;
using UnityEditor;

public class NavSurface2d : MonoBehaviour
{
    private enum CollectObjectsMethod {
        All,
        Volume,
        Children
    }
    private enum UsedGeometryType {
        Sprites,
        Colliders2d
    }


    [SerializeField]
    private CollectObjectsMethod collectObjects = CollectObjectsMethod.All;
    [SerializeField]
    private LayerMask includeLayers = int.MaxValue;
    [SerializeField]
    private UsedGeometryType useGeometry = UsedGeometryType.Colliders2d;


    public void CollectNavGeometry() {
        IEnu[] navObjects;
        System.Type geometryType = typeof(Collider2D);
        switch (useGeometry)
        {
            case UsedGeometryType.Colliders2d:
                geometryType = typeof(Collider2D);
                break;
            case UsedGeometryType.Sprites:
                geometryType = typeof(SpriteRenderer);
                break;
        }

        switch (collectObjects)
        {
            case CollectObjectsMethod.All:
                var allGameObjects = GameObject.FindObjectsOfType<GameObject>();
                navObjects = (from item in allGameObjects
                 where GameObjectUtility.AreStaticEditorFlagsSet(item, StaticEditorFlags.NavigationStatic) &&
                 item.GetComponent(geometryType) != null
                 select item.GetComponent(geometryType)
                 );

                break;
            case CollectObjectsMethod.Volume:
                break;
            case CollectObjectsMethod.Children:
                break;
        }
    }
}
