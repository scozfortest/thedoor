using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using LitJson;

namespace TheDoor.Main {
    public enum ScriptType {
        Main,//主線
        Side,//支線
    }
    public class ScriptTitleData : MyJsonData {
        public static string DataName { get; set; }
        public new string ID { get; set; }
        public ScriptType MyScriptType { get; private set; }
        public List<RequireData> Requires { get; private set; }

        public static Dictionary<ScriptType, List<ScriptTitleData>> ScriptTypeDic = new Dictionary<ScriptType, List<ScriptTitleData>>();
        public static void ClearStaticDic() {
            ScriptTypeDic.Clear();
        }

        protected override void GetDataFromJson(JsonData _item, string _dataName) {
            DataName = _dataName;
            JsonData item = _item;
            string tmpRequireTypeStr = "";
            foreach (string key in item.Keys) {
                switch (key) {
                    case "ID":
                        ID = item[key].ToString();
                        break;
                    case "ScriptType":
                        MyScriptType = MyEnum.ParseEnum<ScriptType>(item[key].ToString());
                        break;
                    default:
                        try {
                            if (key.Contains("Requirement")) { //需求條件
                                tmpRequireTypeStr = item[key].ToString();
                            } else if (key.Contains("RequireValue")) {
                                if (!string.IsNullOrEmpty(tmpRequireTypeStr)) {
                                    var requireType = MyEnum.ParseEnum<RequireType>(tmpRequireTypeStr);
                                    var requireData = new RequireData(requireType, item[key].ToString());
                                    if (Requires == null) Requires = new List<RequireData>();
                                    Requires.Add(requireData);
                                }
                            }
                        } catch (Exception _e) {
                            WriteLog.LogErrorFormat(DataName + "表格格式錯誤 ID:" + ID + "    Log: " + _e);
                        }
                        //DebugLogger.LogWarning(string.Format("{0}表有不明屬性:{1}", DataName, key));
                        break;
                }
            }
            if (ScriptTypeDic.ContainsKey(MyScriptType))
                ScriptTypeDic[MyScriptType].Add(this);
            else
                ScriptTypeDic.Add(MyScriptType, new List<ScriptTitleData>() { this });

        }
        public static ScriptTitleData GetData(int _id) {
            return GameDictionary.GetJsonData<ScriptTitleData>(DataName, _id);
        }
        public static ScriptTitleData GetRndDataByType(ScriptType _type) {
            return Prob.GetRandomTFromTList(ScriptTypeDic[_type]);
        }
    }
}