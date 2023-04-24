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

        public void LoadDataFromLoco(LocoDataName _name) {
            string json = LocoDataManager.GetDataFromLoco(_name);
            if (string.IsNullOrEmpty(json)) return;
            Dictionary<string, object> dic;
            List<Dictionary<string, object>> list;
            switch (_name) {
                case LocoDataName.Player:
                    dic = GetDicFromJson(json);
                    SetMainPlayerData(dic);
                    break;
                case LocoDataName.Role:
                    list = GetListDicFromJson(json);
                    SetOwnedDatas<OwnedRoleData>(ColEnum.Role, list);
                    break;
                case LocoDataName.Supply:
                    list = GetListDicFromJson(json);
                    SetOwnedDatas<OwnedSupplyData>(ColEnum.Supply, list);
                    break;
                case LocoDataName.Adventure:
                    list = GetListDicFromJson(json);
                    SetOwnedDatas<OwnedAdventureData>(ColEnum.Adventure, list);
                    break;
                default:
                    WriteLog.LogErrorFormat("尚未實作LocoData為{0}類型的json轉Dic方法", _name);
                    break;
            }
        }
        Dictionary<string, object> GetDicFromJson(string _json) {
            JSONNode jsNode = JSONObject.Parse(_json);
            Dictionary<string, object> data = new Dictionary<string, object>();
            foreach (var key in jsNode.Keys)
                data[key] = jsNode[key].Value;
            return data;
        }
        List<Dictionary<string, object>> GetListDicFromJson(string _json) {
            JSONNode jsNode = JSONArray.Parse(_json);
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            foreach (var value in jsNode.Values) {
                Dictionary<string, object> data = GetDicFromJson(value.ToString());
                list.Add(data);
            }
            return list;
        }


        public void SaveToLoco_PlayerData() {
            LocoDataManager.SaveDataToLoco(LocoDataName.Player, Data.ToScozJson());
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
