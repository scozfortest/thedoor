using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Scoz.Func {
    public class CompressEditor : MonoBehaviour {
        [MenuItem("Assets/Scoz/SpriteFormat/壓縮圖檔MaxSize 4096(Android是 Crunched DXT5 IOS是ASTC4x4)")]
        static void CompressTextureWithMaxSize4096() {
            CompressTextureWithMaxSize(4096);
        }
        [MenuItem("Assets/Scoz/SpriteFormat/壓縮圖檔MaxSize 2048(Android是 Crunched DXT5 IOS是ASTC4x4)")]
        static void CompressTextureWithMaxSize2048() {
            CompressTextureWithMaxSize(2048);
        }
        [MenuItem("Assets/Scoz/SpriteFormat/壓縮圖檔MaxSize 1024(Android是 Crunched DXT5 IOS是ASTC4x4)")]
        static void CompressTextureWithMaxSize1024() {
            CompressTextureWithMaxSize(1024);
        }
        [MenuItem("Assets/Scoz/SpriteFormat/壓縮圖檔MaxSize 512(Android是 Crunched DXT5 IOS是ASTC4x4)")]
        static void CompressTextureWithMaxSize512() {
            CompressTextureWithMaxSize(512);
        }
        [MenuItem("Assets/Scoz/SpriteFormat/壓縮圖檔MaxSize 256(Android是 Crunched DXT5 IOS是ASTC4x4)")]
        static void CompressTextureWithMaxSize256() {
            CompressTextureWithMaxSize(256);
        }
        static void CompressTextureWithMaxSize(int _maxTextureSize) {
            Object[] selectTextureS = Selection.GetFiltered(typeof(Texture), SelectionMode.DeepAssets);
            Object selectObj = Selection.activeObject;
            Object[] selectObjS = Selection.objects;

            Selection.activeObject = null;
            Selection.objects = new Object[0];
            foreach (var obj in selectTextureS) {
                if (obj == null)
                    continue;
                var path = AssetDatabase.GetAssetPath(obj.GetInstanceID());
                if (EditorUtility.DisplayCancelableProgressBar("Compressing (RGBA Crunched DXT5)... ", path, 0f)) {
                    Selection.activeObject = selectObj;
                    Selection.objects = selectObjS;
                    EditorUtility.ClearProgressBar();
                    return;
                }

                bool bChange = false;
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;

                bChange |= SetTextureRGBACrunchedDXT5(importer, _maxTextureSize);

                if (bChange) {
                    importer.SaveAndReimport();
                }
            }

            Selection.activeObject = selectObj;
            Selection.objects = selectObjS;
            EditorUtility.ClearProgressBar();
        }

        static bool SetTextureRGBACrunchedDXT5(TextureImporter _importer, int _maxTextureSize) {
            if (_importer == null)
                return false;
            var setting = _importer.GetPlatformTextureSettings("Standalone");
            bool result = false;
            if (setting.format != TextureImporterFormat.DXT5Crunched ||
                setting.compressionQuality != 100 || setting.maxTextureSize != _maxTextureSize) {
                setting.crunchedCompression = true;
                setting.format = TextureImporterFormat.DXT5Crunched;
                setting.compressionQuality = 100;
                setting.maxTextureSize = _maxTextureSize;
                _importer.filterMode = FilterMode.Bilinear;
                _importer.npotScale = TextureImporterNPOTScale.None;
                setting.overridden = true;
                _importer.SetPlatformTextureSettings(setting);
                result |= true;
            }
            setting = _importer.GetPlatformTextureSettings("Android");
            if (setting.format != TextureImporterFormat.DXT5Crunched ||
setting.compressionQuality != 100 || setting.maxTextureSize != _maxTextureSize) {
                setting.crunchedCompression = true;
                setting.format = TextureImporterFormat.DXT5Crunched;
                setting.compressionQuality = 100;
                setting.maxTextureSize = _maxTextureSize;
                _importer.filterMode = FilterMode.Bilinear;
                _importer.npotScale = TextureImporterNPOTScale.None;
                setting.overridden = true;
                _importer.SetPlatformTextureSettings(setting);
                result |= true;
            }
            setting = _importer.GetPlatformTextureSettings("iOS");
            if (setting.format != TextureImporterFormat.ASTC_4x4 ||
setting.compressionQuality != 100 || setting.maxTextureSize != _maxTextureSize) {
                setting.crunchedCompression = true;
                setting.format = TextureImporterFormat.ASTC_4x4;
                setting.compressionQuality = 100;
                setting.maxTextureSize = _maxTextureSize;
                _importer.filterMode = FilterMode.Bilinear;
                _importer.npotScale = TextureImporterNPOTScale.None;
                setting.overridden = true;
                _importer.SetPlatformTextureSettings(setting);
                result |= true;
            }
            return result;
        }

        [MenuItem("Assets/Scoz/SpriteFormat/圖片格式設定為Sprite")]
        static void SetTextureTypeToSprite() {
            Object[] selectTextureS = Selection.GetFiltered(typeof(Texture), SelectionMode.DeepAssets);
            Object selectObj = Selection.activeObject;
            Object[] selectObjS = Selection.objects;

            Selection.activeObject = null;
            Selection.objects = new Object[0];
            foreach (var obj in selectTextureS) {
                if (obj == null)
                    continue;
                var path = AssetDatabase.GetAssetPath(obj.GetInstanceID());
                if (EditorUtility.DisplayCancelableProgressBar("Set Texture to Sprite... ", path, 0f)) {
                    Selection.activeObject = selectObj;
                    Selection.objects = selectObjS;
                    EditorUtility.ClearProgressBar();
                    return;
                }

                bool bChange = false;
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;

                bChange |= SetTextureToSprite(importer);

                if (bChange) {
                    importer.SaveAndReimport();
                }
            }
            Selection.activeObject = selectObj;
            Selection.objects = selectObjS;
            EditorUtility.ClearProgressBar();
        }
        static bool SetTextureToSprite(TextureImporter _importer) {
            if (_importer == null)
                return false;
            if (_importer.textureType != TextureImporterType.Sprite) {
                _importer.textureType = TextureImporterType.Sprite;
                return true;
            } else
                return false;
            /*
            var setting = _importer.GetPlatformTextureSettings("Standalone");
            if (setting.format != TextureImporterFormat.DXT5Crunched ||
                setting.compressionQuality != 100)
            {
                setting.crunchedCompression = true;
                setting.format = TextureImporterFormat.DXT5Crunched;
                setting.compressionQuality = 100;
                setting.maxTextureSize = MaxTextureSize;
                _importer.filterMode = FilterMode.Bilinear;
                _importer.npotScale = TextureImporterNPOTScale.None;
                setting.overridden = true;
                _importer.SetPlatformTextureSettings(setting);
                return true;
            }
            return false;
            */
        }
    }
}