using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Firebase.Auth;
using Scoz.Func;
using System.IO;
using Firebase;

public class FirebaseEditor : MonoBehaviour {
    [MenuItem("MaJamPachinko/Signout Firebase")]
    public static void SignoutFirebaseAuth() {

        EnvVersion envVersion = EnvVersion.Dev;
#if Dev
        envVersion = EnvVersion.Dev;
#elif Test
            envVersion= EnvVersion.Test;
#elif Release
            envVersion= EnvVersion.Release;
#endif

        //讀取google-services.json來更改Firebase專案
        var path = Application.streamingAssetsPath + "/google-services.json";
        var jsonText = File.ReadAllText(path);
        AppOptions appOptions = AppOptions.LoadFromJsonConfig(jsonText);
        FirebaseApp.Create(appOptions, envVersion.ToString());
        var firebaseApp = FirebaseApp.GetInstance(envVersion.ToString());
        var auth = FirebaseAuth.GetAuth(firebaseApp);
        if (auth.CurrentUser != null) {
            DebugLogger.Log("Signout Firebase Auth:" + auth.CurrentUser.UserId);
            DebugLogger.Log("PlayerPrefs.DeleteAll");
            auth.SignOut();
        } else
            DebugLogger.Log("Already Signout");
        PlayerPrefs.DeleteAll();//刪除Firebase也一併清除本機資料，頭像才不會記錄
    }
}
