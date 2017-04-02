using UnityEngine;
using System.Collections;
using UnityEditor;

public class GraphDataDrawer
{

    Material lineMaterial;

    public GraphDataDrawer()
    {
        CreateLineMaterial();
    }

    public void DrawGraph(Rect position, Graph graph)
    {
        if (Event.current.type != EventType.Repaint)
            return;

        EditorGUI.DrawRect(position, new Color(0.7f, 0.7f, 0.7f));
        Rect pos = position;
        pos.height = EditorGUIUtility.singleLineHeight;
        EditorGUI.LabelField(pos, graph.yMax.ToString("0.00"));
        pos.y = position.yMax - EditorGUIUtility.singleLineHeight;
        EditorGUI.LabelField(pos, graph.yMin.ToString("0.00"));

        CreateLineMaterial();
        lineMaterial.SetPass(0);

        GL.PushMatrix();
        GL.LoadPixelMatrix();

        GL.Begin(GL.LINES);
        GL.Color(new Color(0.5f, 0.5f, 0.5f));
        GL.Vertex3(position.xMin, Mathf.Lerp(position.yMin, position.yMax, Mathf.InverseLerp(graph.yMax, graph.yMin, 0.5f)), 0);
        GL.Vertex3(position.xMax, Mathf.Lerp(position.yMin, position.yMax, Mathf.InverseLerp(graph.yMax, graph.yMin, 0.5f)), 0);

        int W = (int)position.xMax;
        int H = (int)position.height;
        int prevYPix;
        int prevXPix;
        for (int chan = 0; chan < graph.channels.Length; chan++)
        {
            Graph.Channel C = graph.channels[chan];

            if (C == null)
                Debug.Log("FOO:" + chan);

            GL.Color(C._color);
            prevYPix = (int)(Mathf.InverseLerp(graph.yMax, graph.yMin, 0.5f) * H + position.yMin);
            prevXPix = W - 1;

            for (int h = 0; h < Graph.MAX_HISTORY; h++)
            {
                int xPix = (W - 1) - h;

                if (xPix >= position.xMin)
                {
                    float y = C._data[h];

                    float y_01 = Mathf.InverseLerp(graph.yMax, graph.yMin, y);

                    int yPix = (int)(y_01 * H + position.yMin);

                    GL.Vertex3(prevXPix, prevYPix, 0);
                    GL.Vertex3(xPix, yPix, 0);
                    //Plot(xPix, yPix);

                    prevYPix = yPix;
                    prevXPix = xPix;
                }
                
            }
        }

        GL.End();

        GL.PopMatrix();

    }

    // plot an X
    void Plot(float x, float y)
    {
        // first line of X
        GL.Vertex3(x - 1, y - 1, 0);
        GL.Vertex3(x + 1, y + 1, 0);

        // second
        GL.Vertex3(x - 1, y + 1, 0);
        GL.Vertex3(x + 1, y - 1, 0);
    }

    void CreateLineMaterial()
    {
        if (!lineMaterial)
        {
            lineMaterial = new Material(Shader.Find("Lines/Colored Blended"));
        }
    }
}
