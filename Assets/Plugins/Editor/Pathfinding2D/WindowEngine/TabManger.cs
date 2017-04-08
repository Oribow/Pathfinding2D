using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[System.Serializable]
internal class TabManger
{
    public ITab CurrentTab { get { if (currentTabIndex == -1) return null; return tabs[currentTabIndex]; } }
    public int TabCount { get { return tabs.Count; } }

    [SerializeField]
    List<ITab> tabs;
    [SerializeField]
    int currentTabIndex;

    public TabManger()
    {
        tabs = new List<ITab>();
        currentTabIndex = 0;
    }

    public TabManger(params ITab[] tab)
    {
        tabs = new List<ITab>(tab);
        currentTabIndex = 0;
    }

    public void DoLayoutGUI()
    {
        bool prevEnabled = GUI.enabled;

        if (currentTabIndex >= tabs.Count || (currentTabIndex != -1 && !tabs[currentTabIndex].IsEnabled))
            currentTabIndex = -1;

        GUILayout.BeginHorizontal(GUILayout.MinHeight(21));
        {
            for (int iTab = 0; iTab < tabs.Count; iTab++)
            {
                ITab t = tabs[iTab];
                GUI.enabled = t.IsEnabled && iTab != currentTabIndex;
                if (GUILayout.Button(t.TabHeader, GUILayout.ExpandHeight(true)))
                {
                    SwitchToTab(iTab);
                }
            }
            GUI.enabled = prevEnabled;
        }
        GUILayout.EndHorizontal();
    }

    public void DoSceneGUI(SceneView sceneView)
    {
        if (currentTabIndex != -1)
            tabs[currentTabIndex].OnSceneGUI(sceneView);
    }

    public void AddTab(ITab newTab)
    {
        tabs.Add(newTab);
    }

    public void RemoveTab(ITab tab)
    {
        tabs.Remove(tab);
        if (currentTabIndex >= tabs.Count)
            currentTabIndex = -1;
    }

    public void SwitchToTab(int newTabIndex)
    {
        if (newTabIndex == currentTabIndex)
            return;

        if (currentTabIndex != -1)
            tabs[currentTabIndex].OnUnselected();
        tabs[newTabIndex].OnSelected();
        currentTabIndex = newTabIndex;
    }

    public void UnselectTab()
    {
        if (currentTabIndex == -1)
            return;

        tabs[currentTabIndex].OnUnselected();
        currentTabIndex = -1;
    }
}


