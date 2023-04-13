using UnityEngine;
using System.Collections;
using UnityEngine.Android;
using System;
using System.Collections.Generic;

namespace Scoz.Func {

    public class AndroidPermission : MonoBehaviour {
        //推播權限
        static string[] LaunchPermissions = new string[] {
         //藍芽相關
        "android.permission.BLUETOOTH_SCAN",
        "android.permission.BLUETOOTH",
        "android.permission.BLUETOOTH_ADMIN",
        "android.permission.BLUETOOTH_CONNECT",

        //"android.permission.ACCESS_COARSE_LOCATION",
        //"android.permission.ACCESS_FINE_LOCATION",
        //"android.permission.ACCESS_BACKGROUND_LOCATION",
        };

        //推播權限
        static string[] NotificationPermissions = new string[] {
        "android.permission.SCHEDULE_EXACT_ALARM",
        "android.permission.USE_EXACT_ALARM",
        };

        public static bool CheckPermission(string _permissionStr) {
            return Permission.HasUserAuthorizedPermission(_permissionStr);
        }
        public void RequestNotificationPermissions() {
            List<string> needPermissions = new List<string>();
            for (int i = 0; i < NotificationPermissions.Length; i++) {
                if (CheckPermission(NotificationPermissions[i]) == false)
                    needPermissions.Add(NotificationPermissions[i]);
            }
            if (needPermissions.Count == 0) return;
            Debug.LogError("//////開始請求Notification Permissions//////");
            for (int i = 0; i < needPermissions.Count; i++)
                Debug.LogError("Permission: " + needPermissions[i]);
            var callbacks = new PermissionCallbacks();
            callbacks.PermissionDenied += OnPermissionDenied;
            callbacks.PermissionGranted += OnPermissionGranted;
            callbacks.PermissionDeniedAndDontAskAgain += OnPermissionDeniedAndDontAskAgain;
            Permission.RequestUserPermissions(needPermissions.ToArray(), callbacks);
        }
        public void RequestLaunchPermissions() {
            List<string> needPermissions = new List<string>();
            for (int i = 0; i < LaunchPermissions.Length; i++) {
                if (CheckPermission(LaunchPermissions[i]) == false)
                    needPermissions.Add(LaunchPermissions[i]);
            }
            if (needPermissions.Count == 0) return;
            //Debug.LogError("//////開始請求Permissions//////");
            //for (int i = 0; i < needPermissions.Count; i++)
            //    Debug.LogError("Permission: " + needPermissions[i]);
            var callbacks = new PermissionCallbacks();
            callbacks.PermissionDenied += OnPermissionDenied;
            callbacks.PermissionGranted += OnPermissionGranted;
            callbacks.PermissionDeniedAndDontAskAgain += OnPermissionDeniedAndDontAskAgain;
            Permission.RequestUserPermissions(needPermissions.ToArray(), callbacks);
        }
        void OnPermissionDenied(string _cbStr) {
            WriteLog.LogError("OnPermissionDenied: " + _cbStr);
        }
        void OnPermissionGranted(string _cbStr) {
            WriteLog.LogError("OnPermissionGranted: " + _cbStr);
        }
        void OnPermissionDeniedAndDontAskAgain(string _cbStr) {
            WriteLog.LogError("OnPermissionDeniedAndDontAskAgain: " + _cbStr);
        }
    }
}
