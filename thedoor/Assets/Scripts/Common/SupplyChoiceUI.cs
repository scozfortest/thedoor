using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Scoz.Func;
using System;
using TMPro;
using System.Linq;

namespace TheDoor.Main {
    public class SupplyChoiceUI : ItemSpawner_Remote<DisplaySupplyPrefab> {


        [SerializeField] TextMeshProUGUI NameText;
        [SerializeField] PlayAudio MyPlayAudio;
        [SerializeField] TextMeshProUGUI BtnText;

        public static SupplyChoiceUI Instance;

        Action<List<SupplyData>> BackAction;
        ItemType MytemType;
        List<DisplaySupplyPrefab> CurSelecteds = new List<DisplaySupplyPrefab>();
        int SelectCount = 0;

        public override void Init() {
            base.Init();
            Instance = this;
        }


        public void ShowUI(string _title, int _selectCount, List<SupplyData> _datas, Action<List<SupplyData>> _ac = null) {
            if (_datas == null || _datas.Count == 0 || _selectCount == 0) {
                _ac?.Invoke(null);
                return;
            }
            SelectCount = _selectCount;
            CurSelecteds.Clear();
            BackAction = _ac;
            if (string.IsNullOrEmpty(_title))
                NameText.text = string.Format(StringData.GetUIString("GainItemTypeTitle"), MytemType);
            else
                NameText.text = _title;
            SpawnItems(_datas);

            SetActive(true);
            MyPlayAudio.PlayByName("reward");
            RefreshUI();
        }


        void SpawnItems(List<SupplyData> _datas) {
            if (!LoadItemFinished) {
                WriteLog.LogError("SupplyPrefab尚未載入完成");
                return;
            }
            InActiveAllItem();

            if (_datas == null || _datas.Count == 0) {
                return;
            }
            for (int i = 0; i < _datas.Count; i++) {
                //bool _replaced = false;//此物品是否因為已擁有而被取代成其他物品了
                //if (i >= replacedItemIndex)
                //    _replaced = true;
                if (i < ItemList.Count) {
                    ItemList[i].SetData(_datas[i]);
                    ItemList[i].IsActive = true;
                    ItemList[i].gameObject.SetActive(true);
                } else {
                    var item = Spawn();
                    item.SetData(_datas[i]);
                }
            }

        }
        public override void RefreshUI() {
            base.RefreshUI();
            BtnText.text = (CurSelecteds.Count != 0) ? StringData.GetUIString("Confirm") : StringData.GetUIString("Skip");
        }
        public void Select(DisplaySupplyPrefab _data) {
            if (!CurSelecteds.Contains(_data)) {
                CurSelecteds.Add(_data);
                if (CurSelecteds.Count > SelectCount) {
                    var first = CurSelecteds.First();
                    first.SetSelect(false);
                    UnSelect(first);
                }
            }
            RefreshUI();
        }
        public void UnSelect(DisplaySupplyPrefab _data) {
            if (CurSelecteds.Contains(_data))
                CurSelecteds.Remove(_data);
            RefreshUI();
        }
        public void Confirm() {
            SetActive(false);
            List<SupplyData> datas = CurSelecteds.Select(a => a.MySupplyData).ToList();
            BackAction?.Invoke(datas);//要先關閉在跑CB否則如果CB中有跑再次開啟GainItemListUI就會因為執行順序而又被關閉
        }
    }
}