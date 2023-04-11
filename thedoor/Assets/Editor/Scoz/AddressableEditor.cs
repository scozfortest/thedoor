using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.AddressableAssets;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;

[InitializeOnLoad]
public class AddressableEditor {
    static AddressableEditor() {
        AddressableAssetSettingsDefaultObject.Settings.OnModification += OnSettingsModificationCustom;
    }

    static void OnSettingsModificationCustom(AddressableAssetSettings s, AddressableAssetSettings.ModificationEvent e, object o) {
        if (o == null) return;
        if (e == AddressableAssetSettings.ModificationEvent.EntryCreated || e == AddressableAssetSettings.ModificationEvent.EntryAdded || e == AddressableAssetSettings.ModificationEvent.EntryRemoved) {
            var entryList = o as List<AddressableAssetEntry>;
            if (entryList != null) {
                for (int i = 0; i < entryList.Count; i++) {
                    var entry = entryList[i];
                    if (entry != null) {
                        switch (e) {
                            case AddressableAssetSettings.ModificationEvent.EntryCreated:
                                RoleImageAutoAddLabel(entry);
                                break;
                            case AddressableAssetSettings.ModificationEvent.EntryAdded:
                                RoleImageAutoAddLabel(entry);
                                break;
                            case AddressableAssetSettings.ModificationEvent.EntryRemoved:
                                RoleImageAutoRemoveLabel(entry);
                                break;
                        }
                    }
                }
            }
        }
    }
    static void RoleImageAutoAddLabel(AddressableAssetEntry _entry) {
        string path = _entry.ToString();
        if (!path.Contains("Assets/AddressableAssets/Image/Role/")) return;
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        string lableName = "Role" + path.Replace("Assets/AddressableAssets/Image/Role/", "");
        settings.AddLabel(lableName);
        _entry.SetLabel(lableName, true);
    }
    static void RoleImageAutoRemoveLabel(AddressableAssetEntry _entry) {
        string path = _entry.ToString();
        if (!path.Contains("Assets/AddressableAssets/Image/Role/")) return;
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        string lableName = "Role" + path.Replace("Assets/AddressableAssets/Image/Role/", "");
        settings.RemoveLabel(lableName);
    }
}