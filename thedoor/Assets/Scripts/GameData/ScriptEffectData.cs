using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using LitJson;

namespace TheDoor.Main {
    public class ScriptEffectData : MyJsonData {
        public static string DataName { get; set; }
        public List<TargetEffectData> MyEffects = new List<TargetEffectData>();

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
                    default:
                        try {
                            if (key.Contains("EffectType")) {
                                tmpTEffectType = MyEnum.ParseEnum<EffectType>(item[key].ToString());
                            } else if (key.Contains("EffectValue")) {
                                tmpTypeValue = int.Parse(item[key].ToString());
                            } else if (key.Contains("EffectProb")) {
                                TargetEffectData tmpTEffectData = new TargetEffectData(Target.Myself, tmpTEffectType, float.Parse(item[key].ToString()), tmpTypeValue);
                                MyEffects.Add(tmpTEffectData);
                            }
                        } catch (Exception _e) {
                            WriteLog.LogErrorFormat(DataName + "表格格式錯誤 ID:" + ID + "    Log: " + _e);
                        }
                        //WriteLog.LogWarning(string.Format("{0}表有不明屬性:{1}", DataName, key));
                        break;
                }
            }
        }
        public static ScriptEffectData GetData(int _id) {
            return GameDictionary.GetJsonData<ScriptEffectData>(DataName, _id);
        }
    }
}