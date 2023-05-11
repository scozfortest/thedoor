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
        public static string DataName { get; set; }
        public string Title { get; private set; }
        public List<string> NextIDs = new List<string>();
        public string Content {
            get {
                string str = StringData.GetString_static(DataName + "_" + ID, "Content");
                if (Requires != null) {
                    foreach (var require in Requires) {
                        str += require.GetRequireStr();
                    }
                }
                return str;
            }
        }
        public string EndType { get; private set; }
        public string EndValue { get; private set; }
        public string RefImg { get; private set; }
        public string RefSound { get; private set; }
        public string RefBGM { get; private set; }
        public string RefVoice { get; private set; }
        public bool HaveOptions { get; private set; }
        public HashSet<string> CamEffects { get; private set; }
        public float CamShake { get; private set; }
        public List<ScriptRequireData> Requires { get; private set; }//選擇此選項需求的條件
        public List<ItemData> GainItems { get; private set; }//立刻獲得物品清單
        public List<ItemData> RewardItems { get; private set; }//戰鬥獲勝獎勵物品清單
        public List<TargetEffectData> MyEffects = new List<TargetEffectData>();//觸發腳色效果清單


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
            ItemType tmpItemType = ItemType.Gold;
            ItemData tmpItemData = null;
            Target tmpTarget = Target.Enemy;
            EffectType tmpTEffectType = EffectType.Attack;
            int tmpTypeValue = 0;

            foreach (string key in item.Keys) {
                switch (key) {
                    case "ID":
                        ID = item[key].ToString();
                        break;
                    case "Title":
                        Title = item[key].ToString();
                        TitleScriptDic.Add(Title, this);
                        break;
                    case "NextIDs":
                        NextIDs = item[key].ToString().Split('/').ToList();
                        HaveOptions = NextIDs.Count > 1;
                        NextIDs.RemoveAll(a => string.IsNullOrEmpty(a));
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
                        try {
                            if (key.Contains("Requirement")) { //選擇此選項需求的條件
                                tmpRequireTypeStr = item[key].ToString();
                            } else if (key.Contains("RequireValue")) {
                                if (!string.IsNullOrEmpty(tmpRequireTypeStr)) {
                                    var requireType = MyEnum.ParseEnum<ScriptRequireType>(tmpRequireTypeStr);
                                    var requireData = new ScriptRequireData(requireType, item[key].ToString());
                                    if (Requires == null) Requires = new List<ScriptRequireData>();
                                    Requires.Add(requireData);
                                }
                            } else if (key.Contains("GainType")) { //立刻獲得物品清單
                                tmpItemType = MyEnum.ParseEnum<ItemType>(item[key].ToString());
                                tmpItemData = null;
                            } else if (key.Contains("GainValue")) {
                                tmpItemData = new ItemData(tmpItemType, long.Parse(item[key].ToString()));
                                if (GainItems == null) GainItems = new List<ItemData>();
                                GainItems.Add(tmpItemData);
                            } else if (key.Contains("RewardType")) { //戰鬥獲勝獎勵物品清單
                                tmpItemType = MyEnum.ParseEnum<ItemType>(item[key].ToString());
                                tmpItemData = null;
                            } else if (key.Contains("RewardValue")) {
                                tmpItemData = new ItemData(tmpItemType, long.Parse(item[key].ToString()));
                                if (RewardItems == null) RewardItems = new List<ItemData>();
                                RewardItems.Add(tmpItemData);
                            } else if (key.Contains("Target")) {//觸發腳色效果清單
                                tmpTarget = MyEnum.ParseEnum<Target>(item[key].ToString());
                            } else if (key.Contains("EffectType")) {
                                tmpTEffectType = MyEnum.ParseEnum<EffectType>(item[key].ToString());
                            } else if (key.Contains("EffectValue")) {
                                tmpTypeValue = int.Parse(item[key].ToString());
                            } else if (key.Contains("EffectProb")) {
                                TargetEffectData tmpTEffectData = new TargetEffectData(tmpTarget, tmpTEffectType, float.Parse(item[key].ToString()), tmpTypeValue);
                                MyEffects.Add(tmpTEffectData);
                            }
                        } catch (Exception _e) {
                            WriteLog.LogErrorFormat(DataName + "表格格式錯誤 ID:" + ID + "    Log: " + _e);
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

        public bool MeetAllRequirements() {
            if (Requires != null) {
                foreach (var require in Requires) {
                    if (!require.MeetRequire()) return false;
                }
            }
            return true;
        }

    }
}
