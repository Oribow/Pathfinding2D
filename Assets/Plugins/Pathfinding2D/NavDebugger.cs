using UnityEngine;
using System.Collections;
using Pathfinding2D;
using Utility;
using System;
using System.Collections.Generic;
using NavData2d;

public class NavDebugger : MonoBehaviour
{
    /*
    public enum DebugWhichSet { CollisionGeometrySet, OutlineTree, ExpandedTree, NavigationData2D }

    public int circleVertCount;
    public DebugWhichSet debugConfig;

    [Range(0.0f, 10.0f)]
    public float minHeightTest = 0;
    [Range(0.001f, .5f)]
    public float nodeMergeDist;
    [Range(0.0f, 5f)]
    public float maxEdgeDeviation;
    [SerializeField]
    public NavAgentGroundWalkerSettings agentSettings;
    [SerializeField]
    public NavPath path;
    public Collider2D forceLast;
    public int autoSolveTill;
    public bool showDebug;

    public Transform goal;
    public Transform start;
    public Transform closestTestPoint;


    GeometrySet cgs;
    ContourTree outlineTree;
    ExpandedTree exTrees;
    NavigationData2D navData2D;

    // Use this for initialization
    void Start()
    {
        try
        {
            System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();

            Collider2D[] allCollider = GameObject.FindObjectsOfType<Collider2D>();
            for (int ic = 0; ic < allCollider.Length; ic++)
            {
                if (allCollider[ic] == forceLast)
                {
                    allCollider[ic] = allCollider[allCollider.Length - 1];
                    allCollider[allCollider.Length - 1] = forceLast;
                    break;
                }
            }
            long totalEllapsedTime = 0;

            cgs = GeometrySetBuilder.Build(allCollider, circleVertCount);
            Debug.Log("CollisionGeometrySetBuilder finished in " + (watch.ElapsedMilliseconds / 1000f) + " sec.");
            
            totalEllapsedTime += watch.ElapsedMilliseconds;
            watch.Reset();
            watch.Start();

            outlineTree = ContourTree.Build(cgs, nodeMergeDist, maxEdgeDeviation);
            Debug.Log("OutlineTreeBuilder finished in " + (watch.ElapsedMilliseconds / 1000f) + " sec.");

            totalEllapsedTime += watch.ElapsedMilliseconds;
            watch.Reset();
            watch.Start();

            exTrees = ExpandedTree.Build(outlineTree, minHeightTest);
            Debug.Log("ExpandedTreeSetBuilder finished in " + (watch.ElapsedMilliseconds / 1000f) + " sec.");

            totalEllapsedTime += watch.ElapsedMilliseconds;
            watch.Reset();
            watch.Start();

            /* navData2D = new RawNavigationData2DBuilder(agentSettings).Build(exTrees, ScriptableObject.CreateInstance<RawNavigationData2D>())
                 .ToNavigationData2D();
             Debug.Log("NavigationData2DBuilder finished in " + (watch.ElapsedMilliseconds / 1000f) + " sec.");
             
            totalEllapsedTime += watch.ElapsedMilliseconds;
            watch.Stop();
            Debug.Log("Total build time is " + (totalEllapsedTime / 1000f) + " sec.");
        }
        catch (Exception e)
        {
            Debug.Log(e.Message +" -> "+ e.StackTrace);
        }
    }

    void PathCalcFinished(NavPath path)
    {
        this.path = path;
        if (path != null)
            path.Visualize();
    }

    int counter = -1;
    bool show;
    bool error;
    // Update is called once per frame
    void Update()
    {
        //PathPlaner.Instance.FindRequestedPath(new PathRequest(start.position, goal.position, PathCalcFinished, lightSkin, false));

        if (!showDebug)
            return;

        switch (debugConfig)
        {
            case DebugWhichSet.OutlineTree:
                if ((Input.GetKeyDown(KeyCode.Space) || counter < autoSolveTill) && !error)
                {
                    Debug.Log("wspace");
                    timer = 0;
                    if (counter == -1)
                    {
                        outlineTree = new ContourTree();
                        counter = 0;
                    }
                    if (counter < cgs.colliderVerts.Count)
                    {
                        try
                        {
                            outlineTree.AddContour(cgs.colliderVerts[counter]);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e.StackTrace);
                            error = true;
                        }
                        counter++;
                    }
                }
                break;
        }
        timer += Time.deltaTime;
        switch (debugConfig)
        {
            case DebugWhichSet.CollisionGeometrySet:
                if (cgs != null)
                {
                    cgs.VisualDebug();
                }
                break;
            case DebugWhichSet.OutlineTree:
                if (outlineTree != null)
                {
                    outlineTree.VisualDebug();
                    if (timer % 1 < 0.1f)
                        show = !show;
                    if (counter > 0 && show)
                    {
                        Contour cC = new Contour(cgs.colliderVerts[counter - 1]);
                        cC.VisualDebug(Utility.DifferentColors.GetColor(0));
                        
                    }
                }
                break;
            case DebugWhichSet.ExpandedTree:
                if (exTrees != null)
                {
                    exTrees.headNode.VisualDebug();
                    Vector2 mappedPoint;
                    Vector2 normal;
                    if (exTrees.TryMapPointToContour(closestTestPoint.position, out mappedPoint, out normal))
                    {
                        Debug.DrawLine(closestTestPoint.position, mappedPoint, Color.green);
                        DebugExtension.DebugPoint(mappedPoint);
                    }

                }
                break;
            
        }
    }
    float timer = 0;*/
}
