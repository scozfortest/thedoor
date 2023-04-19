using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Scoz.Func;
using System;
namespace TheDoor.Main {
    public class GainItemListUI : ItemSpawner_Remote<ItemPrefab> {

        [SerializeField] PlayAudio MyPlayAudio;
        //[SerializeField] GameObject ReplaceInfoGO;
        Action BackAction;

        public void CallUI(List<ItemData> _datas, List<ItemData> _replacedItems, Action _ac = null) {
            BackAction = _ac;
            //if (_replacedItems != null && _replacedItems.Count > 0)
            //    ReplaceInfoGO.SetActive(true);
            //else
            //    ReplaceInfoGO.SetActive(false);
            SpawnItems(_datas, _replacedItems);
            SetActive(true);
            MyPlayAudio.PlayByName("reward");
        }

        void SpawnItems(List<ItemData> _datas, List<ItemData> _replacedItems) {
            if (!LoadItemFinished) {
                WriteLog.LogError("ItemPrefab尚未載入完成");
                return;
            }
            InActiveAllItem();

            if (_datas == null || _datas.Count == 0) {
                return;
            }
            //int replacedItemIndex = _datas.Count;
            if (_replacedItems != null && _replacedItems.Count > 0)
                _datas.AddRange(_replacedItems);
            for (int i = 0; i < _datas.Count; i++) {
                //bool _replaced = false;//此物品是否因為已擁有而被取代成其他物品了
                //if (i >= replacedItemIndex)
                //    _replaced = true;
                if (i < ItemList.Count) {
                    ItemList[i].Show(_datas[i]);
                    ItemList[i].IsActive = true;
                    ItemList[i].gameObject.SetActive(true);
                } else {
                    ItemPrefab item = Spawn();
                    item.Show(_datas[i]);
                }
            }

        }
        public void OnCloseClick() {
            BackAction?.Invoke();
            SetActive(false);
        }
    }
}