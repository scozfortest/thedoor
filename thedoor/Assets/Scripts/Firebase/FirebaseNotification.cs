using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AppsFlyerSDK;
using Scoz.Func;

namespace TheDoor.Main {
    public partial class FirebaseManager {
        public static void InitNotification() {
            DebugLogger.Log("Init Firebase Notification");
            Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
            Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;
        }

        public static void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token) {
            DebugLogger.Log("Received Registration Token: " + token.Token);
#if !UNITY_EDITOR && APPSFLYER && UNITY_ANDROID
            AppsFlyer.updateServerUninstallToken(token.Token);
#endif
        }

        public static void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e) {
            DebugLogger.Log("Received a new message from: " + e.Message.From);
        }
    }
}