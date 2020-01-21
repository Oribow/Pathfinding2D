using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class GizmosQueue : MonoBehaviour
{
    public static GizmosQueue Instance { get; private set; }

    private LinkedList<GizmosCommand> commands;

    [SerializeField, ReadOnly]
    private int itemCount;

    [SerializeField]
    private float timeScale;

    [SerializeField]
    private bool enableDrawing;

    private double lastGizmosCallTime;

    private void OnEnable()
    {
        GizmosQueue.Instance = this;
        lastGizmosCallTime = EditorApplication.timeSinceStartup;
    }

    public void Enqueue(float time, Action drawFunc)
    {
        if (commands == null)
            commands = new LinkedList<GizmosCommand>();

        commands.AddLast(new GizmosCommand(time, drawFunc));
        EditorApplication.QueuePlayerLoopUpdate();
    }

    private void OnDrawGizmos()
    {
        double delta = EditorApplication.timeSinceStartup - lastGizmosCallTime;
        lastGizmosCallTime = EditorApplication.timeSinceStartup;

        if (commands == null)
            return;

        itemCount = commands.Count;
        var node = commands.First;

        while (node != null)
        {
            if (enableDrawing)
                node.Value.drawGizmos();
            node.Value.lifeTime -= (float)delta * timeScale;
            if (node.Value.lifeTime <= 0)
            {
                var prevNext = node.Next;
                commands.Remove(node);
                node = prevNext;
            }
            else
            {
                node = node.Next;
            }
        }

        if (commands.Count > 0)
            EditorApplication.QueuePlayerLoopUpdate();
    }

}

class GizmosCommand
{
    public float lifeTime;
    public Action drawGizmos;

    public GizmosCommand(float lifeTime, Action drawGizmos)
    {
        this.lifeTime = lifeTime;
        this.drawGizmos = drawGizmos;
    }
}
