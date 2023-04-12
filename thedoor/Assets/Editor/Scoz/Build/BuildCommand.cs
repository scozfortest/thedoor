using System.Collections;
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using System;
using UnityEditor.AddressableAssets;
using Unity.EditorCoroutines.Editor;
using Scoz.Func;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace Scoz.Editor {
    public class BuildCommand {

        private static string[] BuildScenes = { "Assets/Scenes/StartScene.unity", "Assets/Scenes/LobbyScene.unity" };
        private static string ANDROID_MANIFEST_PATH = "Assets/Plugins/Android/AndroidManifest.xml";
        static object owner = new System.Object();

        private static IEnumerator BuildAssetBundleAsync(EnvVersion _version, Action callback) {

            SwitchVersion.RunSwitchVersion(_version, result => {
                if (!result) {
                    Debug.LogError("RunSwitchVersion Failed.");
                    callback?.Invoke();
                    return;
                } else {
                    Debug.Log("Start Build Bundle EnvVersion: " + _version);
                    AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult buildResult);
                    if (!string.IsNullOrEmpty(buildResult.Error)) {
                        Debug.LogError("Build Bundle Error.");
                        callback?.Invoke();
                    }
                    callback?.Invoke();
                }

            });
            yield return null;
        }

        private static IEnumerator UpdateAssetBundleAsync(EnvVersion _version, Action callback) {
            SwitchVersion.RunSwitchVersion(_version, result => {

                Debug.Log("Switch EnvVersion Success: " + result);
                if (!result) {
                    Debug.LogError("RunSwitchVersion Failed.");
                    callback?.Invoke();
                    return;
                } else {
                    Debug.Log("Start Build Bundle");
                    var path = ContentUpdateScript.GetContentStateDataPath(false);
                    if (!string.IsNullOrEmpty(path)) {
                        Debug.Log("Update Bundle at path : " + path);
                        ContentUpdateScript.BuildContentUpdate(AddressableAssetSettingsDefaultObject.Settings, path);
                        callback?.Invoke();
                    } else {
                        Debug.LogError("ContentUpdateScript path is null.");
                        callback?.Invoke();
                    }
                }
            });
            yield return null;

        }

        public static void SetPlayerSettingsVersion() {
            string[] args = System.Environment.GetCommandLineArgs();
            string version = "";
            string versionCode = "";
            for (int i = 0; i < args.Length; i++) {
                Debug.Log("ARG " + i + ": " + args[i]);
                switch (args[i]) {
                    case "-buildVersion":
                        version = args[i + 1];
                        break;
                    case "-buildVersionCode":
                        versionCode = args[i + 1];
                        break;
                }
            }
            PlayerSettings.bundleVersion = version;
            PlayerSettings.Android.bundleVersionCode = int.Parse(versionCode);
            Close();
        }

        public static void BuildBundleWithArg() {
            EditorSceneManager.OpenScene($"Assets/Scenes/" + MyScene.StartScene.ToString() + ".unity");
            string[] args = System.Environment.GetCommandLineArgs();
            EnvVersion envVersion = EnvVersion.Dev;
            string version = "";
            for (int i = 0; i < args.Length; i++) {
                Debug.Log("ARG " + i + ": " + args[i]);
                switch (args[i]) {
                    case "-enviorment":
                        if (!MyEnum.TryParseEnum(args[i + 1], out envVersion)) {
                            Debug.LogError("傳入的版本參數錯誤");
                            Close();
                            return;
                        }
                        break;
                    case "-buildVersion":
                        version = args[i + 1];
                        break;
                }
            }
            if (string.IsNullOrEmpty(version)) {
                Debug.LogError("version argument is not set.");
                Close();
                return;
            }
            PlayerSettings.bundleVersion = version;
            EditorCoroutine editorCoroutine = EditorCoroutineUtility.StartCoroutine(BuildAssetBundleAsync(envVersion, Close), owner);
        }
        [MenuItem("Scoz/ForTest/BuildBundleWithArg_Test")]
        public static void BuildBundleWithArg_Test() {
            EnvVersion envVersion = EnvVersion.Dev;
#if Dev
            envVersion = EnvVersion.Dev;
#elif Test
               envVersion=EnvVersion.Test;
#elif Release
            envVersion = EnvVersion.Release;
#endif
            EditorCoroutine editorCoroutine = EditorCoroutineUtility.StartCoroutine(BuildAssetBundleAsync(envVersion, Close), owner);
        }

        public static void UpdateBundleWithArg() {
            EditorSceneManager.OpenScene($"Assets/Scenes/" + MyScene.StartScene.ToString() + ".unity");
            string[] args = System.Environment.GetCommandLineArgs();
            EnvVersion envVersion = EnvVersion.Dev;
            string version = "";
            for (int i = 0; i < args.Length; i++) {
                Debug.Log("ARG " + i + ": " + args[i]);
                switch (args[i]) {
                    case "-enviorment":
                        if (!MyEnum.TryParseEnum(args[i + 1], out envVersion)) {
                            Debug.LogError("傳入的版本參數錯誤");
                            Close();
                            return;
                        }
                        break;
                    case "-buildVersion":
                        version = args[i + 1];
                        break;
                }
            }
            if (string.IsNullOrEmpty(version)) {
                Debug.LogError("version argument is not set.");
                Close();
                return;
            }
            PlayerSettings.bundleVersion = version;
            EditorCoroutine editorCoroutine = EditorCoroutineUtility.StartCoroutine(UpdateAssetBundleAsync(envVersion, Close), owner);
        }
        [MenuItem("Scoz/ForTest/UpdateBundleWithArg_Test")]
        public static void UpdateBundleWithArg_Test() {
            EnvVersion envVersion = EnvVersion.Dev;
#if Dev
            envVersion = EnvVersion.Dev;
#elif Test
               envVersion=EnvVersion.Test;
#elif Release
            envVersion = EnvVersion.Release;
#endif
            EditorCoroutine editorCoroutine = EditorCoroutineUtility.StartCoroutine(UpdateAssetBundleAsync(envVersion, null), owner);
        }
        public static void BuildAPK() {
            string[] args = System.Environment.GetCommandLineArgs();
            EnvVersion envVersion = EnvVersion.Dev;
            string version = "";
            string versionCode = "";
            string keyaliasPass = "";
            string keystorePass = "";
            string outputFileName = "";
            for (int i = 0; i < args.Length; i++) {
                Debug.Log("ARG " + i + ": " + args[i]);
                switch (args[i]) {
                    case "-enviorment":
                        if (!MyEnum.TryParseEnum(args[i + 1], out envVersion)) {
                            Debug.LogError("傳入的版本參數錯誤");
                            Close();
                            return;
                        }
                        break;
                    case "-buildVersion":
                        version = args[i + 1];
                        break;
                    case "-buildVersionCode":
                        versionCode = args[i + 1];
                        break;
                    case "-keyaliasPass":
                        keyaliasPass = args[i + 1];
                        break;
                    case "-keystorePass":
                        keystorePass = args[i + 1];
                        break;
                    case "-outputFileName":
                        outputFileName = args[i + 1];
                        break;
                }
            }
            Debug.LogFormat("輸出APK位置: {0}", outputFileName);
            PlayerSettings.bundleVersion = version;
            PlayerSettings.Android.bundleVersionCode = int.Parse(versionCode);
            PlayerSettings.keyaliasPass = keyaliasPass;
            PlayerSettings.keystorePass = keystorePass;
            EditorCoroutineUtility.StartCoroutine(BuildAPKAsync(envVersion, outputFileName, Close), owner);
        }

        public static void BuildAAB() {
            string[] args = System.Environment.GetCommandLineArgs();
            EnvVersion envVersion = EnvVersion.Dev;
            string version = "";
            string versionCode = "";
            string keyaliasPass = "";
            string keystorePass = "";
            string outputFileName = "";
            for (int i = 0; i < args.Length; i++) {
                Debug.Log("ARG " + i + ": " + args[i]);
                switch (args[i]) {
                    case "-enviorment":
                        if (!MyEnum.TryParseEnum(args[i + 1], out envVersion)) {
                            Debug.LogError("傳入的版本參數錯誤");
                            Close();
                            return;
                        }
                        break;
                    case "-buildVersion":
                        version = args[i + 1];
                        break;
                    case "-buildVersionCode":
                        versionCode = args[i + 1];
                        break;
                    case "-keyaliasPass":
                        keyaliasPass = args[i + 1];
                        break;
                    case "-keystorePass":
                        keystorePass = args[i + 1];
                        break;
                    case "-outputFileName":
                        outputFileName = args[i + 1];
                        break;
                }
            }
            Debug.LogFormat("輸出AAB位置: {0}", outputFileName);
            PlayerSettings.bundleVersion = version;
            PlayerSettings.Android.bundleVersionCode = int.Parse(versionCode);
            PlayerSettings.keyaliasPass = keyaliasPass;
            PlayerSettings.keystorePass = keystorePass;
            EditorCoroutineUtility.StartCoroutine(BuildAabAsync(envVersion, outputFileName, Close), owner);
        }

        [MenuItem("Scoz/ForTest/BuildAAB_Test")]
        public static void BuildAAB_Test() {
            string[] args = System.Environment.GetCommandLineArgs();
            EnvVersion envVersion = EnvVersion.Dev;
            string version = "1.1.1";
            string versionCode = "1";
            string keyaliasPass = "amongus";
            string keystorePass = "amongus";
            string outputFileName = "../../TheDoor.aab";
            Debug.LogFormat("輸出AAB位置: {0}", outputFileName);
            PlayerSettings.bundleVersion = version;
            PlayerSettings.Android.bundleVersionCode = int.Parse(versionCode);
            PlayerSettings.keyaliasPass = keyaliasPass;
            PlayerSettings.keystorePass = keystorePass;
            EditorCoroutineUtility.StartCoroutine(BuildAabAsync(envVersion, outputFileName, null), owner);
        }
        private static void Close() {
            EditorApplication.Exit(0);
        }


        private static IEnumerator BuildAPKAsync(EnvVersion envVersion, string outputFileName, Action callback) {
            //設定
            EditorUserBuildSettings.development = true;
            EditorUserBuildSettings.buildAppBundle = false;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;
            PlayerSettings.Android.useAPKExpansionFiles = false;
            ModifyAndroidManifest.ModifyDebuggable(ANDROID_MANIFEST_PATH, true);
            //切換環境
            SwitchVersion.RunSwitchVersion(envVersion, result => {

                if (!result) {
                    Debug.LogError("RunSwitchVersion Failed.");
                    callback?.Invoke();
                    return;
                } else {
                    BuildPipeline.BuildPlayer(BuildScenes, outputFileName, BuildTarget.Android, BuildOptions.None);
                    callback?.Invoke();
                }

            });
            yield return null;
        }

        private static IEnumerator BuildAabAsync(EnvVersion _envVersion, string outputFileName, Action callback) {
            //設定
            EditorUserBuildSettings.development = false;
            EditorUserBuildSettings.buildAppBundle = true;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
            PlayerSettings.Android.useAPKExpansionFiles = true;
            ModifyAndroidManifest.ModifyDebuggable(ANDROID_MANIFEST_PATH, false);


            SwitchVersion.RunSwitchVersion(_envVersion, result => {

                if (!result) {
                    Debug.LogError("RunSwitchVersion Failed.");
                    callback?.Invoke();
                    return;
                } else {
                    BuildPipeline.BuildPlayer(BuildScenes, outputFileName, BuildTarget.Android, BuildOptions.None);
                    callback?.Invoke();
                }
            });
            yield return null;


        }

        public static void BuildXcode() {
            string[] args = System.Environment.GetCommandLineArgs();
            EnvVersion envVersion = EnvVersion.Dev;
            string version = "";
            string versionCode = "";
            string outputFileName = "";
            for (int i = 0; i < args.Length; i++) {
                Debug.Log("ARG " + i + ": " + args[i]);
                switch (args[i]) {
                    case "-enviorment":
                        if (!MyEnum.TryParseEnum(args[i + 1], out envVersion)) {
                            Debug.LogError("傳入的版本參數錯誤");
                            Close();
                            return;
                        }
                        break;
                    case "-buildVersion":
                        version = args[i + 1];
                        break;
                    case "-buildVersionCode":
                        versionCode = args[i + 1];
                        break;
                    case "-outputFileName":
                        outputFileName = args[i + 1];
                        break;
                }
            }
            Debug.LogFormat("輸出Xcode Project位置: {0}", outputFileName);
            PlayerSettings.bundleVersion = version;
            PlayerSettings.iOS.buildNumber = versionCode;

            try {
                EditorCoroutineUtility.StartCoroutine(BuildXcodeAsync(envVersion, outputFileName, Close), owner);
            } catch (Exception _e) {

                Debug.LogError("BuildXcode發生錯誤: " + _e);
            }
        }
        [MenuItem("Scoz/ForTest/BuildXcode_Test")]
        public static void BuildXcode_Test() {
            EnvVersion envVersion = EnvVersion.Dev;
#if Dev
            envVersion = EnvVersion.Dev;
#elif Test
               envVersion=EnvVersion.Test;
#elif Release
            envVersion = EnvVersion.Release;
#endif
            string outputFileName = "../../xcode_build";
            Debug.LogFormat("輸出Xcode Project位置: {0}", outputFileName);
            try {
                EditorCoroutineUtility.StartCoroutine(BuildXcodeAsync(envVersion, outputFileName, null), owner);
            } catch (Exception _e) {
                Debug.LogError("BuildXcode發生錯誤: " + _e);
            }
        }
        private static IEnumerator BuildXcodeAsync(EnvVersion evnVersion, string outputFileName, Action callback) {
            SwitchVersion.RunSwitchVersion(evnVersion, result => {

                if (!result) {
                    Debug.LogError("RunSwitchVersion Failed.");
                    callback?.Invoke();
                    return;
                } else {
                    Debug.Log("Start BuildXcodeAsync");
                    BuildPipeline.BuildPlayer(BuildScenes, outputFileName, BuildTarget.iOS, BuildOptions.AcceptExternalModificationsToPlayer);

                    callback?.Invoke();
                }

            });
            yield return null;
        }
    }
}