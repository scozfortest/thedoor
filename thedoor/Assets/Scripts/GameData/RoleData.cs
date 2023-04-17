using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using LitJson;
using System;
using System.Linq;

namespace TheDoor.Main {
    public class RoleData : MyJsonData {
        public static string DataName { get; set; }
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

        public string Ref { get; set; }
        public int Rank { get; private set; }
        public int HP { get; private set; }
        public int SanP { get; private set; }
        string TalentStr;
        TalentData MyTalentData {
            get {
                return TalentData.GetData(TalentStr);
            }
        }
        public RequireData Require { get; private set; }
        public List<int> RandomItems = new List<int>();
        public List<int> Items = new List<int>();
        public List<string> ExclusiveScripts = new List<string>();

        protected override void GetDataFromJson(JsonData _item, string _dataName) {
            DataName = _dataName;
            JsonData item = _item;
            string tmpRequireStr = "";
            foreach (string key in item.Keys) {
                switch (key) {
                    case "ID":
                        ID = int.Parse(item[key].ToString());
                        break;
                    case "Ref":
                        Ref = item[key].ToString();
                        break;
                    case "Rank":
                        Rank = int.Parse(item[key].ToString());
                        break;
                    case "HP":
                        HP = int.Parse(item[key].ToString());
                        break;
                    case "SanP":
                        SanP = int.Parse(item[key].ToString());
                        break;
                    case "Talent":
                        TalentStr = item[key].ToString();
                        break;
                    case "Requirement":
                        tmpRequireStr = item[key].ToString();
                        break;
                    case "RequireValue":
                        if (!string.IsNullOrEmpty(tmpRequireStr)) {
                            RequireType requireType = MyEnum.ParseEnum<RequireType>(tmpRequireStr);
                            Require = new RequireData(requireType, item[key].ToString());
                        }
                        break;
                    case "RandomItems":
                        RandomItems = TextManager.StringSplitToIntList(item[key].ToString(), ',');
                        break;
                    case "Items":
                        Items = TextManager.StringSplitToIntList(item[key].ToString(), ',');
                        break;
                    case "ExclusiveScripts":
                        ExclusiveScripts = item[key].ToString().Split(',').ToList();
                        break;
                    default:
                        WriteLog.LogWarning(string.Format("{0}表有不明屬性:{1}", DataName, key));
                        break;
                }
            }
        }

        public static RoleData GetData(int _id) {
            return GameDictionary.GetJsonData<RoleData>(DataName, _id);
        }
    }

}
