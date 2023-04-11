using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using System.Linq;
using System;

namespace TheDoor.Main {

    public class ShopData {
        public string UID { get; private set; }
        public string Name { get; private set; }
        public DateTime CreateTime { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }
        public ItemData MyItemData { get; private set; }
        public Dictionary<Currency, long> Price_One { get; private set; } = new Dictionary<Currency, long>();
        public Dictionary<Currency, long> Price_Ten { get; private set; } = new Dictionary<Currency, long>();
        public string ImgPah { get; private set; }
        public SaleState MySaleState { get; private set; }
        public int Priority { get; private set; }
        public BuyLimitType MyBuyLimitType { get; private set; } = BuyLimitType.None;
        public int BuyLimit { get; private set; } = 0;
        public float Discount { get; private set; } = 0;
        public bool IsLegalData { get; private set; } = false;
        public string InfoURL { get; private set; }
        public string Style { get; private set; }
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

        public ShopData(Dictionary<string, object> _data) {
            IsLegalData = false;
            try {
                object value;
                UID = _data.TryGetValue("UID", out value) ? Convert.ToString(value) : default(string);
                if (UID == null || UID == "") {
                    DebugLogger.LogErrorFormat("商城品項的UID錯誤 UID:" + UID);
                    IsLegalData = false;
                    return;
                }
                Name = _data.TryGetValue("Name", out value) ? Convert.ToString(value) : default(string);
                Style = _data.TryGetValue("Style", out value) ? Convert.ToString(value) : "Classic";
                CreateTime = _data.TryGetValue("CreateTime", out value) ? FirebaseManager.GetDateTimeFromFirebaseTimestamp(value) : default(DateTime);
                StartTime = _data.TryGetValue("StartTime", out value) ? FirebaseManager.GetDateTimeFromFirebaseTimestamp(value) : default(DateTime);
                EndTime = _data.TryGetValue("EndTime", out value) ? FirebaseManager.GetDateTimeFromFirebaseTimestamp(value) : default(DateTime);
                InfoURL = _data.TryGetValue("InfoURL", out value) ? Convert.ToString(value) : default(string);
                string itemTypeStr = _data.TryGetValue("ItemType", out value) ? Convert.ToString(value) : default(string);
                long itemValue = _data.TryGetValue("ItemValue", out value) ? Convert.ToInt64(value) : default(long);

                ItemType itemType;
                if (MyEnum.TryParseEnum(itemTypeStr, out itemType))
                    MyItemData = new ItemData(itemType, itemValue);
                else {//有錯誤資料時視為無效資料
                    DebugLogger.LogErrorFormat("商城品項的ItemType錯誤 UID:" + UID);
                    IsLegalData = false;
                    return;
                }
                //設定PriceOne
                Price_One.Clear();
                if (_data.TryGetValue("Price_One", out value)) {
                    Dictionary<string, object> priceData = value as Dictionary<string, object>;
                    if (priceData != null) {
                        Price_One = GetPrice(priceData);
                    }
                }
                //設定PriceTen
                Price_Ten.Clear();
                if (_data.TryGetValue("Price_Ten", out value)) {
                    Dictionary<string, object> priceData = value as Dictionary<string, object>;
                    if (priceData != null) {
                        Price_Ten = GetPrice(priceData);
                    }
                }
                ImgPah = _data.TryGetValue("ImgPath", out value) ? Convert.ToString(value) : default(string);
                //設定販賣狀態
                string saleStateStr = _data.TryGetValue("SaleState", out value) ? Convert.ToString(value) : default(string);
                SaleState saleState;
                if (MyEnum.TryParseEnum(saleStateStr, out saleState))
                    MySaleState = saleState;
                else {//有錯誤資料時視為無效資料
                    DebugLogger.LogErrorFormat("商城品項的SaleState錯誤 UID:" + UID);
                    IsLegalData = false;
                    return;
                }
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
                        DebugLogger.LogErrorFormat("商城品項的BuyLimitType錯誤 UID:" + UID);
                        IsLegalData = false;
                        return;
                    }
                }
                Discount = _data.TryGetValue("Discount", out value) ? (float)Convert.ToDouble(value) : default(float);
                if (Discount > 1 || Discount < 0) {//不合理的折扣視為無效資料
                    IsLegalData = false;
                    return;
                }
                IsLegalData = true;
            } catch (Exception _e) {//有錯誤資料時視為無效資料
                DebugLogger.LogErrorFormat("商城品項資料錯誤 UID:" + UID);
                IsLegalData = false;
                DebugLogger.LogError(_e);
            }
        }
        Dictionary<Currency, long> GetPrice(Dictionary<string, object> _priceData) {

            Dictionary<Currency, long> price = new Dictionary<Currency, long>();
            foreach (var data in _priceData) {
                Currency currency;
                if (MyEnum.TryParseEnum(data.Key, out currency)) {
                    price.Add(currency, Convert.ToInt32(data.Value));
                }
            }
            return price;
        }
        public long GetPrice(Currency _currency, BuyCount _buyCount) {
            long price = 0;
            switch (_buyCount) {
                case BuyCount.One:
                    Price_One.TryGetValue(_currency, out price);
                    break;
                case BuyCount.Ten:
                    Price_Ten.TryGetValue(_currency, out price);
                    break;
            }
            return price;
        }
    }

}
