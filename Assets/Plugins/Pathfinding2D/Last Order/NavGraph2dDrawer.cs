using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavGraph2dDrawer : MonoBehaviour
{
    public bool draw;
    [SerializeField, ReadOnly]
    public int nodeCount;

    private void OnDrawGizmos()
    {
        var graph = NavGraph2d.Instance;
        if (draw && graph != null)
        {
            nodeCount = graph.NodeCount;
            graph.DrawGizmos();
        }
    }
}
