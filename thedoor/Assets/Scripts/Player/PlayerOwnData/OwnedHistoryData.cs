using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using System.Linq;
using System;

namespace TheDoor.Main {
    /// <summary>
    /// 觸發事件紀錄
    /// </summary>
    public class TriggerEventRecord {
        public TriggerEvent Type { get; private set; }//觸發事件類型，參考列舉TriggerEvent
        public DateTime TriggerTime { get; private set; }//觸發時間
        public TriggerEventRecord(TriggerEvent _type, DateTime _claimTime) {
            Type = _type;
            TriggerTime = _claimTime;
        }
    }

    public class OwnedHistoryData : OwnedData {


        //限購商品
        public List<string> LimitShopItems = new List<string>();//已購買的限量商品UID清單
        public List<string> DailyLimitShopItems = new List<string>();//已購買的每日限量商品UID清單
        public List<string> LimitPurchaseItems = new List<string>();//已購買的限量儲值UID清單
        public List<string> DailyLimitPurchaseItems = new List<string>();//已購買的每日限量儲值UID清單

        public List<TriggerEventRecord> TriggerEventRecords = new List<TriggerEventRecord>();//觸發事件紀錄陣列

        public string BougthShopUID { get; set; }
        public int WatchADTimes { get; set; }

        public OwnedHistoryData(Dictionary<string, object> _data)
            : base(_data) {
        }
        public override void SetData(Dictionary<string, object> _data) {
            base.SetData(_data);
            object value;

            //已購買的限量商品UID清單
            try {
                LimitShopItems.Clear();
                if (_data.ContainsKey("LimitShopItems")) {
                    List<object> datas = _data["LimitShopItems"] as List<object>;
                    if (datas != null) {
                        LimitShopItems = datas.ConvertAll(a => a.ToString());
                    }
                }
            } catch (Exception _ex) {
                WriteLog.LogError("OwnedHistoryData已購買的限量商品UID清單資料錯誤: " + _ex);
            }

            //已購買的每日限量商品UID清單
            try {
                DailyLimitShopItems.Clear();
                if (_data.ContainsKey("DailyLimitShopItems")) {
                    List<object> datas = _data["DailyLimitShopItems"] as List<object>;
                    if (datas != null) {
                        DailyLimitShopItems = datas.ConvertAll(a => a.ToString());
                    }
                }
            } catch (Exception _ex) {
                WriteLog.LogError("OwnedHistoryData已購買的每日限量商品UID清單資料錯誤: " + _ex);
            }

            //已購買的限量儲值UID清單
            try {
                LimitPurchaseItems.Clear();
                if (_data.ContainsKey("LimitPurchaseItems")) {
                    List<object> datas = _data["LimitPurchaseItems"] as List<object>;
                    if (datas != null) {
                        LimitPurchaseItems = datas.ConvertAll(a => a.ToString());
                    }
                }
            } catch (Exception _ex) {
                WriteLog.LogError("OwnedHistoryData已購買的限量儲值UID清單資料錯誤: " + _ex);
            }

            //已購買的每日限量儲值UID清單
            try {
                DailyLimitPurchaseItems.Clear();
                if (_data.ContainsKey("DailyLimitPurchaseItems")) {
                    List<object> datas = _data["DailyLimitPurchaseItems"] as List<object>;
                    if (datas != null) {
                        DailyLimitPurchaseItems = datas.ConvertAll(a => a.ToString());
                    }
                }
            } catch (Exception _ex) {
                WriteLog.LogError("OwnedHistoryData已購買的每日限量儲值UID清單資料錯誤: " + _ex);
            }


            //觸發事件記錄陣列
            try {
                TriggerEventRecords.Clear();
                if (_data.ContainsKey("TriggerEvents")) {
                    var triggerEvents = _data["TriggerEvents"] as List<object>;
                    if (triggerEvents != null) {
                        for (int i = 0; i < triggerEvents.Count; i++) {
                            IDictionary dic = triggerEvents[i] as IDictionary;
                            Dictionary<string, object> eventRecordDic = DictionaryExtension.ConvertToStringKeyDic(dic);
                            if (MyEnum.TryParseEnum(eventRecordDic["Type"].ToString(), out TriggerEvent _type)) {
                                DateTime dateTime = FirebaseManager.GetDateTimeFromFirebaseTimestamp(eventRecordDic["TriggerTime"]);
                                var eventRecord = new TriggerEventRecord(_type, dateTime);
                                TriggerEventRecords.Add(eventRecord);
                            }
                        }
                    }
                }

            } catch (Exception _ex) {
                WriteLog.LogError("OwnedHistoryData成就資料錯誤: " + _ex);
            }

            // 最後購買內購的紀錄商城商品UID
            BougthShopUID = _data.TryGetValue("BougthShopUID", out value) ? Convert.ToString(value) : default(string);

        }


        /// <summary>
        /// 傳入限量類型與商品UID，取得該商品購買次數(只有限量商品才會記錄購買紀錄)
        /// </summary>
        public int GetLimitShopBuyCount(BuyLimitType _type, string _shopUID) {
            switch (_type) {
                case BuyLimitType.Permanence:
                    return LimitShopItems.Where(a => a == _shopUID).Count();
                case BuyLimitType.Daily:
                    return DailyLimitShopItems.Where(a => a == _shopUID).Count();
            }
            return 999999;
        }
        /// <summary>
        /// 傳入限量類型與儲值品項UID，取得該儲值品項購買次數(只有限量儲值品項才會記錄購買紀錄)
        /// </summary>
        public int GetLimitPurchaseBuyCount(BuyLimitType _type, string _PurchaseUID) {
            switch (_type) {
                case BuyLimitType.Permanence:
                    return LimitPurchaseItems.Where(a => a == _PurchaseUID).Count();
                case BuyLimitType.Daily:
                    return DailyLimitPurchaseItems.Where(a => a == _PurchaseUID).Count();
            }
            return 999999;
        }

        /// <summary>
        /// 設定
        /// </summary>
        public void SetBougthShopUID(string bougthShopUID) {
            BougthShopUID = bougthShopUID;
            FirebaseManager.SetBougthShopUID(bougthShopUID, null);
        }
    }
}
