using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Utility.ExtensionMethods;
using NavMesh2D.Core;

namespace NavData2d.Editor
{
    [System.Serializable]
    internal class ColliderSet
    {
        public List<Collider2D> colliderList;
        public Vector2d[][] colliderVerts;

        public int CircleColliderVertCount
        {
            get { return circleColliderVerts; }
            set
            {
                value = Mathf.Max(circleColliderVerts, 3);
                if (value != circleColliderVerts)
                {
                    circleColliderVerts = value;
                    colliderVerts = GeometrySetBuilder.Build(colliderList, circleColliderVerts).colliderVerts.ToArray();
                    return;
                }
                circleColliderVerts = value;
            }
        }
        [SerializeField]
        int circleColliderVerts = 4;

        int oldColliderCount;

        public ColliderSet()
        {
            colliderList = new List<Collider2D>();
        }

        public void AddAllStaticCollider()
        {
            Collider2D[] allCollider = GameObject.FindObjectsOfType<Collider2D>();
            colliderList.AddRange(allCollider.Where(x => x.gameObject.isStatic && !colliderList.Any(y => y == x)));
            TryUpdateGeometryVerts();
        }

        public void AddSelectedCollider()
        {
            foreach (Transform selectedTransforms in Selection.transforms)
            {
                Collider2D[] childCollider = selectedTransforms.GetComponentsInChildren<Collider2D>();
                if (childCollider != null)
                    colliderList.AddRange(childCollider.Where(x => !colliderList.Any(y => y == x)));
            }
            TryUpdateGeometryVerts();
        }

        public void AddColliderOnLayer(LayerMask layerMask)
        {
            Collider2D[] allCollider = GameObject.FindObjectsOfType<Collider2D>();
            colliderList.AddRange(allCollider.Where(x => layerMask.IsLayerWithinMask(x.gameObject.layer) && !colliderList.Any(y => y == x)));
            TryUpdateGeometryVerts();
        }

        public void RemoveDuplicates()
        {
            var passedValues = new HashSet<Collider2D>();

            // Relatively simple dupe check alg used as example
            foreach (var item in colliderList)
                passedValues.Add(item); // True if item is new

            colliderList.Clear();
            colliderList.AddRange(passedValues.ToList());
            TryUpdateGeometryVerts();
        }

        public void RemoveAll()
        {
            colliderList.Clear();
            colliderList.Capacity = Mathf.Min(30, colliderList.Capacity);
            TryUpdateGeometryVerts();
        }

        public void TriggerGeometryVertsUpdate()
        {
            colliderVerts = GeometrySetBuilder.Build(colliderList, circleColliderVerts).colliderVerts.ToArray();
        }

        public void TryUpdateGeometryVerts()
        {
            if (oldColliderCount != colliderList.Count)
            {
                colliderVerts = GeometrySetBuilder.Build(colliderList, circleColliderVerts).colliderVerts.ToArray();
                oldColliderCount = colliderList.Count;
            }
        }

        public GeometrySet ToCollisionGeometrySet()
        {
            return GeometrySetBuilder.Build(colliderList, circleColliderVerts);
        }
    }
}
