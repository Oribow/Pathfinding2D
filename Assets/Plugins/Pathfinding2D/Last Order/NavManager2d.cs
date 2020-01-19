using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class NavManager2d : MonoBehaviour
{
    public static NavManager2d Instance { get; private set; }

    private List<NavSurface2d> navSurfaces = new List<NavSurface2d>();

    private void OnEnable()
    {
        if (NavManager2d.Instance != null && NavManager2d.Instance != this)
        {
            Debug.LogWarning("Multiple instances of NavManager2d found. Please delete all but one.");
        }
        NavManager2d.Instance = this;
    }


}
