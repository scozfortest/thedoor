
using UnityEditor;
using System;
using UnityEngine;

public class UnityJavaSDKFix  {

    [InitializeOnLoadMethod]
    static void SetJavaHome() {

        if(Application.platform== RuntimePlatform.OSXEditor) {
            Debug.Log("JAVA_HOME in editor 位置為: " + Environment.GetEnvironmentVariable("JAVA_HOME"));

            string newJDKPath = EditorApplication.applicationPath.Replace("Unity.app", "PlaybackEngines/AndroidPlayer/OpenJDK");

            if (Environment.GetEnvironmentVariable("JAVA_HOME") != newJDKPath) {
                Environment.SetEnvironmentVariable("JAVA_HOME", newJDKPath);
            }

            Debug.Log("JAVA_HOME in editor 設定為: " + Environment.GetEnvironmentVariable("JAVA_HOME"));
        }


    }
}