using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using LitJson;
using SimpleJSON;

namespace Scoz.Func {
    public class GameSettingData : MyJsonData {
        public static string DataName { get; set; }
        public new string ID;
        //一般
        static Dictionary<GameSetting, string> SettingDic = new Dictionary<GameSetting, string>();


        public static int GetInt(GameSetting _type) {
            return int.Parse(SettingDic[_type]);
        }
        public static float GetFloat(GameSetting _type) {
            return float.Parse(SettingDic[_type]);
        }
        public static bool GetBool(GameSetting _type) {
            return bool.Parse(SettingDic[_type]);
        }
        public static byte GetByte(GameSetting _type) {
            return byte.Parse(SettingDic[_type]);
        }
        public static string GetStr(GameSetting _type) {
            return SettingDic[_type];
        }
        public static JSONNode GetJsNode(GameSetting _type) {
            string jsStr = GetStr(_type);
            JSONNode jsNode = JSONNode.Parse(jsStr);
            return jsNode;
        }
        public static void ClearStaticDic() {
            SettingDic.Clear();
        }
        protected override void GetDataFromJson(JsonData _item, string _dataName) {
            DataName = _dataName;
            JsonData item = _item;
            if (item.Keys.Contains("ID") && item.Keys.Contains("Value")) {
                string id = item["ID"].ToString();
                var key = MyEnum.ParseEnum<GameSetting>(id);
                SettingDic.Add(key, item["Value"].ToString());
            }
        }
    }
}
