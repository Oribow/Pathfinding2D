using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using NavGraph.Build;
using System.Text;

namespace NavGraph.EditorUI
{
    public class BuildStepUIList
    {
        const float circleTextureHeight = 20;
        const float circleMarginLeft = 7;
        const float circleMarginUp = 7f;
        const float lineWidth = 5;
        static Color lineOutOfDate = new Color(0.7215f, 0.7529f, 0.7843f);
        static Color lineOK = new Color(0.2f, 0.6f, 1);
        static Color lineNotBuild = new Color(0.7215f, 0.7529f, 0.7843f);

        public int BuildStepCount { get { return buildSteps.Count; } }

        List<BuildStepUIElement> buildSteps;
        Texture circleFilled;
        Texture circleGrayed;
        Texture circleOutdated;
        GUIStyle displayNameStyle;

        public BuildStepUIList()
        {
            buildSteps = new List<BuildStepUIElement>(5);
            circleFilled = EditorAssetLoadHelper.LoadAssetRequired<Texture>(EditorAssetLoadHelper.BuildStepList_CircleFilled);
            circleGrayed = EditorAssetLoadHelper.LoadAssetRequired<Texture>(EditorAssetLoadHelper.BuildStepList_CircleGrayed);
            circleOutdated = EditorAssetLoadHelper.LoadAssetRequired<Texture>(EditorAssetLoadHelper.BuildStepList_CircleOutdated);
        }

        public void AddBuildStep(BuildStepUIElement buildStep)
        {
            buildSteps.Add(buildStep);
        }

        public void RemoveBuildStep(BuildStepUIElement buildStep)
        {
            buildSteps.Remove(buildStep);
        }

        public BuildStepUIElement GetBuildStepAt(int index)
        {
            return buildSteps[index];
        }

        public void DrawLayout()
        {
            if (buildSteps.Count == 0)
                return;
            if (displayNameStyle == null) //Doesn't allow me to set this in the constructor for some reason :(
                displayNameStyle = EditorStyles.largeLabel;

            Rect[] circleRects = new Rect[buildSteps.Count];
            bool restoreEnable = GUI.enabled;
            for (int iStep = 0; iStep < buildSteps.Count; iStep++)
            {
                var step = buildSteps[iStep];
                GUI.enabled = restoreEnable && step.isEnabled;
                EditorGUILayout.BeginHorizontal();

                Rect textureRect = GUILayoutUtility.GetRect(circleTextureHeight + circleMarginLeft, circleTextureHeight + circleMarginUp, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
                textureRect.x += circleMarginLeft;
                textureRect.width -= circleMarginLeft;
                textureRect.y += circleMarginUp;
                textureRect.height -= circleMarginUp;
                circleRects[iStep] = textureRect;

                EditorGUILayout.BeginVertical();

                //Display display name
                GUILayout.Label(step.DisplayName, displayNameStyle);

                EditorGUI.indentLevel++;

                //Display informational notification
                StringBuilder informationalStringBuilder = new StringBuilder(300);
                foreach (var info in step.InformationalNotification)
                {
                    EditorGUILayout.LabelField(info());
                    informationalStringBuilder.AppendLine(info());
                }
                if (informationalStringBuilder.Length > 0)
                    informationalStringBuilder.Remove(informationalStringBuilder.Length - 1, 1);
                //EditorGUILayout(informationalStringBuilder.ToString(), MessageType.None);

                //Display warnings and errors
                if (step.Notifications.Count > 0)
                {
                    StringBuilder warningStringBuilder = new StringBuilder(300);
                    StringBuilder errorStringBuilder = new StringBuilder(300);
                    foreach (var notification in step.Notifications)
                    {
                        if (notification.notificationType == BuildStepUIElement.BuildNotification.NotificationType.Error)
                            errorStringBuilder.AppendLine(notification.notificationContent);
                        else
                            warningStringBuilder.AppendLine(notification.notificationContent);
                    }

                    if (warningStringBuilder.Length > 0)
                    {
                        warningStringBuilder.Remove(warningStringBuilder.Length - 1, 1);
                        EditorGUILayout.HelpBox(warningStringBuilder.ToString(), MessageType.Warning);
                    }
                    if (errorStringBuilder.Length > 0)
                    {
                        errorStringBuilder.Remove(errorStringBuilder.Length - 1, 1);
                        EditorGUILayout.HelpBox(errorStringBuilder.ToString(), MessageType.Error);
                    }
                }

                EditorGUILayout.BeginHorizontal();
                bool restoreEnableButton = GUI.enabled;
                for (int iButton = 0; iButton < step.ButtonDisplayNames.Length; iButton++)
                {
                    GUI.enabled = restoreEnableButton && step.ButtonEnabled[iButton];
                    if (GUILayout.Button(step.ButtonDisplayNames[iButton]))
                    {
                        step.InvokeButtonDown(iButton);
                    }
                }
                GUI.enabled = restoreEnableButton;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }
            GUI.enabled = restoreEnable;

            //Draw lines now
            Rect lineRect = new Rect(circleRects[0].x + circleRects[0].width / 2 - lineWidth / 2, circleRects[0].y + circleRects[0].height / 2, lineWidth, 0);
            for (int iStep = 0; iStep < buildSteps.Count - 1; iStep++)
            {
                lineRect.height = (circleRects[iStep + 1].y + circleRects[iStep + 1].height / 2) - lineRect.y;
                var step = buildSteps[iStep + 1];
                switch (step.buildStepStatus)
                {
                    case BuildStepUIElement.BuildStepStatus.Notbuilt: EditorGUI.DrawRect(lineRect, lineNotBuild); break;
                    case BuildStepUIElement.BuildStepStatus.OK: EditorGUI.DrawRect(lineRect, lineOK); break;
                    case BuildStepUIElement.BuildStepStatus.Outdated: EditorGUI.DrawRect(lineRect, lineOutOfDate); break;
                }
                lineRect.y += lineRect.height;
            }

            //Draw circles now
            for (int iStep = 0; iStep < buildSteps.Count; iStep++)
            {
                var step = buildSteps[iStep];
                switch (step.buildStepStatus)
                {
                    case BuildStepUIElement.BuildStepStatus.Notbuilt: GUI.DrawTexture(circleRects[iStep], circleGrayed, ScaleMode.ScaleToFit); break;
                    case BuildStepUIElement.BuildStepStatus.OK: GUI.DrawTexture(circleRects[iStep], circleFilled, ScaleMode.ScaleToFit); break;
                    case BuildStepUIElement.BuildStepStatus.Outdated: GUI.DrawTexture(circleRects[iStep], circleOutdated, ScaleMode.ScaleToFit); break;
                }
            }
        }
    }
}
