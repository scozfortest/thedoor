using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using System.Linq;
using System;

namespace TheDoor.Main {

    public class PurchaseData {
        public string UID { get; private set; }
        public string Name { get; private set; }
        public string Content { get; private set; }
        public DateTime CreateTime { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }
        public ItemData MyItemData { get; private set; }
        public string ImgPah { get; private set; }
        public SaleState MySaleState { get; private set; }
        public int Price { get; private set; }
        public int Priority { get; private set; }
        public BuyLimitType MyBuyLimitType { get; private set; } = BuyLimitType.None;
        public int BuyLimit { get; private set; } = 0;
        public float Discount { get; private set; } = 0;
        public bool IsLegalData { get; private set; } = false;
        /// <summary>
        /// 平台IAP產品品項UID
        /// </summary>
        public string ProductUID { get; private set; }
        public string InfoURL { get; private set; }
        public Tag MyTag { get; private set; }
        public enum Tag {
            Purchase,//儲值頁籤商品
            SpecialSet,//特惠儲值頁籤商品
            All,//所有(給IAPManager設定用)
        }

        /// <summary>
        /// 是否正在販售中(判斷開始時間跟結束時間)
        /// </summary>
        public bool IsReadyToSale {
            get {
                if (GameManager.Instance.NowTime < StartTime || (EndTime != default(DateTime) && GameManager.Instance.NowTime > EndTime))
                    return false;
                else
                    return true;
            }
        }

        public PurchaseData(Dictionary<string, object> _data) {
            IsLegalData = false;
            try {
                object value;
                UID = _data.TryGetValue("UID", out value) ? Convert.ToString(value) : default(string);
                Name = _data.TryGetValue("Name", out value) ? Convert.ToString(value) : default(string);
                Content = _data.TryGetValue("Content", out value) ? Convert.ToString(value) : default(string);
                CreateTime = _data.TryGetValue("CreateTime", out value) ? FirebaseManager.GetDateTimeFromFirebaseTimestamp(value) : default(DateTime);
                StartTime = _data.TryGetValue("StartTime", out value) ? FirebaseManager.GetDateTimeFromFirebaseTimestamp(value) : default(DateTime);
                EndTime = _data.TryGetValue("EndTime", out value) ? FirebaseManager.GetDateTimeFromFirebaseTimestamp(value) : default(DateTime);
                InfoURL = _data.TryGetValue("InfoURL", out value) ? Convert.ToString(value) : default(string);
                string itemTypeStr = _data.TryGetValue("ItemType", out value) ? Convert.ToString(value) : default(string);
                long itemValue = _data.TryGetValue("ItemValue", out value) ? Convert.ToInt64(value) : default(long);
                ItemType itemType;
                if (MyEnum.TryParseEnum(itemTypeStr, out itemType)) {
                    MyItemData = new ItemData(itemType, itemValue);
                } else {//有錯誤資料時視為無效資料
                    WriteLog.LogErrorFormat("儲值品項的ItemType錯誤 UID:" + UID);
                    IsLegalData = false;
                    return;
                }
                ImgPah = _data.TryGetValue("ImgPath", out value) ? Convert.ToString(value) : default(string);
                //設定販賣狀態
                string saleStateStr = _data.TryGetValue("SaleState", out value) ? Convert.ToString(value) : default(string);
                SaleState saleState;
                if (MyEnum.TryParseEnum(saleStateStr, out saleState))
                    MySaleState = saleState;
                else {//有錯誤資料時視為無效資料
                    WriteLog.LogErrorFormat("儲值品項的SaleState錯誤 UID:" + UID);
                    IsLegalData = false;
                    return;
                }
                //設定商品頁籤
                string tagStr = _data.TryGetValue("Tag", out value) ? Convert.ToString(value) : default(string);
                Tag myTag;
                if (MyEnum.TryParseEnum(tagStr, out myTag))
                    MyTag = myTag;
                else {//有錯誤資料時視為無效資料
                    WriteLog.LogErrorFormat("儲值品項的Tag錯誤 UID:" + UID);
                    IsLegalData = false;
                    return;
                }
                Price = _data.TryGetValue("Price", out value) ? Convert.ToInt32(value) : default(int);
                Priority = _data.TryGetValue("Priority", out value) ? Convert.ToInt32(value) : default(int);
                //設定販賣限定類型
                MyBuyLimitType = BuyLimitType.None;
                if (_data.ContainsKey("BuyLimitType") && _data.ContainsKey("BuyLimit")) {
                    string buyLimitTypeStr = _data.TryGetValue("BuyLimitType", out value) ? Convert.ToString(value) : default(string);
                    BuyLimitType buyLimitType;
                    if (MyEnum.TryParseEnum(buyLimitTypeStr, out buyLimitType)) {
                        MyBuyLimitType = buyLimitType;
                        BuyLimit = _data.TryGetValue("BuyLimit", out value) ? Convert.ToInt32(value) : default(int);
                        if (BuyLimit <= 0)//限購數量<=0就當沒有限購
                            MyBuyLimitType = BuyLimitType.None;
                    } else {//有錯誤資料時視為無效資料
                        WriteLog.LogErrorFormat("儲值品項的BuyLimitType錯誤 UID:" + UID);
                        IsLegalData = false;
                        return;
                    }
                }
                Discount = _data.TryGetValue("Discount", out value) ? (float)Convert.ToDouble(value) : default(float);
                if (Discount > 1 || Discount < 0) {//不合理的折扣視為無效資料
                    IsLegalData = false;
                    return;
                }

                ProductUID = _data.TryGetValue("ProductUID", out value) ? Convert.ToString(value) : default(string);
                IsLegalData = true;
            } catch (Exception _e) {//有錯誤資料時視為無效資料
                WriteLog.LogErrorFormat("儲值品項資料錯誤 UID:" + UID);
                IsLegalData = false;
                WriteLog.LogError(_e);
            }
        }
    }

}
