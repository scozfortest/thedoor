using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System;

namespace Scoz.Func {
    public class ItemSpawner_Remote<T> : BaseUI where T : MonoBehaviour, IItem {
        public AssetReference ItemAsset;
        public Transform ParentTrans = null;
        T ItemPrefab;
        [HideInInspector]
        public List<T> ItemList = new List<T>();
        public bool LoadItemFinished { get; private set; } = false;
        public bool AssetLoaded { get; private set; } = false;
        public void LoadItemAsset(Action _cb = null) {
            if (AssetLoaded) {
                _cb?.Invoke();
                return;
            }

            AssetLoaded = true;
            Addressables.LoadAssetAsync<GameObject>(ItemAsset).Completed += handle => {
                ItemPrefab = handle.Result.GetComponent<T>();
                if (ItemPrefab == null)
                    WriteLog.LogErrorFormat("取不到Component: {0} ", typeof(T).Name);
                LoadItemFinished = true;
                _cb?.Invoke();
            };
        }
        public T Spawn() {
            if (ItemPrefab == null) {
                WriteLog.LogError("ItemPrefab載入失敗");
                return default(T);
            }
            T item = Instantiate(ItemPrefab);
            item.transform.SetParent(ParentTrans);
            item.transform.localPosition = Vector3.zero;
            item.transform.localScale = ItemPrefab.transform.localScale;
            item.IsActive = true;
            ItemList.Add(item);
            return item;
        }
        public virtual void InActiveAllItem() {
            for (int i = 0; i < ItemList.Count; i++) {
                ItemList[i].gameObject.SetActive(false);
                ItemList[i].IsActive = false;
            }
        }
        public void ClearItems() {
            for (int i = 0; i < ItemList.Count; i++)
                Destroy(ItemList[i].gameObject);
            ItemList.Clear();
        }
        public void RemoveNullItems() {
            for (int i = ItemList.Count - 1; i >= 0; i--) {
                if (ItemList[i] == null) {
                    Destroy(ItemList[i].gameObject);
                    ItemList.RemoveAt(i);
                }
            }
        }
    }
}
