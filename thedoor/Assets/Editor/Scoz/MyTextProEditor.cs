using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections;
using TMPro;

namespace Scoz.Func {
    [CustomEditor(typeof(MyTextPro))]
    public class MyTextProEditor : TMPro.EditorUtilities.TMP_EditorPanelUI {
        SerializedProperty UIString;
        protected override void OnEnable() {
            base.OnEnable();
            UIString = serializedObject.FindProperty("UIString");
        }
        public override void OnInspectorGUI() {
            serializedObject.Update();
            Rect textFieldRect = new Rect(EditorGUILayout.GetControlRect(true, 16));
            EditorGUI.PropertyField(textFieldRect, UIString);
            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
        [MenuItem("GameObject/Scoz/UI/MyTextPro", false, 10)]
        static void CreateCustomGameObject(MenuCommand menuCommand) {
            GameObject go = new GameObject("MyTextPro");
            go.AddComponent<MyTextPro>();
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeGameObject = go;
        }
    }
}