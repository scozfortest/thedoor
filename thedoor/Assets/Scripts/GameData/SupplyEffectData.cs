using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using LitJson;

namespace TheDoor.Main {
    public class SupplyEffectData : MyJsonData {
        public static string DataName { get; set; }
        public string Description { get { return StringData.GetString_static(DataName + "_" + ID, "Description"); } }
        public int SupplyID { get; private set; }
        public Target MyTarget { get; private set; }
        public List<TargetEffectData> MyEffects = new List<TargetEffectData>();

        static Dictionary<int, List<SupplyEffectData>> SupplyEffectDataDic = new Dictionary<int, List<SupplyEffectData>>();
        public static void ClearStaticDic() {
            SupplyEffectDataDic.Clear();
        }

        protected override void GetDataFromJson(JsonData _item, string _dataName) {
            DataName = _dataName;
            JsonData item = _item;
            EffectType tmpTEffectType = EffectType.Attack;
            int tmpTypeValue = 0;
            MyEffects.Clear();
            foreach (string key in item.Keys) {
                switch (key) {
                    case "ID":
                        ID = int.Parse(item[key].ToString());
                        break;
                    case "SupplyID":
                        SupplyID = int.Parse(item[key].ToString());
                        break;
                    case "Target":
                        MyTarget = MyEnum.ParseEnum<Target>(item[key].ToString());
                        break;
                    default:
                        try {
                            if (key.Contains("EffectType")) {
                                tmpTEffectType = MyEnum.ParseEnum<EffectType>(item[key].ToString());
                            } else if (key.Contains("EffectValue")) {
                                tmpTypeValue = int.Parse(item[key].ToString());
                            } else if (key.Contains("EffectProb")) {
                                TargetEffectData tmpTEffectData = new TargetEffectData(MyTarget, tmpTEffectType, float.Parse(item[key].ToString()), tmpTypeValue);
                                MyEffects.Add(tmpTEffectData);
                            }
                        } catch (Exception _e) {
                            WriteLog.LogErrorFormat(DataName + "表格格式錯誤 ID:" + ID + "    Log: " + _e);
                        }
                        //WriteLog.LogWarning(string.Format("{0}表有不明屬性:{1}", DataName, key));
                        break;
                }
            }
            AddToSupplyEffectDic(this);
        }

        void AddToSupplyEffectDic(SupplyEffectData _data) {
            if (SupplyEffectDataDic.ContainsKey(_data.SupplyID)) {
                SupplyEffectDataDic[_data.SupplyID].Add(_data);
            } else {
                SupplyEffectDataDic.Add(_data.SupplyID, new List<SupplyEffectData> { _data });
            }
        }


        public static List<SupplyEffectData> GetSupplyEffectDatas(int _id) {
            if (SupplyEffectDataDic.ContainsKey(_id)) return SupplyEffectDataDic[_id];
            return null;
        }
        public string GetEffectStr() {
            string s = "";
            foreach (var e in MyEffects) {

            }
            return s;
        }
    }
}