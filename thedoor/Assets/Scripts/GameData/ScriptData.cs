using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using LitJson;
using System;
using System.Linq;

namespace TheDoor.Main {
    public class ScriptData : MyJsonData {
        public new string ID { get; private set; }
        public static string DataName { get; private set; }

        public string Content {
            get {
                string str = StringData.GetString_static(DataName + "_" + ID, "Conversation");
                //str = StringReplacer.GetReplacedStr(StringReplacer.StringReplaceType.TalkerName, str);
                return str;
            }
        }
        public bool Exclusive { get; private set; }
        public List<string> NextIDs = new List<string>();
        public string EndType { get; private set; }
        public string EndValue { get; private set; }
        public string RefImg { get; private set; }
        public string RefSound { get; private set; }
        public string RefBGM { get; private set; }
        public string RefVoice { get; private set; }
        public List<RequireData> Requires { get; private set; }

        /// <summary>
        /// 取得所有選項
        /// </summary>
        public string[] GetOptions() {
            string str = StringData.GetString_static(DataName + "_" + ID, "Option");
            string[] strs = str.Split('/');
            return strs;
        }
        static Dictionary<string, ScriptData> TitleScriptDic = new Dictionary<string, ScriptData>();

        public static void ClearStaticDic() {
            TitleScriptDic.Clear();
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
                    case "Title":
                        string title = item[key].ToString();
                        TitleScriptDic.Add(title, this);
                        break;
                    case "Exclusive":
                        Exclusive = bool.Parse(item[key].ToString());
                        break;
                    case "NextIDs":
                        NextIDs = item[key].ToString().Split('/').ToList();
                        break;
                    case "End":
                        EndType = item[key].ToString();
                        break;
                    case "EndValue":
                        EndValue = item[key].ToString();
                        break;
                    case "RefImg":
                        RefImg = item[key].ToString();
                        break;
                    case "RefSound":
                        RefSound = item[key].ToString();
                        break;
                    case "RefBGM":
                        RefBGM = item[key].ToString();
                        break;
                    case "RefVoice":
                        RefVoice = item[key].ToString();
                        break;
                    default:
                        if (key.Contains("Requirement")) {
                            tmpRequireTypeStr = item[key].ToString();
                        } else if (key.Contains("RequireValue")) {
                            if (!string.IsNullOrEmpty(tmpRequireTypeStr)) {
                                var requireType = MyEnum.ParseEnum<RequireType>(tmpRequireTypeStr);
                                var requireData = new RequireData(requireType, item[key].ToString());
                                if (Requires == null) Requires = new List<RequireData>();
                                Requires.Add(requireData);
                            }
                        }
                        //DebugLogger.LogWarning(string.Format("{0}表有不明屬性:{1}", DataName, key));
                        break;
                }
            }
        }
        public static ScriptData GetData(string _id) {
            return GameDictionary.GetJsonData<ScriptData>(DataName, _id);
        }
        /// <summary>
        /// 傳入選項索引，取得下一句對話
        /// </summary>
        public ScriptData GetNextScript(int _index = 0) {
            if (NextIDs == null || NextIDs.Count == 0)
                return null;
            if (_index >= NextIDs.Count) {
                DebugLogger.LogErrorFormat("GetNextRolePlot下一句對話傳入的索引超出範圍");
                return null;
            }
            string nextID = NextIDs[_index];
            return GetData(nextID);
        }
        public void GetBGSprite(Action<Sprite> _ac) {
            if (string.IsNullOrEmpty(RefImg))
                _ac?.Invoke(null);
            AddressablesLoader.GetSprite(RefImg, (sprite, handle) => {
                if (sprite != null) {
                    _ac?.Invoke(sprite);
                }
            });
        }
        public static ScriptData GetScriptByTitle(string _title) {
            if (!TitleScriptDic.ContainsKey(_title))
                return null;
            return TitleScriptDic[_title];
        }

    }
}
