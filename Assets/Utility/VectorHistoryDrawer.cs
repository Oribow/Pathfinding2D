using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VectorHistoryDrawer : MonoBehaviour
{

    public static VectorHistoryDrawer instance;

    public static void EnqueueNewLines(int channel, params Vector2[] lines)
    {
        if (instance != null)
            instance.EnqueueLines(channel, lines);
    }

    public static void EnqueueNewLines(int channel, Color[] colors, Vector2[] lines, string[] msg)
    {
        if (instance != null)
            instance.EnqueueLines(channel, colors, lines, msg);
    }
    public static void EnqueueNewLines(int channel, Vector2[] lines, string[] msg)
    {
        if (instance != null)
            instance.EnqueueLines(channel, lines, msg);
    }

    public static void EnqueueNewLines(int channel, params Vector2d[] lines)
    {
        if (instance != null)
            instance.EnqueueLines(channel, lines);
    }

    public static void EnqueueNewLines(int channel, Color[] colors, Vector2d[] lines, string[] msg)
    {
        if (instance != null)
            instance.EnqueueLines(channel, colors, lines, msg);
    }

    public static void EnqueueNewLines(int channel, Vector2d[] lines, string[] msg)
    {
        if (instance != null)
            instance.EnqueueLines(channel, lines, msg);
    }

    public static void MoveFurther()
    {
        if (instance != null)
            instance.MovFurther();
    }

    public static void SetNewestOnlyFlag(int channel, bool flag)
    {
        if (instance != null)
            instance.SetDrawOnlyNewestFlag(channel, flag);
    }

    public Vector3 offset;
    public Vector3 channelOffset;
    public Vector3 historyOffset;
    public bool clearData;
    [Range(0, 33)]
    public int historyToDraw;
    public int pointsOfChannel;
    [ReadOnly]
    public int historyDataCount;
    [ReadOnly]
    public List<LineData> currentLines;

    List<List<List<LineData>>> data;
    List<bool> drawOnlyNewest;
    public int time;

    public void EnqueueLines(int channel, params Vector2[] lines)
    {
        if (lines.Length % 2 != 0)
            throw new System.Exception("The supplied line array has to have an even length!");
        EnsureCapacity(channel);
        for (int iLine = 0; iLine < lines.Length; iLine += 2)
        {
            data[channel][time].Add(new LineData(lines[iLine], lines[iLine + 1], Utility.DifferentColors.GetColor(time + data[channel][time].Count)));
        }
    }

    public void EnqueueLines(int channel, Color[] colors, Vector2[] lines, string[] msg)
    {
        if (lines.Length % 2 != 0)
            throw new System.Exception("The supplied line array has to have an even length!");

        EnsureCapacity(channel);
        for (int iLine = 0; iLine < lines.Length; iLine += 2)
        {
            data[channel][time].Add(new LineData(lines[iLine], lines[iLine + 1], colors[iLine], msg[iLine]));
        }
    }

    public void EnqueueLines(int channel, Vector2[] lines, string[] msg)
    {
        if (lines.Length % 2 != 0)
            throw new System.Exception("The supplied line array has to have an even length!");

        EnsureCapacity(channel);
        for (int iLine = 0; iLine < lines.Length; iLine += 2)
        {
            data[channel][time].Add(new LineData(lines[iLine], lines[iLine + 1], Utility.DifferentColors.GetColor(time + data[channel][time].Count), msg[iLine]));
        }
    }

    public void EnqueueLines(int channel, params Vector2d[] lines)
    {
        if (lines.Length % 2 != 0)
            throw new System.Exception("The supplied line array has to have an even length!");
        EnsureCapacity(channel);
        for (int iLine = 0; iLine < lines.Length; iLine += 2)
        {
            data[channel][time].Add(new LineData((Vector2)lines[iLine], (Vector2)lines[iLine + 1], Utility.DifferentColors.GetColor(time + data[channel][time].Count)));
        }
    }

    public void EnqueueLines(int channel, Color[] colors, Vector2d[] lines, string[] msg)
    {
        if (lines.Length % 2 != 0)
            throw new System.Exception("The supplied line array has to have an even length!");

        EnsureCapacity(channel);
        for (int iLine = 0; iLine < lines.Length; iLine += 2)
        {
            data[channel][time].Add(new LineData((Vector2)lines[iLine], (Vector2)lines[iLine + 1], colors[iLine], msg[iLine]));
        }
    }

    public void EnqueueLines(int channel, Vector2d[] lines, string[] msg)
    {
        if (lines.Length % 2 != 0)
            throw new System.Exception("The supplied line array has to have an even length!");

        EnsureCapacity(channel);
        for (int iLine = 0; iLine < lines.Length; iLine += 2)
        {
            data[channel][time].Add(new LineData((Vector2)lines[iLine], (Vector2)lines[iLine + 1], Utility.DifferentColors.GetColor(time + data[channel][time].Count), msg[iLine]));
        }
    }

    void EnsureCapacity(int channel)
    {
        while (data.Count <= channel)
        {
            data.Add(new List<List<LineData>>());
        }
        while (drawOnlyNewest.Count <= channel)
            drawOnlyNewest.Add(false);
        if (data[channel] == null)
            data[channel] = new List<List<LineData>>();
        while (data[channel].Count <= time)
        {
            data[channel].Add(new List<LineData>());
        }
    }

    public void SetDrawOnlyNewestFlag(int channel, bool flag)
    {
        EnsureCapacity(channel);
        drawOnlyNewest[channel] = flag;
    }

    public void MovFurther()
    {
        time++;
    }

    void DrawLines(int channel, int historyIndex)
    {
        List<LineData> lines = data[channel][historyIndex];
        Vector3 offset;
        if (drawOnlyNewest[channel])
            offset = channel * channelOffset + this.offset;
        else
            offset = channel * channelOffset + historyOffset * historyIndex + this.offset;
        for (int iLine = 0; iLine < lines.Count; iLine++)
        {
            Debug.DrawLine((Vector3)lines[iLine].pointA + offset, (Vector3)lines[iLine].pointB + offset, lines[iLine].color);
        }
        if (historyIndex > 0 && !drawOnlyNewest[channel])
            DrawLines(channel, --historyIndex);
    }

    void Awake()
    {
        data = new List<List<List<LineData>>> {
            new List<List<LineData>>(),
            new List<List<LineData>>(),
            new List<List<LineData>>(),
            new List<List<LineData>>(),
            new List<List<LineData>>(),
            new List<List<LineData>>(),
             new List<List<LineData>>(),
        };
        drawOnlyNewest = new List<bool>
        {
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
        };
        instance = this;
    }

    void Update()
    {
        if (clearData)
        {
            data.Clear();
            drawOnlyNewest.Clear();
            clearData = false;
            time = 0;
        }
        if (data.Count == 0)
            return;
        pointsOfChannel = Mathf.Clamp(pointsOfChannel, 0, data.Count - 1);
        currentLines = data[pointsOfChannel][Mathf.Clamp(historyToDraw, 0, data[pointsOfChannel].Count - 1)];
        historyDataCount = data[pointsOfChannel][Mathf.Clamp(historyToDraw, 0, data[pointsOfChannel].Count - 1)].Count;
        for (int iCha = 0; iCha < data.Count; iCha++)
        {
            if (data[iCha].Count == 0)
                continue;
            DrawLines(iCha, Mathf.Clamp(historyToDraw, 0, data[iCha].Count - 1));
        }
    }

    [System.Serializable]
    public class LineData
    {
        public Vector2 pointA;
        public Vector2 pointB;
        public Color color;
        [Multiline]
        public string message;

        public LineData(Vector2 pointA, Vector2 pointB, Color color, string msg = "")
        {
            this.pointA = pointA;
            this.pointB = pointB;
            this.color = color;
            this.message = msg;
        }
    }
}
