using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using System.Linq;
using System;

namespace TheDoor.Main {


    public class TriggerEventData {
        public enum TriggerTiming {
            //以下擇一觸發
            EnterLobby_LoseGame,//輸麻將回大廳時觸發
            EnterLobby_Always,//沒輸麻將回到大廳時觸發
            EnterLobby_First,//第一次進大廳時觸發

            //以下擇一觸發
            Lobby_CloseMainUI,//關閉主要視窗(麻將房、商店、社群、休息站、生涯)回到大廳介面時觸發

            //以下擇一觸發
            NotEnoughGold,//購買時不夠金幣時觸發
            NotEnoughPoint,//購買時不夠點數時觸發
            NotEnoughBall,//購買時不夠小鋼珠時觸發

            //以下擇一觸發
            GainRoleCard,//獲得腳色卡後觸發

        }
        public enum ConditionType {
            GoldBelow,//金幣低於 參數為金幣數
            PointBelow,//點數低於 參數為金板數
            BallBelow,//小鋼珠低於 參數為小鋼珠數
            CD,//冷卻 參數為秒數
            AvailableMiniGame,//有小遊戲可以玩 不用參數
            Probability,//機率性觸發 參數為機率(填0.3就是30%)
            //(只有TriggerTiming為EnterLobby_Always時才有用) 參數為贏台數高於
            WinMaJamPointAbove,
            //(只有GameData-Setting>TriggerEvent中的RecordableEvent有填入的TriggerEventType才會有用) 參數為玩家這個帳號目前觸發次數低於
            TriggerTimesBelow,
            //(只有TriggerTiming為EnterLobby_LoseGame或EnterLobby_Always時才有用) 上一場為某貨幣類型的房間
            LastGameBetType,
        }
        public struct Condition {

            public ConditionType Type;
            public object Value;
            public Condition(ConditionType _type, object _value) {
                Type = _type;
                Value = _value;
            }
        }
        public TriggerTiming Timing { get; private set; }
        List<Condition> Conditions = new List<Condition>();
        public TriggerEvent MyTriggerEvent { get; private set; }
        public string TriggerEventParam { get; private set; }
        public bool IsLegalData { get; private set; } = false;
        public int Priority { get; private set; } = 0;

