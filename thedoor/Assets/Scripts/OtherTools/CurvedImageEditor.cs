using UnityEngine;
using UnityEngine.UI;
using System.Collections;

#if UNITY_EDITOR

using UnityEditor;

[CustomEditor(typeof(CurvedImage))]
public class CurvedImageEditor : CurvedGraphicEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        CurvedImage script = (CurvedImage)this.target;

        EditorGUILayout.Space();

        EditorGUI.BeginChangeCheck();

        EditorGUI.BeginDisabledGroup(!(script.UIImage.type == Image.Type.Sliced || script.UIImage.type == Image.Type.Tiled));
        Vector2 newCornerRatio = EditorGUILayout.Vector2Field("Corner Ratio", script.cornerPosRatio);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(script, "Change Corner Ratio");
            EditorUtility.SetDirty(script);
            script.cornerPosRatio = newCornerRatio;
        }

        if (GUILayout.Button("Use native corner ratio"))
        {
            Undo.RecordObject(script, "Change Corner Ratio");
            EditorUtility.SetDirty(script);
            script.cornerPosRatio = script.OriCornerPosRatio;
        }

        EditorGUI.EndDisabledGroup();
    }

    protected override void OnSceneGUI()
    {
        base.OnSceneGUI();

        CurvedImage script = (CurvedImage)this.target;

        if (script.UIImage.type == Image.Type.Sliced || script.UIImage.type == Image.Type.Tiled)
        {
            Vector3 cornerPos = Vector3.zero;//

            if (script.IsCurved)
            {
                cornerPos = script.GetBCurveSandwichSpacePoint(script.cornerPosRatio.x, script.cornerPosRatio.y);
            }
            else
            {
                cornerPos.x = script.cornerPosRatio.x * script.RectTrans.rect.width - script.RectTrans.pivot.x * script.RectTrans.rect.width;
                cornerPos.y = script.cornerPosRatio.y * script.RectTrans.rect.height - script.RectTrans.pivot.y * script.RectTrans.rect.height;
            }

            Handles.color = Color.gray;
            EditorGUI.BeginChangeCheck();
            Vector3 newCornerPos = Handles.FreeMoveHandle(script.transform.TransformPoint(cornerPos), script.transform.rotation, HandleUtility.GetHandleSize(script.transform.TransformPoint(cornerPos)) / 7, Vector3.one, Handles.SphereHandleCap);
            Handles.Label(newCornerPos, string.Format("Corner Mover"));

            newCornerPos = script.transform.InverseTransformPoint(newCornerPos);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(script, "Move Corner");
                EditorUtility.SetDirty(script);

                script.cornerPosRatio = new Vector2(newCornerPos.x, newCornerPos.y);
                script.cornerPosRatio.x = (script.cornerPosRatio.x + script.RectTrans.pivot.x * script.RectTrans.rect.width) / script.RectTrans.rect.width;
                script.cornerPosRatio.y = (script.cornerPosRatio.y + script.RectTrans.pivot.y * script.RectTrans.rect.height) / script.RectTrans.rect.height;
            }
        }
    }
}
#endif
