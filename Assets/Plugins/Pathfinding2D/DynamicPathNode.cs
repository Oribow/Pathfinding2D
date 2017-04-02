using UnityEngine;

namespace Pathfinding2D
{
    public class DynamicPathNode : PathNode
    {
        Quaternion lastRotation;
        Collider2D collider;

        public DynamicPathNode(int id, float xMin, float xMax, Vector2[] vertices, Collider2D collider) : base(id, xMin, xMax, vertices)
        {
            this.collider = collider;
            lastRotation = collider.transform.rotation;
            xMin -= collider.transform.position.x;
            xMax -= collider.transform.position.x;
        }

        public void UpdateBounds()
        {
            if (collider.transform.rotation != lastRotation)
            {
                //bounds update
            }
        }

        public override float XMin
        {
            get { return collider.transform.position.x + xMin; }
        }

        public override float XMax
        {
            get { return collider.transform.position.x + XMax; }
        }
    }
}
