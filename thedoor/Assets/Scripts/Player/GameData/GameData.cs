using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using System.Linq;
using System;

namespace TheDoor.Main {

    public class GameData {
        static List<ShopData> ShopDataList { get; set; } = new List<ShopData>();
        /// <summary>
        /// Firebase初始化後開始載資料前要執行的內容
        /// </summary>
        public static void OnFirebaseInit() {
        }

        public static ShopData GetShopData(string _shopUID) {
            ShopData shopData = ShopDataList.Find(a => a.UID == _shopUID);
            return shopData;
        }
        /// <summary>
        /// 取得商品清單
        /// 1. 排除已達購買上限的商品
        /// </summary>
        public static List<ShopData> GetShopDatas() {
            List<ShopData> shopDatas = ShopDataList.FindAll(a => {
                //已達限量次數的商品不用存
                if (a.MyBuyLimitType != BuyLimitType.None) {
                    var history = GamePlayer.Instance.MyHistoryData;
                    if (history != null && history.GetLimitShopBuyCount(a.MyBuyLimitType, a.UID) >= a.BuyLimit)
                        return false;
                }
                return true;
            });
            return shopDatas;
        }
        static List<PurchaseData> PurchaseDataList { get; set; } = new List<PurchaseData>();
        public static PurchaseData GetPurchaseData(string _purchaseUID) {
            PurchaseData purchaseData = PurchaseDataList.Find(a => a.UID == _purchaseUID);
            return purchaseData;
        }
        /// <summary>
        /// 取得儲值品項清單
        /// 1. 排除已達購買上限的品項
        /// 2. 只取傳入頁籤的商品
        /// </summary>
        public static List<PurchaseData> GetPurchaseDatas(PurchaseData.Tag _tag) {
            var history = GamePlayer.Instance.MyHistoryData;
            List<PurchaseData> purchaseDatas = PurchaseDataList.FindAll(a => {
                //已達限量次數的商品不取
                if (a.MyBuyLimitType != BuyLimitType.None) {
                    if (history != null && history.GetLimitPurchaseBuyCount(a.MyBuyLimitType, a.UID) >= a.BuyLimit)
                        return false;
                }
                //不同頁籤不取
                if (_tag != PurchaseData.Tag.All && _tag != a.MyTag)
                    return false;
                return true;
            });
            return purchaseDatas;
        }
    }
}