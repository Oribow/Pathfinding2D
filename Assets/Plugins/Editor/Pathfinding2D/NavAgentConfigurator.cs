using UnityEngine;
using System;
using UnityEditor;

namespace NavGraph.Build
{
    [Serializable]
    public class NavAgentConfigurator //: ITab
    {/*
        public bool IsEnabled
        {
            get
            {
                return navBuilder.GlobalBuildContainer != null;
            }
        }

        public string TabHeader
        {
            get
            {
                return "AgentConfig";
            }
        }

        NavAgentGroundWalkerSettings navSet { get { return navBuilder.GlobalBuildContainer.navAgentSettings; } set { navBuilder.GlobalBuildContainer.navAgentSettings = value; } }
        INavDataBuilder navBuilder;

        public NavAgentConfigurator(INavDataBuilder navBuilder)
        {
            this.navBuilder = navBuilder;
        }

        public void OnSceneGUI(SceneView sceneView)
        {

        }

        public void OnSelected()
        {

        }

        public void OnUnselected()
        {

        }

        public void OnGUI()
        {
            if (navSet != null)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.LabelField("NavAgent Settings:", EditorStyles.boldLabel);
                navSet.height = Mathf.Max(EditorGUILayout.FloatField("Height", navSet.height), 0.01f);
                navSet.width = Mathf.Max(EditorGUILayout.FloatField("Width", navSet.width), 0.01f);
                navSet.maxXVel = Mathf.Max(EditorGUILayout.FloatField("Max X Vel", navSet.maxXVel), 0.01f);
                navSet.slopeLimit = Mathf.Clamp(EditorGUILayout.FloatField("Slope Limit", navSet.slopeLimit), 0, 60);
                EditorGUILayout.Space();
                if (EditorGUI.EndChangeCheck())
                    EditorUtility.SetDirty(navSet);

                /*IMovementEntity movEntity = null;
                if (Selection.activeGameObject != null)
                    movEntity = Selection.activeGameObject.GetComponentInChildren<IMovementEntity>();

                GUI.enabled = movEntity != null && navSet != null;
                if (GUILayout.Button("Copy from selected MovingActor"))
                {
                    CopyValuesFromCC2DMotor(movEntity);
                    EditorUtility.SetDirty(navSet);
                }
            }

            
            GUI.enabled = navSet != null;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save As New"))
            {
               Save();
            }
            GUI.enabled = true;
            if (GUILayout.Button("Create New"))
            {
                CreateNew();
            }
            GUILayout.EndHorizontal();
        }

        void CopyValuesFromCC2DMotor(IMovementEntity iMovementEntity)
        {
            /*Renderer[] renderer = iMovementEntity.GetComponentsInChildren<Renderer>();
            float minHeight = renderer[0].bounds.min.y;
            float maxHeight = renderer[0].bounds.max.y;
            float minWidth = renderer[0].bounds.min.x;
            float maxWidth = renderer[0].bounds.max.x;
            foreach (Renderer r in renderer)
            {
                maxHeight = Mathf.Max(maxHeight, r.bounds.max.y);
                minHeight = Mathf.Min(minHeight, r.bounds.min.y);
                maxWidth = Mathf.Max(maxWidth, r.bounds.max.x);
                minWidth = Mathf.Min(minWidth, r.bounds.min.x);
            }
            navSet.height = Mathf.Max(0.01f, maxHeight - minHeight);
            navSet.width = Mathf.Max(0.01f, maxWidth - minWidth);

            SerializedObject spMotor = new SerializedObject(iMovementEntity.CC2DMotor);
            navSet.maxXVel = spMotor.FindProperty("walkHMaxSpeed").floatValue;
            navSet.gravity = spMotor.FindProperty("gravityAcceleration").floatValue;
            navSet.jumpForce = Mathf.Sqrt(spMotor.FindProperty("jumpMaxHeight").floatValue * navSet.gravity * 2);

            SerializedObject spCC2D = new SerializedObject(iMovementEntity.CharacterController2D);
            navSet.slopeLimit = spCC2D.FindProperty("slopeLimit").floatValue;
        }

        void CreateNew()
        {
            navSet = ScriptableObject.CreateInstance<NavAgentGroundWalkerSettings>();
        }

        void Save()
        {
            string path = EditorUtility.SaveFilePanel("Save NavAgentSettings", "Assets", "GroundWalkerSettings", "asset");
            if (path == null || path.Length == 0)
                return;
            path = path.Substring(path.IndexOf("Assets"));
            Debug.Log(path);
            AssetDatabase.CreateAsset(navSet, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = navSet;
        }*/
    }
}