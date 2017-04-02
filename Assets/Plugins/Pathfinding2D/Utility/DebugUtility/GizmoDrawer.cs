using UnityEngine;
using System.Collections;

public class GizmoDrawer : MonoBehaviour {
#if UNITY_EDITOR
    public string icon;

    void OnDrawGizmos()
    {
        Gizmos.DrawIcon(transform.position, icon);
    }
#endif
}
