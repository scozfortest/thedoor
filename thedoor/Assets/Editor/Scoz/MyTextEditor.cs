using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections;

namespace Scoz.Func
{
    [CustomEditor(typeof(MyText))]
    public class MyTextEditor : UnityEditor.UI.TextEditor
    {
        SerializedProperty UIString;

        protected override void OnEnable()
        {
            base.OnEnable();
            UIString = serializedObject.FindProperty("UIString");
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Rect textFieldRect = new Rect(EditorGUILayout.GetControlRect(true, 16));
            EditorGUI.PropertyField(textFieldRect, UIString);
            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
        [MenuItem("GameObject/Scoz/UI/MyText", false, 10)]
        static void CreateCustomGameObject(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("MyText");
            go.AddComponent<MyText>();
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeGameObject = go;
        }
    }
}