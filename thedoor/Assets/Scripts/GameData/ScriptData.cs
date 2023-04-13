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
        public string Title { get; private set; }
        public string Content {
            get {
                string str = StringData.GetString_static(DataName + "_" + ID, "Content");
                //str = StringReplacer.GetReplacedStr(StringReplacer.StringReplaceType.TalkerName, str);
                return str;
            }
        }
        public bool Exclusive { get; private set; }
        public List<string> NextIDs = new List<string>();
        public bool IsOption { get; private set; }
        public string EndType { get; private set; }
        public string EndValue { get; private set; }
        public int[] EffectIDs { get; private set; }
        public string RefImg { get; private set; }
        public string RefSound { get; private set; }
        public string RefBGM { get; private set; }
        public string RefVoice { get; private set; }
        public List<RequireData> Requires { get; private set; }
        public List<ItemData> ItemDatas { get; private set; }

        /// <summary>
        /// 取得所有選項
        /// </summary>
        public string[] GetOptions() {
            string str = StringData.GetString_static(DataName + "_" + ID, "Option");
            string[] strs = str.Split('/');
            return strs;
        }
        static Dictionary<string, ScriptData> TitleScriptDic = new Dictionary<string, ScriptData>();
        static List<string> NonExclusiveTitles = new();
        public static void ClearStaticDic() {
            TitleScriptDic.Clear();
        }
        protected override void GetDataFromJson(JsonData _item, string _dataName) {
            DataName = _dataName;
            JsonData item = _item;
            string tmpRequireTypeStr = "";
            ItemType tmpItemType = ItemType.Gold;
            ItemData tmpItemData = null;
            foreach (string key in item.Keys) {
                switch (key) {
                    case "ID":
                        ID = item[key].ToString();
                        break;
                    case "Title":
                        Title = item[key].ToString();
                        TitleScriptDic.Add(Title, this);
                        break;
                    case "Exclusive":
                        Exclusive = bool.Parse(item[key].ToString());
                        break;
                    case "NextIDs":
                        NextIDs = item[key].ToString().Split('/').ToList();
                        IsOption = NextIDs.Count > 1;
                        NextIDs.RemoveAll(a => string.IsNullOrEmpty(a));
                        break;
                    case "End":
                        EndType = item[key].ToString();
                        break;
                    case "EndValue":
                        EndValue = item[key].ToString();
                        break;
                    case "EffectIDs":
                        EffectIDs = TextManager.StringSplitToIntArray(item[key].ToString(), ',');
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
                            } else if (key.Contains("ItemType")) { //獲得物品
                                tmpItemType = MyEnum.ParseEnum<ItemType>(item[key].ToString());
                                tmpItemData = null;
                            } else if (key.Contains("ItemValue")) {
                                tmpItemData = new ItemData(tmpItemType, long.Parse(item[key].ToString()));
                                if (ItemDatas == null) ItemDatas = new List<ItemData>();
                                ItemDatas.Add(tmpItemData);
                            }
                        } catch (Exception _e) {
                            WriteLog.LogErrorFormat(DataName + "表格格式錯誤 ID:" + ID + "    Log: " + _e);
                        }
                        //DebugLogger.LogWarning(string.Format("{0}表有不明屬性:{1}", DataName, key));
                        break;
                }
            }
            //把非獨特的劇本加入清單中，在隨機取劇本時用
            if (!string.IsNullOrEmpty(Title) && !Exclusive)
                NonExclusiveTitles.Add(Title);
        }
        public static ScriptData GetData(string _id) {
            return GameDictionary.GetJsonData<ScriptData>(DataName, _id);
        }
        /// <summary>
        /// 傳入選項索引，取得下一句對話
        /// </summary>
        public ScriptData NextScript(int _index = 0) {
            if (NextIDs == null || NextIDs.Count == 0)
                return null;
            if (_index >= NextIDs.Count) {
                WriteLog.LogErrorFormat("GetNextScript下一句對話傳入的索引超出範圍");
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
        public static string GetRandomNonExclusiveTitle() {
            string randTitle = Probability.GetRandomTFromTList(NonExclusiveTitles);
            return randTitle;
        }
        public List<TargetEffectData> GetTargetEffects() {
            if (EffectIDs == null || EffectIDs.Length == 0) return null;
            List<TargetEffectData> targetEffects = new List<TargetEffectData>();
            for (int i = 0; i < EffectIDs.Length; i++) {
                var scriptEffectData = ScriptEffectData.GetData(EffectIDs[i]);
                targetEffects.AddRange(scriptEffectData.MyEffects);
            }
            return targetEffects;
        }

    }
}
