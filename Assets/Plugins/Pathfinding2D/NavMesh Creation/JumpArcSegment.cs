using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class JumpArcSegment
{
    [SerializeField]
    public float j, halfG, v, doubleG;
    [SerializeField]
    public float minX, maxX;
    [SerializeField]
    public float startX, endX;
    [SerializeField]
    public float minY, maxY;

    public JumpArcSegment(float j, float g, float v, float startX, float endX)
    {
        this.j = j;
        this.halfG = g / 2;
        this.v = v;
        this.startX = startX;
        this.endX = endX;
        if (startX < endX)
        {
            minX = startX;
            maxX = endX;
        }
        else
        {
            minX = endX;
            maxX = startX;
        }
        minY = Mathf.Min(Calc(0), Calc(maxX - minX));
        doubleG = g * 2;
        maxY = (j * j) / (4 * doubleG);
    }

    public void UpdateArc(float j, float g, float v, float startX, float endX)
    {
        this.j = j;
        this.halfG = g / 2;
        this.v = v;
        this.startX = startX;
        this.endX = endX;
        if (startX < endX)
        {
            minX = startX;
            maxX = endX;
        }
        else
        {
            minX = endX;
            maxX = startX;
        }
        minY = Mathf.Min(Calc(0), Calc(maxX - minX));
        doubleG = g * 2;
        maxY = (j * j) / (4 * doubleG);
    }

    public float Calc(float x)
    {
        x /= v;
        return (j - halfG * x) * x;
    }

    public void VisualDebug(Vector2 origin, Color color)
    {
        Vector2 swapPos;
        Vector2 prevPos = new Vector2(minX, Calc(minX)) + origin;
        for (float x = minX; x + 0.1f < maxX; x += 0.1f)
        {
            swapPos = new Vector2(x, Calc(x)) + origin;
            Debug.DrawLine(prevPos, swapPos, color);
            prevPos = swapPos;
        }
        Debug.DrawLine(prevPos, new Vector2(maxX, Calc(maxX)) + origin, color);

    }
}
