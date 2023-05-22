using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using LitJson;
using System;
using System.Linq;

namespace TheDoor.Main {
    public class RestData : MyJsonData {
        public static string DataName { get; set; }
        public string Ref { get; set; }


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
                    default:
                        WriteLog.LogWarning(string.Format("{0}表有不明屬性:{1}", DataName, key));
                        break;
                }
            }
        }
        public static RestData GetData(int _id) {
            return GameDictionary.GetJsonData<RestData>("Rest", _id);
        }
        public static RestData GetRndData() {
            var doorTypeDic = GameDictionary.GetIntKeyJsonDic<RestData>("Rest");

            List<RestData> RestDatas = doorTypeDic.Values.ToList();
            return Prob.GetRandomTsFromTList(RestDatas);
        }



    }

}
