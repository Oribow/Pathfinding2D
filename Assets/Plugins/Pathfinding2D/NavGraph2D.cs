using UnityEngine;
using System.Collections;
using NavGraph.Build;

//Basic component and representation of exactly 1 unique NavGraph.
//Should be a general interface for all NavGraph related things.
public class NavGraph2D : MonoBehaviour {

    [SerializeField]
    CollisionGeometryFilter filter;

    SmartNavGraphUpdater smartNavGraphUpdater;
    ContourGenerationController contourGenerationController;

    void Awake()
    {
        contourGenerationController = new ContourGenerationController(filter);
        //smartNavGraphUpdater = new SmartNavGraphUpdater();
    }

    void Update()
    {
        //smartNavGraphUpdater.DoUpdate(filter.GetStaticCollider());
        ContourNode[] nodes = contourGenerationController.GetContours();
        foreach (var n in nodes)
            n.contour.DebugVisualization(false);
    }
}
