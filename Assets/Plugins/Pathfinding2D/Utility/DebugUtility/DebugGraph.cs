using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class DebugGraph : IEnumerable<GraphHolder> {

    static DebugGraph instance;
    public static DebugGraph Instance { get { if (instance == null) instance = new DebugGraph(); return instance; } }

    Dictionary<int, GraphHolder> graphs;

    public DebugGraph()
    {
#if UNITY_EDITOR
        graphs = new Dictionary<int, GraphHolder>(1);
#else
        Debug.Log("Please don't call DebugGraph() will not in Editormode!");
#endif
    }

    public void StartNewGraph(int graphId, Graph graph, string graphName)
    {
#if UNITY_EDITOR
        graphs.Add(graphId, new GraphHolder(graph, graphName));
#else
        Debug.Log("Please don't call StartNewGraph() will not in Editormode!");
#endif
    }

    public void EndGraph(int graphId)
    {
#if UNITY_EDITOR
        graphs.Remove(graphId);
#else
        Debug.Log("Please don't call EndGraph() will not in Editormode!");
#endif
    }

    public void FeedChannel(int graphId, int channelIndex, float value)
    {
#if UNITY_EDITOR
        GraphHolder graph;
        if (graphs.TryGetValue(graphId, out graph))
        {
            if (graph.isPaused)
                return;

            graph.graph.channels[channelIndex].Feed(value, graph.graph);
        }
        else
        {
            Debug.LogError("Tried to feed non-existing graph \""+graph.name+"\"");
        }
#else
        Debug.Log("Please don't call FeedChannel() will not in Editormode!");
#endif
    }

    public void DeltaFeedChannel(int graphId, int channelIndex, float delta)
    {
#if UNITY_EDITOR
        GraphHolder graph;
        if (graphs.TryGetValue(graphId, out graph))
        {
            if (graph.isPaused)
                return;

            graph.graph.channels[channelIndex].DelatFeed(delta, graph.graph);
        }
        else
        {
            Debug.LogError("Tried to feed non-existing graph \"" + graphId + "\"");
        }
#else
        Debug.Log("Please don't call FeedChannel() will not in Editormode!");
#endif
    }

    public void SetFreeze(int graphId, bool freezed)
    {
        GraphHolder graph;
        if (graphs.TryGetValue(graphId, out graph))
        {
            graph.isPaused = freezed;
        }
        else
        {
            Debug.LogError("Tried to freeze non-existing graph \"" + graph.name + "\"");
        }
    }

    public void SetHide(int graphId, bool hide)
    {
        GraphHolder graph;
        if (graphs.TryGetValue(graphId, out graph))
        {
            graph.isHidden = hide;
        }
        else
        {
            Debug.LogError("Tried to hide non-existing graph \"" + graph.name + "\"");
        }
    }

#if UNITY_EDITOR
    IEnumerator IEnumerable.GetEnumerator()
    {
        return graphs.Values.GetEnumerator();
    }

    public IEnumerator<GraphHolder> GetEnumerator()
    {
        return graphs.Values.GetEnumerator();
    }

    public void ClearStartedGraphs()
    {
        graphs.Clear();
    }

    public bool IsEmpty { get { return graphs.Count == 0; } }
#endif
}

public class GraphHolder
{
    public Graph graph;
    public string name;
    public bool isPaused;
    public bool isHidden;

    public GraphHolder(Graph graph, string name, bool isPaused = false, bool isHidden = false)
    {
        this.graph = graph;
        this.name = name;
        this.isPaused = isPaused;
    }
}
