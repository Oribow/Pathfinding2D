using UnityEngine;
using System.Collections;

public interface IVisualizable {

    void DebugVisualization(bool drawWithGizmos);
    void DebugVisualization(bool drawWithGizmos, Color primaryColor, Color secondaryColor);
    void DebugVisualization(bool drawWithGizmos, Color primaryColor, Color secondaryColor, float indicatorSize);
    void DebugVisualization(bool drawWithGizmos, Color primaryColor, Color secondaryColor, Color highlightColor, float indicatorSize);
}
