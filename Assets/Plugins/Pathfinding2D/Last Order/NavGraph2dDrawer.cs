using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavGraph2dDrawer : MonoBehaviour
{
    public bool draw;

    private void OnDrawGizmos()
    {
        var graph = NavGraph2d.Instance;
        if (draw && graph != null)
        {
            graph.DrawGizmos();
        }
    }
}
