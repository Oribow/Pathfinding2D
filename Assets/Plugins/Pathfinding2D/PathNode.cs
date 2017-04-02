using UnityEngine;

namespace Pathfinding2D
{
    public class PathNode
    {
        public int id;
        public byte flag;
        public Vector2[] vertices;
        protected float xMin;
        protected float xMax;

        public PathNode(int id, float xMin, float xMax, Vector2[] vertices)
        {
            this.id = id;
            this.xMax = xMax;
            this.xMin = xMin;
            this.vertices = vertices;
        }

        public virtual float XMin
        {
            get { return xMin; }
        }

        public virtual float XMax
        {
            get { return xMax; }
        }
    }
}
