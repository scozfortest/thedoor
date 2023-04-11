using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEditor.iOS.Xcode;
using System.IO;
using System;
using UnityEditor.Callbacks;

class PostprocessBuild : IPostprocessBuildWithReport {
    public int callbackOrder { get { return 0; } }

    
    [PostProcessBuild(45)]//must be between 40 and 50 to ensure that it's not overriden by Podfile generation (40) and that it's added before "pod install" (50)
    public static void FixPodFile(BuildTarget buildTarget,string buildPath)
    {
        
        if (buildTarget != BuildTarget.iOS) return;
        
        Debug.Log("//////////////////////設定Podfile/////////////////////////////");
        using (StreamWriter sw = File.AppendText(buildPath + "/Podfile"))
        {
            /*
            sw.WriteLine("post_install do |installer|");
            sw.WriteLine("installer.generated_projects.each do |project|");
            sw.WriteLine("project.targets.each do |target|");
            sw.WriteLine("target.build_configurations.each do |config|");
            
            sw.WriteLine("config.build_settings[\"DEVELOPMENT_TEAM\"] = \"YTWUUZQF9A\"");
            sw.WriteLine("end\nend\nend\nend");
            */
            
            
            sw.WriteLine("post_install do |installer|");
            sw.WriteLine("installer.pods_project.targets.each do |target|");
            sw.WriteLine("target.build_configurations.each do |config|");

            sw.WriteLine("config.build_settings[\"DEVELOPMENT_TEAM\"] = \"YTWUUZQF9A\"");
            sw.WriteLine("end\nend\nend");
            
        }


    }

    
    public void OnPostprocessBuild(BuildReport report) {

        if (report.summary.platform != BuildTarget.iOS) return;



        string projectPath = report.summary.outputPath + "/Unity-iPhone.xcodeproj/project.pbxproj";



        PBXProject pbxProject = new PBXProject();
            pbxProject.ReadFromFile(projectPath);
            //Main
            string target = pbxProject.GetUnityMainTargetGuid();
            Debug.Log("//////////////////////設定Xcode Info.plist/////////////////////////////");
            string infoPlistPath = Path.Combine("../../xcode_build", "Info.plist");
            var propertyList = new PlistDocument();
            propertyList.ReadFromFile(infoPlistPath);
            //設定藍芽說明
            propertyList.root.SetString(
                "NSBluetoothAlwaysUsageDescription",
                "此裝置會使用藍芽獲取鄰近裝置，用於抓取好運遙控器");
            propertyList.root.SetString(
                "NSBluetoothPeripheralUsageDescription",
                "此裝置會使用藍芽獲取鄰近裝置，用於抓取好運遙控器");
            //設定名稱
            propertyList.root.SetString(
            "CFBundleDisplayName",
            "麻將柏青哥");
            //設定追蹤資料說明
            propertyList.root.SetString(
            "NSUserTrackingUsageDescription",
            "僅收集玩家資訊以追蹤遊戲Bug如何產生與優化遊戲體驗使用");
            //設定區域
            var locationArray = propertyList.root.CreateArray("CFBundleLocalizations");
            var localizations = new string[] { "zh_TW", "en" };
            foreach (var localization in localizations) locationArray.AddString(localization);

            //新增Support URLs
            var array = propertyList.root.CreateArray("CFBundleURLTypes");
            var urlDict = array.AddDict();
            urlDict.SetString("CFBundleURLName", "com.among.majampachinkorelease");
            var urlInnerArray = urlDict.CreateArray("CFBundleURLSchemes");
            urlInnerArray.AddString("majampachinko");

            //寫入
            propertyList.WriteToFile(infoPlistPath);

            Debug.Log("//////////////////////設定Xcode Signing & Capabilities/////////////////////////////");
            var capabilityManager = new ProjectCapabilityManager(projectPath, "Entitlements.entitlements", null, target);
            capabilityManager.AddAssociatedDomains(new string[] { "applinks:majampachinko.onelink.me" });
            capabilityManager.AddBackgroundModes(BackgroundModesOptions.BackgroundFetch);
            try
            {
                capabilityManager.AddBackgroundModes(BackgroundModesOptions.RemoteNotifications);
            }
            catch(Exception _e)
            {
                Debug.LogWarning(_e);
            }

            capabilityManager.AddPushNotifications(false);
            capabilityManager.WriteToFile();


            Debug.Log("//////////////////////設定Xcode Framework Settings/////////////////////////////");
            pbxProject.AddFrameworkToProject(target, "AppTrackingTransparency.framework", false);
            pbxProject.AddFrameworkToProject(target, "UserNotifications.framework", false);


            Debug.Log("//////////////////////設定Xcode Build Settings/////////////////////////////");
            //Disabling Bitcode on all targets
            pbxProject.SetBuildProperty(target, "ENABLE_BITCODE", "NO");
            //Unity Tests
            var unityTestTarget = pbxProject.TargetGuidByName(PBXProject.GetUnityTestTargetName());
            pbxProject.SetBuildProperty(unityTestTarget, "ENABLE_BITCODE", "NO");
            //Unity Framework
            var unityFrameWorkTarget = pbxProject.GetUnityFrameworkTargetGuid();
            pbxProject.SetBuildProperty(unityFrameWorkTarget, "ENABLE_BITCODE", "NO");
            pbxProject.SetBuildProperty(unityFrameWorkTarget, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");
            pbxProject.WriteToFile(projectPath);

    }
}