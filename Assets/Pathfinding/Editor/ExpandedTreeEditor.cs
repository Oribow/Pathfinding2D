using UnityEngine;
using System.Collections;
using UnityEditor;
using NavMesh2D.Core;
using System;


[CustomEditor(typeof(ExpandedTree))]
public class ExpandedTreeEditor : Editor
{
    Material previewMat;

    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        RenderTexture previewTex = RenderTexture.GetTemporary((int)r.width, (int)r.height, 0);
        RenderTexture prevActive = RenderTexture.active;

        RenderTexture.active = previewTex;

        if (!previewMat)
        {
            previewMat = new Material(Shader.Find("Lines/Colored Blended"));
            previewMat.hideFlags = HideFlags.HideAndDontSave;
            previewMat.shader.hideFlags = HideFlags.HideAndDontSave;
        }
        ExpandedTree tree = (ExpandedTree)target;
        if (tree.headNode.children.Count > 0)
        {
            Bounds bounds = tree.headNode.children[0].contour.bounds;
            foreach (ExpandedNode cn in tree.headNode.children)
            {
                bounds.Encapsulate(cn.contour.bounds.min);
                bounds.Encapsulate(cn.contour.bounds.max);
            }

            Matrix4x4 modelMat = new Matrix4x4();
            modelMat.SetTRS(-bounds.min, Quaternion.identity, -new Vector3((bounds.extents.x * 2) / r.width, (bounds.extents.y * 2) / r.height, 0));
            GL.PushMatrix();
            GL.LoadPixelMatrix(bounds.min.x, bounds.max.x, bounds.min.y, bounds.max.y);
            GL.Begin(GL.LINES);
            try
            {
                previewMat.SetPass(0);
                GL.Clear(true, true, Color.white);


                int counter = 0;
                foreach (ExpandedNode cn in tree.headNode.children)
                {
                    DrawContourNode(cn, counter++);
                }
            }
            finally
            {
                GL.End();
                GL.PopMatrix();
            }
        }
        RenderTexture.active = prevActive;
        EditorGUI.DrawPreviewTexture(r, previewTex);
        RenderTexture.ReleaseTemporary(previewTex);

    }

    void DrawContourNode(ExpandedNode node, int colorIndex)
    {
        GL.Color(Color.green);

        int edgeCount = (node.contour.isClosed) ? node.contour.pointNodeCount : node.contour.pointNodeCount - 1;
        PointNode pn = node.contour.firstPoint;
        PointNode.ObstructedSegment obstruction;
        for (int i = 0; i < edgeCount; i++, pn = pn.Next)
        {
            GL.Vertex(pn.pointB);
            GL.Vertex(pn.pointC);
            obstruction = pn.FirstObstructedSegment;
            GL.Color(Color.red);
            while (obstruction != null)
            {
                GL.Vertex(pn.tangentBC * obstruction.start + pn.pointB);
                GL.Vertex(pn.tangentBC * obstruction.end + pn.pointB);
                obstruction = obstruction.next;
            }
            GL.Color(Color.green);
        }
        foreach (ExpandedNode child in node.children)
            DrawContourNode(child, colorIndex);
    }

    public override bool HasPreviewGUI()
    {
        return true;

    }
}
