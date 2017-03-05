using UnityEngine;
using System.Collections.Generic;



public class Graph
{
    public const int MAX_HISTORY = 1024;

    public readonly Channel[] channels;
    public readonly string[] channelNames;

    public float yMax;
    public float yMin;

    public Graph (int channelCount)
    {
        channels = new Channel[channelCount];
        channelNames = new string[channelCount];

        for (int iChannel = 0; iChannel < channelCount; iChannel++)
        {
            channels[iChannel] = new Channel(Utility.DifferentColors.GetColor(iChannel));
            channelNames[iChannel] = "Unnamed";
        }
    }

    public Graph(Color[] channelColors, string[] channelNames)
    {
        Debug.Assert(channelColors.Length > 0);
        Debug.Assert(channelNames.Length == channelColors.Length);
        this.channelNames = channelNames;
        channels = new Channel[channelColors.Length];
        for (int iChannel = 0; iChannel < channelColors.Length; iChannel++)
        {
            channels[iChannel] = new Channel(channelColors[iChannel]);
            channelNames[iChannel] = channelNames[iChannel];
        }
    }

    public Graph(string[] channelNames)
    {
        Debug.Assert(channelNames.Length > 0);
        this.channelNames = channelNames;
        channels = new Channel[channelNames.Length];
        for (int iChannel = 0; iChannel < channelNames.Length; iChannel++)
        {
            channels[iChannel] = new Channel(Utility.DifferentColors.GetColor(iChannel));
        }
    }

    public void UpdateBoundsUpper(Channel excludeChannel, float excludedChannelMax)
    {
        yMax = excludedChannelMax;
        for (int iChannel = 0; iChannel < channels.Length; iChannel++)
        {
            if (channels[iChannel] == excludeChannel)
                continue;

            foreach (var data in channels[iChannel]._data)
            {
                yMax = Mathf.Max(data, yMax);
            }
        }
    }

    public void UpdateBoundsLower(Channel excludeChannel, float excludedChannelMin)
    {
        yMin = excludedChannelMin;
        for (int iChannel = 0; iChannel < channels.Length; iChannel++)
        {
            if (channels[iChannel] == excludeChannel)
                continue;

            foreach (var data in channels[iChannel]._data)
            {
                yMin = Mathf.Max(data, yMin);
            }
        }
    }

    public class Channel
    {
        public float[] _data = new float[Graph.MAX_HISTORY];
        public Color _color = Color.white;

        public Channel(Color _C)
        {
            _color = _C;
        }

        public void Feed(float val, Graph graph)
        {
            float yMax = val;
            float yMin = val;
            float removedVal = _data[Graph.MAX_HISTORY - 1];


            for (int i = Graph.MAX_HISTORY - 1; i >= 1; i--)
            {
                yMax = Mathf.Max(yMax, _data[i-1]);
                yMin = Mathf.Min(yMin, _data[i-1]);
                _data[i] = _data[i - 1];
            }

            if (removedVal == graph.yMax && graph.yMax != _data[Graph.MAX_HISTORY - 1])
            {
                graph.UpdateBoundsUpper(this, yMax);
            }
            else if (removedVal == graph.yMin && graph.yMin != _data[Graph.MAX_HISTORY - 1])
            {
                graph.UpdateBoundsLower(this, yMin);
            }
            else if (graph.yMax < yMax)
                graph.yMax = yMax;
            else if (graph.yMin > yMin)
                graph.yMin = yMin;

            _data[0] = val;
        }

        public void DelatFeed(float delta, Graph graph)
        {
            Feed(_data[0] + delta, graph);
        }
    }
}