        public TriggerEventData(Dictionary<string, object> _data) {
            object value;
            IsLegalData = false;
            string timingStr = _data.TryGetValue("TriggerTiming", out value) ? Convert.ToString(value) : default(string);
            if (MyEnum.TryParseEnum(timingStr, out TriggerTiming _timing)) {
                Timing = _timing;
            } else {
                IsLegalData = false;
                return;
            }
            List<object> objList = _data.TryGetValue("Conditions", out value) ? value as List<object> : null;
            Conditions.Clear();
            if (objList != null) {
                List<string> conditionStrs = objList.ConvertAll(a => a.ToString());
                for (int i = 0; i < conditionStrs.Count; i++) {
                    SetCondition(conditionStrs[i]);
                }
            }
            string triggerEventStr = _data.TryGetValue("TriggerEvent", out value) ? Convert.ToString(value) : default(string);
            if (MyEnum.TryParseEnum(triggerEventStr, out TriggerEvent _triggerEvent)) {
                MyTriggerEvent = _triggerEvent;
            } else {
                IsLegalData = false;
                return;
            }
            Priority = _data.TryGetValue("Priority", out value) ? Convert.ToInt32(value) : 0;

            TriggerEventParam = _data.TryGetValue("TriggerEventParam", out value) ? Convert.ToString(value) : default(string);
            IsLegalData = true;
        }
        void SetCondition(string _conditionStr) {
            if (_conditionStr == null) return;
            string[] dataStrs = _conditionStr.Split(',');
            if (dataStrs == null || dataStrs.Length == 0) return;
            if (MyEnum.TryParseEnum(dataStrs[0], out ConditionType _type)) {
                object value = null;
                if (dataStrs.Length > 1)
                    value = dataStrs[1];
                Condition c = new Condition(_type, value);
                Conditions.Add(c);
            }
        }
        /// <summary>
        /// 傳入觸發時機 回傳是否觸發事件 true為觸發
        /// </summary>
        public bool NeedTriggerEvent(TriggerTiming _timing) {
            if (Timing != _timing) return false;
            for (int i = 0; i < Conditions.Count; i++) {
                switch (Conditions[i].Type) {
                    case ConditionType.CD:
                        float cdSecs = (float)Convert.ToDouble(Conditions[i].Value);
                        if (!CDChecker.DoneCD(_timing.ToString(), cdSecs)) {
                            return false;
                        }
                        break;
                    case ConditionType.GoldBelow:
                        long belowGold = Convert.ToInt64(Conditions[i].Value);
                        if (GamePlayer.Instance.Data.GetCurrency(Currency.Gold) >= belowGold)
                            return false;
                        break;
                    case ConditionType.PointBelow:
                        long belowPoint = Convert.ToInt64(Conditions[i].Value);
                        if (GamePlayer.Instance.Data.GetCurrency(Currency.Point) >= belowPoint)
                            return false;
                        break;
                    case ConditionType.Probability:
                        float probability = (float)Convert.ToDouble(Conditions[i].Value);
                        if (!Probability.GetResult(probability))
                            return false;
                        break;
                    case ConditionType.WinMaJamPointAbove:
                        if (_timing == TriggerTiming.EnterLobby_Always) {
                            int needWinMaJamPoint = Convert.ToInt32(Conditions[i].Value);
                            int curWinMaJamPoint = 0;
                            var result = GamePlayer.Instance.Data.MyLastMaJamGameResult;
                            if (result != null)
                                curWinMaJamPoint = result.TaiNumber;
                            //Debug.LogError("贏台數為" + curWinMaJamPoint);
                            //Debug.LogError("需求贏台數為" + needWinMaJamPoint);
                            if (curWinMaJamPoint <= needWinMaJamPoint) {//贏台數如果沒有大於指定條件就不觸發
                                //Debug.LogError("贏台數不足");
                                return false;
                            }
                        } else
                            return false;
                        break;
                    case ConditionType.TriggerTimesBelow:
                        //到商店點評分只會觸發一次，若已經有觸發過就不會再次觸發
                        var history = GamePlayer.Instance.MyHistoryData;
                        if (history != null) {
                            int limiTimes = Convert.ToInt32(Conditions[i].Value);
                            int curTriggerTimes = 0;
                            for (int j = 0; j < history.TriggerEventRecords.Count; j++) {
                                if (history.TriggerEventRecords[j].Type == MyTriggerEvent) {
                                    curTriggerTimes++;
                                }
                            }
                            //Debug.LogError("目前觸發次數: " + curTriggerTimes);
                            //Debug.LogError("限制次數: " + limiTimes);
                            if (curTriggerTimes >= limiTimes) {
                                //Debug.LogError("已經觸發" + curTriggerTimes + "次不再觸發");
                                return false;
                            }

                        }
                        break;
                    case ConditionType.LastGameBetType:
                        //(只有TriggerTiming為EnterLobby_LoseGame或EnterLobby_Always時才有用) 上一場為某貨幣類型的房間
                        if (_timing == TriggerTiming.EnterLobby_LoseGame || _timing == TriggerTiming.EnterLobby_Always) {
                            string needBetTypeStr = Conditions[i].Value.ToString();
                            if (MyEnum.TryParseEnum(needBetTypeStr, out Currency _needBetType)) {
                                var result = GamePlayer.Instance.Data.MyLastMaJamGameResult;
                                if (result == null || result.BetType != _needBetType)
                                    return false;
                            } else {
                                Debug.LogError("TriggerEventData填的LastGameBetType傳入參數格式錯誤: " + needBetTypeStr);
                                return false;
                            }
                        } else
                            return false;
                        break;
                    default:
                        DebugLogger.LogError("尚未加入enum的條件類型: " + Conditions[i].Type);
                        return false;
                }
            }
            return true;
        }
    }
}
