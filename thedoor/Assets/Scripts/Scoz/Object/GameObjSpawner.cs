using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System;
namespace Scoz.Func {
    public class GameObjSpawner : MonoBehaviour {
        /// <summary>
        /// 藉由路徑來產生特效物件(實際上還是取得物件，只是路徑是在AddressableAssets/Particle底下
        /// </summary>
        public static void SpawnParticleObjByPath(string _path, Vector3 _pos, Vector3 _dir, Transform _parent, Action<GameObject> _cb = null) {
            AddressablesLoader.GetParticle(_path, (obj, handle) => {
                GameObject go = SpawnGameObj(obj, _pos, _dir, _parent);
                _cb?.Invoke(go);
                Addressables.Release(handle);
            });
        }
        /// <summary>
        /// 藉由路徑來產生特效物件(實際上還是取得物件，只是路徑是在AddressableAssets/Particle底下
        /// </summary>
        public static void SpawnParticleObjByPath(string _path, Transform _parent, Action<GameObject> _cb = null) {
            AddressablesLoader.GetParticle(_path, (obj, handle) => {
                GameObject go = SpawnGameObj(obj, obj.transform.localPosition, obj.transform.localRotation.eulerAngles, _parent);
                _cb?.Invoke(go);
                Addressables.Release(handle);
            });
        }
        /// <summary>
        /// 藉由路徑來產生物件，路徑是在AddressableAssets/Prefab底下
        /// </summary>
        public static void SpawnGameObjByPath(string _path, Vector3 _pos, Vector3 _dir, Transform _parent, Action<GameObject> _cb = null) {
            AddressablesLoader.GetPrefab(_path, (obj, handle) => {
                GameObject go = SpawnGameObj(obj, _pos, _dir, _parent);
                _cb?.Invoke(go);
                Addressables.Release(handle);
            });
        }
        /// <summary>
        /// 藉由AssetReference來產生物件
        /// </summary>
        public static void SpawnGameObjByAssetRef(AssetReference _assetRef, Vector3 _pos, Vector3 _dir, Transform _parent, Action<GameObject> _cb = null) {
            AddressablesLoader.GetPrefabByRef(_assetRef, (obj, handle) => {
                GameObject go = SpawnGameObj(obj, _pos, _dir, _parent);
                _cb?.Invoke(go);
                Addressables.Release(handle);
            });
        }
        /// <summary>
        /// 產生物件
        /// </summary>
        public static GameObject SpawnGameObj(GameObject _prefab, Vector3 _pos, Vector3 _dir, Transform _parent) {
            if (_prefab == null) {
                return null;
            }
            GameObject go = Instantiate(_prefab, Vector3.zero, Quaternion.identity);
            go.transform.SetParent(_parent);
            go.transform.localScale = _prefab.transform.localScale;
            go.transform.localPosition = _pos;
            go.transform.localRotation = Quaternion.Euler(_dir);
            return go;
        }
    }
}
