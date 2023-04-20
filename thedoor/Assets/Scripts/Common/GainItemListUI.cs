using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Scoz.Func;
using System;
using TMPro;

namespace TheDoor.Main {
    public class GainItemListUI : ItemSpawner_Remote<ItemPrefab> {


        [SerializeField] TextMeshProUGUI NameText;
        [SerializeField] PlayAudio MyPlayAudio;
        //[SerializeField] GameObject ReplaceInfoGO;
        Action BackAction;
        ItemType MytemType;


        public void ShowUI(string _title, List<ItemData> _datas, List<ItemData> _replacedItems, Action _ac = null) {
            if ((_datas == null || _datas.Count == 0) && (_replacedItems == null || _replacedItems.Count == 0)) {
                _ac?.Invoke();
                return;
            }


            BackAction = _ac;
            //if (_replacedItems != null && _replacedItems.Count > 0)
            //    ReplaceInfoGO.SetActive(true);
            //else
            //    ReplaceInfoGO.SetActive(false);
            MytemType = _datas[0].Type;
            if (string.IsNullOrEmpty(_title))
                NameText.text = string.Format(StringData.GetUIString("GainItemTypeTitle"), MytemType);
            else
                NameText.text = _title;
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
            SetActive(false);
            BackAction?.Invoke();//要先關閉在跑CB否則如果CB中有跑再次開啟GainItemListUI就會因為執行順序而又被關閉
        }
    }
}