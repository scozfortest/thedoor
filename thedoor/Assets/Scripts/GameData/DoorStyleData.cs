using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using LitJson;
using System;
using System.Linq;

namespace TheDoor.Main {
    public class DoorStyleData : MyJsonData {
        public static string DataName { get; set; }
        public string Description {
            get {
                return StringData.GetString_static(DataName + "_" + ID, "Description");
            }
        }
        public string Ref { get; set; }
        HashSet<string> Tags;//分類


        protected override void GetDataFromJson(JsonData _item, string _dataName) {
            DataName = _dataName;
            JsonData item = _item;
            foreach (string key in item.Keys) {
                switch (key) {
                    case "ID":
                        ID = int.Parse(item[key].ToString());
                        break;
                    case "Ref":
                        Ref = item[key].ToString();
                        break;
                    case "Tags":
                        Tags = TextManager.GetHashSetFromSplitStr(item[key].ToString(), ',');
                        break;
                    default:
                        WriteLog.LogWarning(string.Format("{0}表有不明屬性:{1}", DataName, key));
                        break;
                }
            }
        }
        public bool BelongToTag(string _tag) {
            if (Tags == null) return false;
            return Tags.Contains(_tag);
        }

        public static DoorStyleData GetData(int _id) {
            return GameDictionary.GetJsonData<DoorStyleData>("DoorStyle", _id);
        }
        public static DoorStyleData GetRndDatas(string _tag = null) {
            var doorTypeDic = GameDictionary.GetIntKeyJsonDic<DoorStyleData>("DoorStyle");

            List<DoorStyleData> doorStyleDatas;
            if (string.IsNullOrEmpty(_tag))
                doorStyleDatas = doorTypeDic.Values.ToList();
            else
                doorStyleDatas = doorTypeDic.Values.ToList().FindAll(a => a.Tags.Contains(_tag));
            return Prob.GetRandomTFromTList(doorStyleDatas);
        }



    }

}
