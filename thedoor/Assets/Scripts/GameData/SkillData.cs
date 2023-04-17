using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using LitJson;

namespace TheDoor.Main {
    public class SkillData : MyJsonData {
        public static string DataName { get; private set; }
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
        public float Probability { get; private set; }
        public SkillType MyType { get; private set; }
        public List<string> Values { get; private set; } = new List<string>();


        protected override void GetDataFromJson(JsonData _item, string _dataName) {
            DataName = _dataName;
            JsonData item = _item;
            Values.Clear();
            foreach (string key in item.Keys) {
                switch (key) {
                    case "ID":
                        ID = int.Parse(item[key].ToString());
                        break;
                    case "Ref":
                        Ref = item[key].ToString();
                        break;
                    case "SkillType":
                        MyType = MyEnum.ParseEnum<SkillType>(item[key].ToString());
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

        public static SkillData GetData(string _id) {
            return GameDictionary.GetJsonData<SkillData>("Skill", _id);
        }
    }
}