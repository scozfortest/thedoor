using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;
using UnityEditor.U2D;

public class SpriteAtlasPaddingOverride

{

    [MenuItem("Assets/Scoz/Atlas/Set Padding 20")]
    public static void SetPaddingTo20()
    {
        SpriteAtlasCustomPadding(20);
    }
    [MenuItem("Assets/Scoz/Atlas/Set Padding 30")]
    public static void SetPaddingTo30()
    {
        SpriteAtlasCustomPadding(30);
    }
    [MenuItem("Assets/Scoz/Atlas/Set Padding 40")]
    public static void SetPaddingTo40()
    {
        SpriteAtlasCustomPadding(40);
    }
    [MenuItem("Assets/Scoz/Atlas/Set Padding 50")]
    public static void SetPaddingTo50()
    {
        SpriteAtlasCustomPadding(50);
    }
    [MenuItem("Assets/Scoz/Atlas/Set Padding 60")]
    public static void SetPaddingTo60()
    {
        SpriteAtlasCustomPadding(60);
    }
    static void SpriteAtlasCustomPadding(int _padding)
    {
        Object[] objs = Selection.objects;
        foreach (var obj in objs)
        {
            SpriteAtlas sa = obj as SpriteAtlas;
            if (sa)
            {
                var ps = sa.GetPackingSettings();
                ps.padding = _padding;
                sa.SetPackingSettings(ps);
            }
        }
        AssetDatabase.SaveAssets();
    }
}