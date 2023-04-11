using System;
using System.Collections;
using UnityEngine;

namespace Scoz.Func {
    public class VersionSetting {
        public static string AppLargeVersion {
            get {
                string originVersion = Application.version;
                string[] vNumbers = originVersion.Split('.');
                string newVersion = vNumbers[0];
                for (int i = 1; i < vNumbers.Length - 1; i++)
                    newVersion = newVersion + "." + vNumbers[i];
                return newVersion;
            }
        }

        public static int CompareVersion(string _v1, string _v2) {
            string[] v1Numbers = _v1.Split('.');
            string[] v2Numbers = _v2.Split('.');
            if (v1Numbers.Length != v2Numbers.Length)
                DebugLogger.LogError("Compare Version Error:  not the same version length.");

            try {
                for (int i = 0; i < v1Numbers.Length; i++) {

                    int v1value = int.Parse(v1Numbers[i]);
                    int v2value = int.Parse(v2Numbers[i]);

                    if (v1value < v2value)
                        return -1;
                    if (v1value > v2value)
                        return 1;
                }
            } catch (Exception _e) {
                DebugLogger.LogError("Compare Version Error: " + _e);
            }
            return 0;
        }

    }
}