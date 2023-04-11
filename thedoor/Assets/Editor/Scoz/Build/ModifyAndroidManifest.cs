
using System.IO;
using System.Text;
using System.Xml;
using UnityEditor.Android;

namespace Scoz.Editor {

    public class ModifyAndroidManifest {
        public static void ModifyDebuggable(string basePath, bool isDebugable) {
            // If needed, add condition checks on whether you need to run the modification routine.
            // For example, specific configuration/app options enabled

            var androidManifest = new AndroidManifest(basePath);

            androidManifest.SetApplicationDebuable(isDebugable);

            // Add your XML manipulation routines

            androidManifest.Save();
        }

        private static string _manifestFilePath;

        private static string GetManifestPath(string basePath) {
            if (string.IsNullOrEmpty(_manifestFilePath)) {
                var pathBuilder = new StringBuilder(basePath);
                pathBuilder.Append(Path.DirectorySeparatorChar).Append("src");
                pathBuilder.Append(Path.DirectorySeparatorChar).Append("main");
                pathBuilder.Append(Path.DirectorySeparatorChar).Append("AndroidManifest.xml");
                _manifestFilePath = pathBuilder.ToString();
            }
            return _manifestFilePath;
        }
    }


    internal class AndroidXmlDocument : XmlDocument {
        private string m_Path;
        protected XmlNamespaceManager nsMgr;
        public readonly string AndroidXmlNamespace = "http://schemas.android.com/apk/res/android";
        public AndroidXmlDocument(string path) {
            m_Path = path;
            using (var reader = new XmlTextReader(m_Path)) {
                reader.Read();
                Load(reader);
            }
            nsMgr = new XmlNamespaceManager(NameTable);
            nsMgr.AddNamespace("android", AndroidXmlNamespace);
        }

        public string Save() {
            return SaveAs(m_Path);
        }

        public string SaveAs(string path) {
            using (var writer = new XmlTextWriter(path, new UTF8Encoding(false))) {
                writer.Formatting = Formatting.Indented;
                Save(writer);
            }
            return path;
        }
    }


    internal class AndroidManifest : AndroidXmlDocument {
        private readonly XmlElement ApplicationElement;

        public AndroidManifest(string path) : base(path) {
            ApplicationElement = SelectSingleNode("/manifest/application") as XmlElement;
        }

        private XmlAttribute CreateAndroidAttribute(string key, string value) {
            XmlAttribute attr = CreateAttribute("android", key, AndroidXmlNamespace);
            attr.Value = value;
            return attr;
        }

        internal XmlNode GetActivityWithLaunchIntent() {
            return SelectSingleNode("/manifest/application/activity[intent-filter/action/@android:name='android.intent.action.MAIN' and " +
                    "intent-filter/category/@android:name='android.intent.category.LAUNCHER']", nsMgr);
        }

        internal void SetApplicationTheme(string appTheme) {
            ApplicationElement.Attributes.Append(CreateAndroidAttribute("theme", appTheme));
        }

        internal void SetStartingActivityName(string activityName) {
            GetActivityWithLaunchIntent().Attributes.Append(CreateAndroidAttribute("name", activityName));
        }


        internal void SetHardwareAcceleration() {
            GetActivityWithLaunchIntent().Attributes.Append(CreateAndroidAttribute("hardwareAccelerated", "true"));
        }

        internal void SetMicrophonePermission() {
            var manifest = SelectSingleNode("/manifest");
            XmlElement child = CreateElement("uses-permission");
            manifest.AppendChild(child);
            XmlAttribute newAttribute = CreateAndroidAttribute("name", "android.permission.RECORD_AUDIO");
            child.Attributes.Append(newAttribute);
        }

        internal void SetApplicationDebuable(bool value) {
            ApplicationElement.Attributes.Append(CreateAndroidAttribute("debuggable", value.ToString().ToLower()));
        }
    }
}