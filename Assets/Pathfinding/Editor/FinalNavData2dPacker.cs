using UnityEngine;
using System.Collections;
using System;
using UnityEditor;
using NavMesh2D.Core;
using System.Collections.Generic;

namespace NavData2d.Editor
{
    [System.Serializable]
    public class FinalNavData2dPacker : ITab
    {
        public bool IsEnabled
        {
            get
            {
                return navBuilder.GlobalBuildContainer != null && navBuilder.GlobalBuildContainer.filteredNavData;
            }
        }

        public string TabHeader
        {
            get
            {
                return "FinalPacker";
            }
        }

        INavDataBuilder navBuilder;
        [SerializeField]
        NavigationData2D existingField;

        public FinalNavData2dPacker(INavDataBuilder navBuilder)
        {
            this.navBuilder = navBuilder;
        }

        public void OnGUI()
        {
            existingField = (NavigationData2D)EditorGUILayout.ObjectField("OldField", existingField, typeof(NavigationData2D), false);

            if (GUILayout.Button("Bake As New"))
            {
                existingField = MonoBehaviour.Instantiate<NavigationData2D>(navBuilder.GlobalBuildContainer.filteredNavData);
                Bake(existingField);
                existingField.SaveToAsset();
            }

            GUI.enabled = existingField != null;
            if (GUILayout.Button("Update Existing File"))
            {
                Bake(existingField);
                EditorUtility.SetDirty(existingField);
                AssetDatabase.SaveAssets();
            }
            GUI.enabled = true;
        }

        public void OnSceneGUI(SceneView sceneView)
        {
            NavData2DVisualizerWindow.SceneDrawNavData2D(navBuilder.GlobalBuildContainer.filteredNavData);
            foreach (var link in navBuilder.GlobalBuildContainer.jumpLinks)
            {
                link.DrawJumpLink(navBuilder.GlobalBuildContainer.navAgentSettings.width, navBuilder.GlobalBuildContainer.navAgentSettings.height
                    , link.jumpLinkSettings.showInSceneDetailed, link.isBiDirectional, false);
            }
        }

        public void OnSelected()
        {
            
        }

        public void OnUnselected()
        {
           
        }

        void Bake(NavigationData2D navData)
        {
            Dictionary<NavNode, List<MetaJumpLink>> linkTable = new Dictionary<NavNode, List<MetaJumpLink>>(navData.nodes.Length);
            List<MetaJumpLink> cList;
            foreach (MetaJumpLink link in navBuilder.GlobalBuildContainer.jumpLinks)
            {
                if (!link.isJumpLinkValid || !link.TryRemapPoints(navData))
                    continue;

                Debug.Assert(link.jumpArc.j > 0);
                if (link.isBiDirectional)
                {
                    MetaJumpLink inverseLink = link.InvertLink(navBuilder.GlobalBuildContainer.navAgentSettings);
                    if (!inverseLink.TryRemapPoints(navData))
                    {
                        Debug.Assert(false); // Should never happen!
                    }
                    Debug.Assert(inverseLink.jumpArc.j > 0);
                    if (linkTable.TryGetValue(navData.nodes[inverseLink.navPosA.navNodeIndex], out cList))
                    {
                        cList.Add(inverseLink);
                    }
                    else
                    {
                        cList = new List<MetaJumpLink>(2);
                        cList.Add(inverseLink);
                        linkTable.Add(navData.nodes[inverseLink.navPosA.navNodeIndex], cList);
                    }
                }

                if (linkTable.TryGetValue(navData.nodes[link.navPosA.navNodeIndex], out cList))
                {
                    cList.Add(link);
                }
                else
                {
                    cList = new List<MetaJumpLink>(2);
                    cList.Add(link);
                    linkTable.Add(navData.nodes[link.navPosA.navNodeIndex], cList);
                }
            }
            Dictionary<int, List<int>> vertLinkTable;
            List<int> cLinkList;
            MetaJumpLink cLink;
            foreach (KeyValuePair<NavNode, List<MetaJumpLink>> keyPair in linkTable)
            {
                IOffNodeLink[] allLinks = new IOffNodeLink[keyPair.Value.Count];
                vertLinkTable = new Dictionary<int, List<int>>(keyPair.Value.Count);
                for (int iLink = 0; iLink < keyPair.Value.Count; iLink++)
                {
                    cLink = keyPair.Value[iLink];
                    allLinks[iLink] = new Pathfinding2D.JumpLink(cLink);
                    if (vertLinkTable.TryGetValue(cLink.navPosA.navVertIndex, out cLinkList))
                    {
                        cLinkList.Add(iLink);
                    }
                    else
                    {
                        cLinkList = new List<int>(2);
                        cLinkList.Add(iLink);
                        vertLinkTable.Add(cLink.navPosA.navVertIndex, cLinkList);
                    }
                }
                keyPair.Key.links = allLinks;

                foreach (KeyValuePair<int, List<int>> vertLink in vertLinkTable)
                {
                    keyPair.Key.verts[vertLink.Key].linkIndex = vertLink.Value.ToArray();
                }
            }
            navData.navAgentSettings = navBuilder.GlobalBuildContainer.navAgentSettings;
        }
    }
}
