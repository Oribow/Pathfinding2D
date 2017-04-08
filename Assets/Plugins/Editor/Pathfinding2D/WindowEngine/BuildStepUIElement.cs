using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NavGraph.Build
{
    public class BuildStepUIElement
    {
        public enum BuildStepStatus { OK, Outdated, Notbuilt };
        public delegate string NotificationContentCallback();
        public delegate void OnButtonDown(int buttonIndex);

        public string DisplayName { get { return displayName; } }
        public string[] ButtonDisplayNames { get { return buttonDisplayNames; } }
        public bool[] ButtonEnabled { get { return buttonEnabled; } }

        public List<BuildNotification> Notifications { get { return notifications; } }
        public List<NotificationContentCallback> InformationalNotification { get { return informationalNotification; } }
        public BuildStepStatus buildStepStatus;
        public bool isEnabled;

        List<BuildNotification> notifications;
        List<NotificationContentCallback> informationalNotification;
        string displayName;
        string[] buttonDisplayNames;
        bool[] buttonEnabled;
        OnButtonDown buttonDownHandler;

        public BuildStepUIElement(string displayName, string[] buttonDisplayNames)
        {
            notifications = new List<BuildNotification>();
            informationalNotification = new List<NotificationContentCallback>();
            buildStepStatus = BuildStepStatus.Notbuilt;
            this.displayName = displayName;
            this.buttonDisplayNames = buttonDisplayNames;
            this.buttonEnabled = new bool[buttonDisplayNames.Length];
            for (int iButton = 0; iButton < buttonEnabled.Length; iButton++)
            {
                buttonEnabled[iButton] = true;
            }
            isEnabled = true;
        }

        public void AddErrorNotification(string messageContent)
        {
            notifications.Add(new BuildNotification(BuildNotification.NotificationType.Error, messageContent));
        }

        public void AddWarningNotification(string messageContent)
        {
            notifications.Add(new BuildNotification(BuildNotification.NotificationType.Warning, messageContent));
        }

        public void AddInformationalNotification(NotificationContentCallback messageContent)
        {
            informationalNotification.Add(messageContent);
        }

        public void ClearErrorsWarningsNotifications()
        {
            notifications.Clear();
        }

        public void ClearInformationalNotifications()
        {
            informationalNotification.Clear();
        }

        public void InvokeButtonDown(int buttonIndex)
        {
            if (buttonDownHandler != null)
                buttonDownHandler.Invoke(buttonIndex);
        }

        public void SetButtonDownListener(OnButtonDown listener)
        {
            buttonDownHandler = listener;
        }

        public class BuildNotification
        {
            public enum NotificationType { Error, Warning }
            public NotificationType notificationType;
            public string notificationContent;

            public BuildNotification(NotificationType messageType, string messageContent)
            {
                this.notificationType = messageType;
                this.notificationContent = messageContent;
            }
        }
    }
}
