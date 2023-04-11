using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using LitJson;
using System;
using System.Linq;

namespace TheDoor.Main {
    public class RolePlotData : MyJsonData {
        public static string DataName { get; private set; }
        public string Talker {
            get {
                string str = StringData.GetString_static(DataName + "_" + ID, "Talker");
                str = StringReplacer.GetReplacedStr(StringReplacer.StringReplaceType.TalkerName, str);
                return str;
            }
        }
        public string Conversation {
            get {
                string str = StringData.GetString_static(DataName + "_" + ID, "Conversation");
                str = StringReplacer.GetReplacedStr(StringReplacer.StringReplaceType.TalkerName, str);
                return str;
            }
        }
        public int Posture { get; private set; } = 0;
        /// <summary>
        /// 取得所有選項
        /// </summary>
        public string[] GetOptions() {
            string str = StringData.GetString_static(DataName + "_" + ID, "Option");
            string[] strs = str.Split('/');
            return strs;
        }
        /// <summary>
        /// 取得第X個選項
        /// </summary>
        public string GetOption(int _index) {
            string[] options = GetOptions();
            if (options == null)
                return "";
            if (_index < options.Length && options[_index] != null)
                return options[_index];
            return "";
        }
        public int[] NextIDs { get; private set; }
        public enum TalkUIType {
            Normal,//一般
            Option,//選項
            RPS,//剪刀石頭布
        }
        public TalkUIType MyTalkUIType { get; private set; } = TalkUIType.Normal;
        public string RefBG { get; private set; }
        public string RefSound { get; private set; }
        public string RefBGM { get; private set; }
        public string RefVoice { get; private set; }
        static Dictionary<int, int> GroupDic = new Dictionary<int, int>();

        public static void ClearStaticDic() {
            GroupDic.Clear();
        }
        protected override void GetDataFromJson(JsonData _item, string _dataName) {
            DataName = _dataName;
            JsonData item = _item;
            foreach (string key in item.Keys) {
                switch (key) {
                    case "ID":
                        ID = int.Parse(item[key].ToString());
                        break;
                    case "Group":
                        int groupID = int.Parse(item[key].ToString());
                        GroupDic.Add(groupID, ID);
                        break;
                    case "Posture":
                        Posture = int.Parse(item[key].ToString());
                        break;
                    case "TalkUIType":
                        MyTalkUIType = MyEnum.ParseEnum<TalkUIType>(item[key].ToString());
                        break;
                    case "NextIDs":
                        NextIDs = TextManager.StringSplitToIntArray(item[key].ToString(), '/');
                        break;
                    case "RefBG":
                        RefBG = item[key].ToString();
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
                        DebugLogger.LogWarning(string.Format("{0}表有不明屬性:{1}", DataName, key));
                        break;
                }
            }
        }
        public static RolePlotData GetData(int _id) {
            return GameDictionary.GetJsonData<RolePlotData>(DataName, _id);
        }
        /// <summary>
        /// 傳入選項索引，取得下一句對話
        /// </summary>
        public RolePlotData GetNextRolePlot(int _index = 0) {
            if (NextIDs == null || NextIDs.Length == 0)
                return null;
            if (_index >= NextIDs.Length) {
                DebugLogger.LogErrorFormat("GetNextRolePlot下一句對話傳入的索引超出範圍");
                return null;
            }
            int nextID = NextIDs[_index];
            return GetData(nextID);
        }
        public void GetBGSprite(Action<Sprite> _ac) {
            if (string.IsNullOrEmpty(RefBG))
                _ac?.Invoke(null);
            AddressablesLoader.GetSprite(RefBG, (sprite, handle) => {
                if (sprite != null) {
                    _ac?.Invoke(sprite);
                }
            });
        }
        public static RolePlotData GetRolePlotByGoupID(int _groupID) {
            if (!GroupDic.ContainsKey(_groupID))
                return null;
            return GetData(GroupDic[_groupID]);
        }

    }
}
