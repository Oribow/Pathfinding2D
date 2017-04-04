using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Utility.ExtensionMethods;
using System;

namespace NavGraph.Build
{
    [System.Serializable]
    public class ColliderSet : ISerializationCallbackReceiver
    {
        public List<Collider2D> colliderList;
        [System.NonSerialized]
        public Vector2[][] colliderVerts;
        [System.NonSerialized]
        GeometrySetBuilder geometrySetBuilder;

        [SerializeField]
        int circleVertCount;
        [SerializeField]
        int decimalPlacesOfCoords;


        public ColliderSet ()
        {
            if (colliderList == null)
                colliderList = new List<Collider2D>();
            if (geometrySetBuilder == null)
                geometrySetBuilder = new GeometrySetBuilder(4, 3);
        }

        public int CircleColliderVertCount
        {
            get { return geometrySetBuilder.CircleVertCount; }
            set
            {
                if (value != geometrySetBuilder.CircleVertCount && value >= 4)
                {
                    geometrySetBuilder.CircleVertCount = value;
                    colliderVerts = geometrySetBuilder.Build(colliderList, colliderList.Count).ToVertexArray();
                    return;
                }
            }
        }

        public int DecimalPlacesOfCoords
        {
            get
            {
                return geometrySetBuilder.DecimalPlacesOfCoords;
            }
            set
            {
                if (value != geometrySetBuilder.DecimalPlacesOfCoords)
                {
                    geometrySetBuilder.DecimalPlacesOfCoords = value;
                    colliderVerts = geometrySetBuilder.Build(colliderList, colliderList.Count).ToVertexArray();
                }
            }
        }

        int oldColliderCount;

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

            // Relatively simple dupe check alg used
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

        public void RemoveAt(int index)
        {
            colliderList.RemoveAt(index);
            TriggerGeometryVertsUpdate();
        }

        public void TriggerGeometryVertsUpdate()
        {
            colliderVerts = geometrySetBuilder.Build(colliderList, colliderList.Count).ToVertexArray();
        }

        public void TryUpdateGeometryVerts()
        {
            if (oldColliderCount != colliderList.Count)
            {
                colliderVerts = geometrySetBuilder.Build(colliderList, colliderList.Count).ToVertexArray();
                oldColliderCount = colliderList.Count;
            }
        }

        public GeometrySet ToCollisionGeometrySet()
        {
            return geometrySetBuilder.Build(colliderList, colliderList.Count);
        }

        public void OnBeforeSerialize()
        {
            if (geometrySetBuilder != null)
            {
                circleVertCount = geometrySetBuilder.CircleVertCount;
                decimalPlacesOfCoords = geometrySetBuilder.DecimalPlacesOfCoords;
            }
        }

        public void OnAfterDeserialize()
        {
            geometrySetBuilder = new GeometrySetBuilder(circleVertCount, decimalPlacesOfCoords);
        }
    }
}
