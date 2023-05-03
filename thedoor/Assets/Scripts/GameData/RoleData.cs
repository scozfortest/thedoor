using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using LitJson;
using System;
using System.Linq;

namespace TheDoor.Main {
    public class RoleData : MyJsonData, IItemJsonData {
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
        public bool Lock { get; private set; }
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
        public ItemType MyItemType { get; } = ItemType.Role;

        public List<int> Supplies = new List<int>();
        public HashSet<string> ExclusiveScripts = new HashSet<string>();
        public HashSet<int> Melees = new HashSet<int>();

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
                    case "Lock":
                        Lock = bool.Parse(item[key].ToString());
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
                    case "Melees":
                        Melees = TextManager.GetIntHashSetFromSplitStr(item[key].ToString(), ',');
                        break;
                    case "Supplies":
                        Supplies = TextManager.StringSplitToIntList(item[key].ToString(), ',');
                        break;
                    case "ExclusiveScripts":
                        ExclusiveScripts = TextManager.GetHashSetFromSplitStr(item[key].ToString(), ',');
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


        /// <summary>
        /// 取得隨機已解鎖腳色
        /// </summary>
        /// <returns></returns>
        public static RoleData GetRandAvailableData() {
            var roleDic = GameDictionary.GetIntKeyJsonDic<RoleData>("Role");
            var roleDats = roleDic.Values.ToList().FindAll(a => !a.Lock);
            return Prob.GetRandomTFromTList(roleDats);
        }

    }

}
