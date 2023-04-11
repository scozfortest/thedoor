using UnityEngine;
using UnityEngine.UI;
using System.Collections;

#if UNITY_EDITOR

using UnityEditor;

[CustomEditor(typeof(CurvedGraphic), true)]
public class CurvedGraphicEditor : Editor
{

    protected static bool isCurveGpFold = false;

    protected Vector3[] reuse_Vector3s = new Vector3[4];

    public override void OnInspectorGUI()
    {
        CurvedGraphic script = (CurvedGraphic)this.target;

        //base.OnInspectorGUI();
        DrawDefaultInspector();

        // draw the editor that shows the position ratio of all control points from the two bezier curves
        isCurveGpFold = EditorGUILayout.Foldout(isCurveGpFold, "Curves Position Ratios");
        if (isCurveGpFold)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Top Curve");
            EditorGUI.indentLevel++;
            Vector3[] controlPoints = script.RefCurvesControlRatioPoints[1].array;

            EditorGUI.BeginChangeCheck();
            for (int p = 0; p < controlPoints.Length; p++)
            {
                reuse_Vector3s[p] = EditorGUILayout.Vector3Field(string.Format("Control Points {0}", p + 1), controlPoints[p]);
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(script, "Change Ratio Points");
                EditorUtility.SetDirty(script);

                System.Array.Copy(reuse_Vector3s, script.RefCurvesControlRatioPoints[1].array, controlPoints.Length);
                script.UpdateCurveControlPointPositions();
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.LabelField("Bottom Curve");
            EditorGUI.indentLevel++;
            controlPoints = script.RefCurvesControlRatioPoints[0].array;

            EditorGUI.BeginChangeCheck();
            for (int p = 0; p < controlPoints.Length; p++)
            {
                reuse_Vector3s[p] = EditorGUILayout.Vector3Field(string.Format("Control Points {0}", p + 1), controlPoints[p]);
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(script, "Change Ratio Points");
                EditorUtility.SetDirty(script);

                System.Array.Copy(reuse_Vector3s, controlPoints, controlPoints.Length);
                script.UpdateCurveControlPointPositions();
            }
            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Fit Bezier curves to rect transform"))
        {
            Undo.RecordObject(script, "Fit to Rect Transform");
            Undo.RecordObject(script.RefCurves[0], "Fit to Rect Transform");
            Undo.RecordObject(script.RefCurves[1], "Fit to Rect Transform");
            EditorUtility.SetDirty(script);

            script.FixTextToRectTrans();

            script.Refresh();
        }

        EditorGUILayout.Space();
        EditorGUI.BeginDisabledGroup(script.RefCUIGraphic == null);

        if (GUILayout.Button("Reference CUI component for curves"))
        {
            Undo.RecordObject(script, "Reference Reference CUI");
            Undo.RecordObject(script.RefCurves[0], "Reference Reference CUI");
            Undo.RecordObject(script.RefCurves[1], "Reference Reference CUI");
            EditorUtility.SetDirty(script);

            script.ReferenceCUIForBCurves();

            script.Refresh();
        }

        EditorGUI.EndDisabledGroup();
    }

    protected virtual void OnSceneGUI()
    {
        CurvedGraphic script = (CurvedGraphic)this.target;

        script.ReportSet();

        for (int c = 0; c < script.RefCurves.Length; c++)
        {
            BezierCurve curve = script.RefCurves[c];

            if (curve.ControlPoints != null)
            {
                Vector3[] controlPoints = curve.ControlPoints;

                Transform handleTransform = curve.transform;
                Quaternion handleRotation = curve.transform.rotation;

                for (int p = 0; p < BezierCurve.CubicBezierCurvePtNum; p++)
                {
                    EditorGUI.BeginChangeCheck();
                    Handles.Label(handleTransform.TransformPoint(controlPoints[p]), string.Format("Control Point {0}", p + 1));
                    Vector3 newPt = Handles.DoPositionHandle(handleTransform.TransformPoint(controlPoints[p]), handleRotation);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(curve, "Move Point");
                        Undo.RecordObject(script, "Move Point");
                        EditorUtility.SetDirty(curve);
                        controlPoints[p] = handleTransform.InverseTransformPoint(newPt);

                    }
                }

                Handles.color = Color.gray;
                Handles.DrawLine(handleTransform.TransformPoint(controlPoints[0]), handleTransform.TransformPoint(controlPoints[1]));
                Handles.DrawLine(handleTransform.TransformPoint(controlPoints[1]), handleTransform.TransformPoint(controlPoints[2]));
                Handles.DrawLine(handleTransform.TransformPoint(controlPoints[2]), handleTransform.TransformPoint(controlPoints[3]));

                int sampleSize = 10;

                Handles.color = Color.white;
                for (int s = 0; s < sampleSize; s++)
                {
                    Handles.DrawLine(handleTransform.TransformPoint(curve.GetPoint((float)s / sampleSize)), handleTransform.TransformPoint(curve.GetPoint((float)(s + 1) / sampleSize)));
                }

                curve.EDITOR_ControlPoints = controlPoints;
            }
        }

        if (script.RefCurves != null)
        {
            Handles.DrawLine(script.RefCurves[0].transform.TransformPoint(script.RefCurves[0].ControlPoints[0]), script.RefCurves[1].transform.TransformPoint(script.RefCurves[1].ControlPoints[0]));
            Handles.DrawLine(script.RefCurves[0].transform.TransformPoint(script.RefCurves[0].ControlPoints[3]), script.RefCurves[1].transform.TransformPoint(script.RefCurves[1].ControlPoints[3]));
        }

        script.Refresh();
    }
}
#endif
