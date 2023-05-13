using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using System;
using System.Linq;
using TMPro;

namespace TheDoor.Main {

    public class CreateRoleUI : BaseUI {

        [SerializeField] Image Img;
        [SerializeField] TextMeshProUGUI Content;

        OwnedRoleData MyOwnedRoleData;
        RoleData MyRoleData;
        RolePlotData CurRolePlotData;
        int CurPlotIndex;
        List<ItemData> DefaultItems;
        List<ItemData> ExclusiveItems;
        List<ItemData> InheritItems;

        public static CreateRoleUI Instance { get; private set; }

        public override void Init() {
            base.Init();
            Instance = this;
        }

        public void ShowUI(OwnedRoleData _ownedData) {
            MyOwnedRoleData = _ownedData;
            MyRoleData = RoleData.GetData(MyOwnedRoleData.ID);
            CurPlotIndex = 0;
            CurRolePlotData = RolePlotData.GetData(MyRoleData.ID, CurPlotIndex);
            if (CurRolePlotData == null) OnRolePlotEnd();
            RefreshUI();
            SetActive(true);
        }
        public override void RefreshUI() {
            base.RefreshUI();
            ShowContent();
            AddressablesLoader.GetSprite(CurRolePlotData.Ref, (sprite, handle) => {
                Img.sprite = sprite;
            });
        }


        void ShowContent() {
            if (CurRolePlotData == null) {
                Content.text = "";
                return;
            }
            Content.text = CurRolePlotData.Description;
        }
        public void Next() {
            CurPlotIndex++;
            CurRolePlotData = RolePlotData.GetData(MyRoleData.ID, CurPlotIndex);
            if (CurRolePlotData == null) {
                OnRolePlotEnd();
                return;
            }
            RefreshUI();
        }



        /// <summary>
        /// 設定創腳介面跑完要跳出獲得的道具資料
        /// </summary>
        public void SetGainItemList(List<ItemData> _exclusiveItems, List<ItemData> _defaultItems, List<ItemData> _inheritItems) {
            ExclusiveItems = _exclusiveItems;
            DefaultItems = _defaultItems;
            InheritItems = _inheritItems;
        }
        /// <summary>
        /// 腳色劇本跑完跑這裡
        /// </summary>
        public void OnRolePlotEnd() {
            List<List<ItemData>> tmpList = new List<List<ItemData>>();
            tmpList.Add(ExclusiveItems);
            tmpList.Add(DefaultItems);
            tmpList.Add(InheritItems);
            string[] titles = new string[3] { "GainExclusiveSupply", "GainDefaultSupply", "GainInheritSupply" };
            int index = 0;
            PopupUI.ShowRoleInfoUI(GamePlayer.Instance.Data.CurRole, AdventureManager.PRole, true);
            ShowGainItem(index, tmpList, titles);
        }
        void ShowGainItem(int _index, List<List<ItemData>> tmpList, string[] _titles) {
            if (_index >= tmpList.Count) {
                End();
                return;
            }
            string title = StringData.GetUIString(_titles[_index]);
            PopupUI.ShowGainItemListUI(title, tmpList[_index], null, () => {

                _index++;
                ShowGainItem(_index, tmpList, _titles);
            });
        }

        /// <summary>
        /// 劇本跑完且獲得道具也跑完最終跑這裡
        /// </summary>
        void End() {

        }
    }
}



//不使用Server回傳資料了
///// <summary>
///// 設定創腳介面跑完要跳出獲得的道具資料
///// </summary>
//public void SetCreateRoleCBDic(Dictionary<string, object> _cbDic) {
//    DefaultItems = new List<ItemData>();
//    ExclusiveItems = new List<ItemData>();
//    InheritItems = new List<ItemData>();
//    try {
//        var returnItemDic = DataHandler.ConvertDataObjToReturnItemDic(_cbDic);
//        List<ItemData> returnGainItems = returnItemDic["ReturnGainItems"];
//        List<int> exclusiveSupplyIDs = _cbDic["ExclusiveSupplyIDs"].ObjListToIntList();
//        List<int> defaultSupplyIDs = _cbDic["DefaultSupplyIDs"].ObjListToIntList();
//        for (int i = 0; i < returnGainItems.Count; i++) {
//            if (returnGainItems[i].Type != ItemType.Supply) continue;
//            if (defaultSupplyIDs.Contains((int)returnGainItems[i].Value)) {
//                DefaultItems.Add(returnGainItems[i]);
//                defaultSupplyIDs.Remove((int)returnGainItems[i].Value);
//                continue;
//            } else if (exclusiveSupplyIDs.Contains((int)returnGainItems[i].Value)) {
//                ExclusiveItems.Add(returnGainItems[i]);
//                exclusiveSupplyIDs.Remove((int)returnGainItems[i].Value);
//                continue;
//            } else {
//                InheritItems.Add(returnGainItems[i]);
//                continue;
//            }
//        }
//    } catch (Exception _e) {
//        WriteLog.LogError("SetCreateRoleCBDic錯誤: " + _e);
//    }

//}