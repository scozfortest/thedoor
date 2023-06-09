using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using LitJson;
using System;
using System.Linq;

namespace TheDoor.Main {
    public class ItemGroupData : MyJsonData {
        public static string DataName { get; set; }

        public string Name {
            get {
                return StringData.GetString_static(DataName + "_" + ID, "Name");
            }
        }
        public string Ref { get; set; }
        public int Rank { get; private set; }
        public List<ItemData> ItemDatas = new List<ItemData>();
        protected override void GetDataFromJson(JsonData _item, string _dataName) {
            DataName = _dataName;
            JsonData item = _item;
            ItemType tmpItemType = ItemType.Gold;
            ItemData tmpItemData = null;
            ItemDatas.Clear();
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
                    default:
                        try {
                            if (key.Contains("ItemType")) {
                                tmpItemType = MyEnum.ParseEnum<ItemType>(item[key].ToString());
                                tmpItemData = null;
                            } else if (key.Contains("ItemValue")) {
                                tmpItemData = new ItemData(tmpItemType, long.Parse(item[key].ToString()));
                                ItemDatas.Add(tmpItemData);
                            }
                        } catch (Exception _e) {
                            WriteLog.LogErrorFormat(DataName + "表格格式錯誤 ID:" + ID + "    Log: " + _e);
                        }
                        //WriteLog.LogWarning(string.Format("{0}表有不明屬性:{1}", DataName, key));
                        break;
                }
            }
        }
        public static ItemGroupData GetData(int _id) {
            return GameDictionary.GetJsonData<ItemGroupData>(DataName, _id);
        }
    }

}
