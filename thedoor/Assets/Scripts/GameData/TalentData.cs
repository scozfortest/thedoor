using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using LitJson;

namespace TheDoor.Main {
    public class TalentData : MyJsonData {
        public static string DataName { get; set; }
        public new string ID { get; private set; }
        public string Name {
            get {
                return StringData.GetString_static(DataName + "_" + ID, "Name");
            }
        }
        public string Description {
            get {
                return StringData.GetString_static(DataName + "_" + ID, "Description");
            }
        }
        public string Ref { get; private set; }
        public TalentType MyTalentType { get; private set; }
        public List<string> Values { get; private set; } = new List<string>();


        protected override void GetDataFromJson(JsonData _item, string _dataName) {
            DataName = _dataName;
            JsonData item = _item;
            Values.Clear();
            foreach (string key in item.Keys) {
                switch (key) {
                    case "ID":
                        ID = item[key].ToString();
                        MyTalentType = MyEnum.ParseEnum<TalentType>(ID);
                        break;
                    case "Ref":
                        Ref = item[key].ToString();
                        break;
                    default:
                        try {
                            if (key.Contains("Value")) {
                                Values.Add(item[key].ToString());
                            }
                        } catch (Exception _e) {
                            WriteLog.LogErrorFormat(DataName + "表格格式錯誤 ID:" + ID + "    Log: " + _e);
                        }
                        //DebugLogger.LogWarning(string.Format("{0}表有不明屬性:{1}", DataName, key));
                        break;
                }
            }
        }

        public static TalentData GetData(string _id) {
            return GameDictionary.GetJsonData<TalentData>("Talent", _id);
        }

        public int GetAttackExtraValue(int _dmg) {
            int value = 0;
            switch (MyTalentType) {
                case TalentType.CombatSkill:
                    value += Mathf.RoundToInt((float)_dmg * float.Parse(Values[0]));
                    break;
            }
            return value;
        }

        public int GetSearchExtraSupply() {
            int value = 0;
            switch (MyTalentType) {
                case TalentType.Investigation:
                    value += int.Parse(Values[0]);
                    break;
            }
            return value;
        }
        public int GetRestExtraSanPRestore(PlayerRole _role) {
            int value = 0;
            switch (MyTalentType) {
                case TalentType.Faith:
                    value += int.Parse(Values[0]);
                    break;
            }
            return value;
        }
        public int GetRestExtraHPRestore(PlayerRole _role) {
            int value = 0;
            switch (MyTalentType) {
                case TalentType.EmergencyWoundCare:
                    int loseHP = _role.MaxHP - _role.CurHP;
                    value += Mathf.RoundToInt((float)loseHP * float.Parse(Values[0]));
                    break;
            }
            return value;
        }


    }
}