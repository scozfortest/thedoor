using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using UnityEngine.UI;
using System;

namespace TheDoor.Main {
    /// <summary>
    /// 觸發事件
    /// </summary>
    public enum TriggerEvent {
        None,//無任何事件
        PlayMaJam,//玩麻將房 參數填麻將房ID
        ShopBuy_One,//購買福袋(單抽) 參數填入GameData-Shop的UID
        Request_GoLinkUI,//跳出導引至UI 參數為LinkUIType類型        
    }

    public class TriggerEventHandler {

        public static void PlayEvent(TriggerEvent _event, string _param, Action _cb = null) {
            if (FirestoreGameSetting.RecordableEvents.Contains(_event.ToString()))//如果是需要紀錄的事件就送CF
                FirebaseManager.TriggerEvent(_event);
            switch (_event) {
                case TriggerEvent.PlayMaJam:
                    _cb?.Invoke();
                    break;
                case TriggerEvent.ShopBuy_One:
                    //GamePlayer.Instance.ShopBuy(_param, BuyCount.One, dataObj => {
                    //    var returnItemDic = DataHandler.ConvertDataObjToReturnItemDic(dataObj);
                    //    PopupUI.ShowGetItems(returnItemDic["ReturnGainItems"], returnItemDic["ReplaceGainItems"], true, () => {
                    //        ShopUI.GetInstance<ShopUI>()?.Refresh();//刷新ShopUI
                    //        _cb?.Invoke();
                    //    });
                    //});
                    break;
            }
        }
        /// <summary>
        ///  傳入觸發時機 若有符合條件的事件就觸發事件
        /// </summary>
        public static void TriggerEventCheck(TriggerEventData.TriggerTiming _timing) {
            //DebugLogger.LogError("TriggerEventCheck: " + _timing);
            DoEvent(0, _timing);
        }
        /// <summary>
        /// 遞迴觸發事件
        /// </summary>
        static void DoEvent(int _index, TriggerEventData.TriggerTiming _timing) {
            if (_index >= FirestoreGameSetting.TriggerEvents.Count) return;
            bool needTriggerEvent = FirestoreGameSetting.TriggerEvents[_index].NeedTriggerEvent(_timing);
            if (needTriggerEvent) {
                PlayEvent(FirestoreGameSetting.TriggerEvents[_index].MyTriggerEvent, FirestoreGameSetting.TriggerEvents[_index].TriggerEventParam, () => {
                    //執行下一個事件
                    _index++;
                    DoEvent(_index, _timing);
                });
            } else {
                //執行下一個事件
                _index++;
                DoEvent(_index, _timing);
            }
        }
    }
}