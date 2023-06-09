using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using System;
using System.Linq;
using LitJson;
using SimpleJSON;

namespace TheDoor.Main {

    public partial class GamePlayer : MyPlayer {

        public void LoadAllDataFromLoco() {
            List<LocoDataName> locoDataNames = MyEnum.GetList<LocoDataName>();
            locoDataNames.Remove(LocoDataName.PlayerSetting);
            foreach (var name in locoDataNames) {
                try {
                    LoadDataFromLoco(name);
                } catch (Exception _e) {
                    WriteLog.LogError("LoadDataFromLoco Error:" + _e);
                }
            }
        }
        bool start = false;
        public void LoadDataFromLoco(LocoDataName _name) {
            string json = LocoDataManager.GetDataFromLoco(_name);
            if (string.IsNullOrEmpty(json)) return;
            Dictionary<string, object> dic;
            List<Dictionary<string, object>> list;
            List<object> objectList;
            switch (_name) {
                case LocoDataName.Player:
                    dic = (Dictionary<string, object>)ScozJsonConverter.ParseJson(json);
                    SetMainPlayerData(dic);
                    break;
                case LocoDataName.History:
                    dic = (Dictionary<string, object>)ScozJsonConverter.ParseJson(json);
                    SetOwnedData<OwnedHistoryData>(ColEnum.History, dic);
                    break;
                case LocoDataName.Role:
                    objectList = (List<object>)ScozJsonConverter.ParseJson(json);
                    list = objectList.Cast<Dictionary<string, object>>().ToList();
                    SetOwnedDatas<OwnedRoleData>(ColEnum.Role, list);
                    break;
                case LocoDataName.Supply:
                    objectList = (List<object>)ScozJsonConverter.ParseJson(json);
                    list = objectList.Cast<Dictionary<string, object>>().ToList();
                    SetOwnedDatas<OwnedSupplyData>(ColEnum.Supply, list);
                    break;
                case LocoDataName.Adventure:
                    start = true;
                    objectList = (List<object>)ScozJsonConverter.ParseJson(json);
                    list = objectList.Cast<Dictionary<string, object>>().ToList();
                    SetOwnedDatas<OwnedAdventureData>(ColEnum.Adventure, list);
                    break;
                default:
                    WriteLog.LogErrorFormat("尚未實作LocoData為{0}類型的json轉Dic方法", _name);
                    break;
            }
        }



        public void SaveToLoco_PlayerData() {
            LocoDataManager.SaveDataToLoco(LocoDataName.Player, Data.ToScozJson());
        }
        public void SaveToLoco_HistoryData() {
            OwnedHistoryData ownedHistoryData = GetOwnedData<OwnedHistoryData>(ColEnum.History, Data.UID);
            JSONObject jsObj = ownedHistoryData.ToScozJson();
            LocoDataManager.SaveDataToLoco(LocoDataName.History, jsObj);
        }
        public void SaveToLoco_RoleData() {
            List<OwnedRoleData> ownedRoleDatas = GetOwnedDatas<OwnedRoleData>(ColEnum.Role);
            JSONArray jsArray = new JSONArray();
            foreach (var ownedData in ownedRoleDatas) {
                jsArray.Add(ownedData.ToScozJson());
            }
            LocoDataManager.SaveDataToLoco(LocoDataName.Role, jsArray);
        }


        public void SaveToLoco_SupplyData() {
            List<OwnedSupplyData> ownedSupplyDatas = GetOwnedDatas<OwnedSupplyData>(ColEnum.Supply);
            JSONArray jsArray = new JSONArray();
            foreach (var ownedData in ownedSupplyDatas) {
                jsArray.Add(ownedData.ToScozJson());
            }
            LocoDataManager.SaveDataToLoco(LocoDataName.Supply, jsArray);
        }
        public void SaveToLoco_AdventureData() {
            List<OwnedAdventureData> ownedSupplyDatas = GetOwnedDatas<OwnedAdventureData>(ColEnum.Adventure);
            JSONArray jsArray = new JSONArray();
            foreach (var ownedData in ownedSupplyDatas) {
                jsArray.Add(ownedData.ToScozJson());
            }
            LocoDataManager.SaveDataToLoco(LocoDataName.Adventure, jsArray);
        }

    }
}
