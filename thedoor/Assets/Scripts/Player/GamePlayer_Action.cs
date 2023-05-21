using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using System;
using System.Linq;
using UnityEngine.Purchasing.Security;
using UnityEngine.SceneManagement;

namespace TheDoor.Main {

    public partial class GamePlayer : MyPlayer {


        public void GainItems(List<ItemData> _datas) {
            Dictionary<Currency, long> gainCurrency = new Dictionary<Currency, long>();
            List<SupplyData> supplies = new List<SupplyData>();

            foreach (var data in _datas) {
                switch (data.Type) {
                    case ItemType.Gold:
                    case ItemType.Point:
                        if (MyEnum.TryParseEnum(data.Type.ToString(), out Currency _type)) {
                            if (gainCurrency.ContainsKey(_type)) gainCurrency[_type] += data.Value;
                            else gainCurrency.Add(_type, data.Value);
                        }
                        break;
                    case ItemType.Supply:
                        var supplyData = SupplyData.GetData((int)data.Value);
                        supplies.Add(supplyData);
                        break;
                    default:
                        WriteLog.Log("尚未實作獲得物品: " + data.Type);
                        break;
                }
            }

            //獲得資源
            foreach (var key in gainCurrency.Keys) {
                GainCurrency(key, gainCurrency[key]);
            }

            //獲得道具
            GainSupply(supplies);


        }

        /// <summary>
        /// 獲得資源
        /// </summary>
        public void GainCurrency(Currency _type, long _value) {
            Data.AddCurrency(_type, _value);
        }

        /// <summary>
        /// 獲得道具
        /// </summary>
        public void GainSupply(List<SupplyData> _datas) {

            List<Dictionary<string, object>> supplyListDic = new List<Dictionary<string, object>>();
            for (int i = 0; i < _datas.Count; i++) {
                var jsonDic = SupplyData.GetJsonDataDic(_datas[i]);
                supplyListDic.Add(jsonDic);
            }
            AddOwnedDatas<OwnedSupplyData>(ColEnum.Supply, supplyListDic);
            AdventureManager.PRole?.AddSupplyExtendAttribute(_datas);//獲得道具增加生命/新智最大值的時候也會同時增加目前值
            AdventureManager.PRole?.GainSupplyPassiveEffects(_datas);//獲得道具狀態
        }

        /// <summary>
        /// 商城購買，花費遊戲中貨幣的購買跑這裡，購買失敗_cb會回傳null
        /// </summary>
        public void ShopBuy(string _shopUID, BuyCount _buyCount, Action<object> _cb = null) {

            ShopData shopData = GameData.GetShopData(_shopUID);
            //檢查是否能資源足夠
            Currency[] currencies = (Currency[])Enum.GetValues(typeof(Currency));
            foreach (var currency in currencies) {
                long needValue = shopData.GetPrice(currency, _buyCount);
                long ownedValue = Data.GetCurrency(currency);
                if (needValue > ownedValue) {
                    PopupUI.ShowClickCancel(string.Format(StringData.GetUIString("NotEnoughCurrency"), StringData.GetUIString(currency.ToString())), () => {
                        _cb?.Invoke(null);
                        TriggerCurrencyNotEnoughEvent(currency);//觸發資源不足事件
                    });
                    return;
                }
            }
            //檢查購買次數
            if (shopData.MyBuyLimitType != BuyLimitType.None && shopData.BuyLimit > 0) {
                //購買次數已達上限會跳提示
                var history = MyHistoryData;
                if (history != null) {
                    if (shopData.BuyLimit <= history.GetLimitShopBuyCount(shopData.MyBuyLimitType, _shopUID)) {
                        string tipStr = "";
                        switch (shopData.MyBuyLimitType) {
                            case BuyLimitType.Daily:
                                tipStr = string.Format(StringData.GetUIString("BuyDailyLimit"), shopData.BuyLimit);
                                break;
                            case BuyLimitType.Permanence:
                                tipStr = string.Format(StringData.GetUIString("BuyLimit"), shopData.BuyLimit);
                                break;
                        }
                        PopupUI.ShowClickCancel(tipStr, () => { _cb?.Invoke(null); });
                        return;
                    }
                }
            }

            FirebaseManager.Shop_Buy(_shopUID, _buyCount, dataObj => {
                _cb?.Invoke(dataObj);
                DownloadAndUpdatePlayerOwnedData(ColEnum.Player, () => {//下載並更新玩家Player資料
                    //Debug.LogError("下載並更新玩家Player資料");
                });
                DownloadAndUpdatePlayerOwnedData(ColEnum.History, () => {//下載並更新玩家History資料
                    //Debug.LogError("下載並更新玩家History資料");
                });
            });
        }




        public event Action<object> OnPurchaseItemCallBack;
        /// <summary>
        /// 儲值購買，IAP跑這裡
        /// </summary>
        public void Purchase(string _purchaseUID, Action<object> _cb = null) {
            PurchaseData purchaseData = GameData.GetPurchaseData(_purchaseUID);
            //檢查購買次數
            if (purchaseData.MyBuyLimitType != BuyLimitType.None && purchaseData.BuyLimit > 0) {
                //購買次數已達上限會跳提示
                var history = MyHistoryData;
                if (history != null) {
                    if (purchaseData.BuyLimit <= history.GetLimitPurchaseBuyCount(purchaseData.MyBuyLimitType, _purchaseUID)) {
                        string tipStr = "";
                        switch (purchaseData.MyBuyLimitType) {
                            case BuyLimitType.Daily:
                                tipStr = string.Format(StringData.GetUIString("BuyDailyLimit"), purchaseData.BuyLimit);
                                break;
                            case BuyLimitType.Permanence:
                                tipStr = string.Format(StringData.GetUIString("BuyLimit"), purchaseData.BuyLimit);
                                break;
                        }
                        PopupUI.ShowClickCancel(tipStr, null);
                        return;
                    }
                }
            }



            WriteLog.Log($"IAPManager Start PurchaseItem ID={purchaseData.UID}");
            OnPurchaseItemCallBack = _cb;
#if UNITY_IAP
            PopupUI.ShowLoading(StringData.GetUIString("Loading"), 60);
            if (!IAPManager.Inst.PurchaseItem(purchaseData.ProductUID, purchaseData.UID, OnPurchaseSuccess, OnPurchaseFail)) {
                // 購買商品時 未進到IAP流程的其他失敗狀況 關閉Loading
                PopupUI.HideLoading();
            }
#else
                    FirebaseManager.Purchase(purchaseData.UID, null, obj => {
                        _cb?.Invoke(obj);
                    });
#endif
        }

        /// <summary>
        /// IAP購買成功
        /// </summary>
        /// <param name="productUID">平台商品UID</param>
        /// <param name="shopUID">商城商品UID</param>
        /// <param name="receipt">商品訂單資訊</param>
        /// <param name="receiptString">訂單內容</param>
        /// <param name="successCallBack">完成驗證後的回呼要通知IAPManager商品已經驗證成功可以完成購買這個項目</param>
        private void OnPurchaseSuccess(string productUID, string shopUID, IPurchaseReceipt receipt, string receiptString, Action<string> successCallBack) {
            WriteLog.Log($"購買商品訂單成立 商店商品Id={shopUID}, 平台商品Id={productUID} 準備驗證");
            FirebaseManager.Purchase(shopUID, receiptString, dataObj => {
                OnPurchaseItemCallBack?.Invoke(dataObj);
                successCallBack?.Invoke(productUID);
                PopupUI.HideLoading();
                DownloadAndUpdatePlayerOwnedData(ColEnum.Player, () => {//下載並更新玩家Player資料
                });
                DownloadAndUpdatePlayerOwnedData(ColEnum.History, () => {//下載並更新玩家History資料
                });
            });
        }

        /// <summary>
        /// IAP購買失敗
        /// </summary>
        /// <param name="shopUID">失敗的商城商品訂單Id</param>
        private void OnPurchaseFail(string shopUID) {
            WriteLog.Log("購買商品訂單失敗 productID=" + shopUID);
            OnPurchaseItemCallBack?.Invoke(null);
            PopupUI.HideLoading();
        }




        /// <summary>
        /// 觀看次數+1(本機)
        /// </summary>
        public void AddWatchADTimes() {
            var history = MyHistoryData;
            if (history != null)
                MyHistoryData.WatchADTimes++;
            else {//本來沒有歷史資料就創新的歷史資料
                Dictionary<string, object> data = new Dictionary<string, object>();
                data.Add("UID", Data.UID);
                data.Add("WatchADTimes", 1);
                SetOwnedData<OwnedHistoryData>(ColEnum.History, data);
            }
        }


        /// <summary>
        /// 看廣告呼叫這裡
        /// ※小遊戲的加倍獎勵廣告不要呼叫這裡，因為這裡會考慮每日上限次數
        /// </summary>
        public void WatchAD() {
            float cd = FirestoreGameSetting.GetFloatData(GameDataDocEnum.ADReward, "CD");
            float remainCDsecs = CDChecker.GetRemainSecs("WatchADCD", cd);
            if (remainCDsecs > 0) {
                PopupUI.ShowClickCancel(string.Format(StringData.GetUIString("ADCD"), Mathf.CeilToInt(remainCDsecs)), null);
                return;
            }


#if GOOGLE_ADS

#if UNITY_EDITOR
            var history = GamePlayer.Instance.MyHistoryData;
            if (history != null) {
                if (history.WatchADTimes < FirestoreGameSetting.GetIntData(GameDataDocEnum.ADReward, "DailyLimitTimes")) {
                    //編輯器直接跑結果
                    GamePlayer.Instance.AddWatchADTimes();//本機觀看次數先+1
                    CDChecker.DoneCD("WatchADCD", cd);//看廣告進CD
                    //LobbyRightUI.GetInstance<LobbyRightUI>()?.SetADCDTimer();//看廣告後按鈕會顯示CD時間
                    //MiniGameUI.GetInstance<MiniGameUI>()?.SetADCDTimer();//看廣告後按鈕會顯示CD時間
                    PopupUI.ShowLoading(StringData.GetUIString("Loading"));
                    //FirebaseManager.DailyReward_GetWatchADReward(dataObj => {
                    //    PopupUI.HideLoading();
                    //    var returnItemDic = DataHandler.ConvertDataObjToReturnItemDic(dataObj);
                    //    PopupUI.ShowGetItems(returnItemDic["ReturnGainItems"], returnItemDic["ReplaceGainItems"], false);
                    //    // 任務更新-看廣告幾次
                    //    FirebaseManager.Quest_FinishProgress(QuestTaskType.WatchAD.ToString(), "", 1, null);
                    //});
                } else
                    PopupUI.ShowClickCancel(StringData.GetUIString("MiniGame_WatchADLimit"), null);
            } else {
                //編輯器直接跑結果
                GamePlayer.Instance.AddWatchADTimes();//本機觀看次數先+1
                CDChecker.DoneCD("WatchADCD", cd);//看廣告進CD
                //LobbyRightUI.GetInstance<LobbyRightUI>()?.SetADCDTimer();//看廣告後按鈕會顯示CD時間
                //MiniGameUI.GetInstance<MiniGameUI>()?.SetADCDTimer();//看廣告後按鈕會顯示CD時間
                //FirebaseManager.DailyReward_GetWatchADReward(dataObj => {
                //    var returnItemDic = DataHandler.ConvertDataObjToReturnItemDic(dataObj);
                //    PopupUI.ShowGetItems(returnItemDic["ReturnGainItems"], returnItemDic["ReplaceGainItems"], false);
                //    // 任務更新-看廣告幾次
                //    FirebaseManager.Quest_FinishProgress(QuestTaskType.WatchAD.ToString(), "", 1, null);
                //});
            }

#else
            // 暫時先關閉音樂
            if (LobbyManager.Instance != null) {
                LobbyManager.Instance.MuteLobbyMusic(true);
            }
            PopupUI.ShowLoading(StringData.GetUIString("Loading"));
            GoogleAdsManager.Inst.ShowGoogleAds("TVAdReward", (isWatchSuccess, result) => {
                PopupUI.HideLoading();
                if (isWatchSuccess) {
                    var history = GamePlayer.Instance.MyHistoryData;
                    if (history != null) {
                        if (history.WatchADTimes < FirestoreGameSetting.GetIntData(GameDataDocEnum.ADReward, "DailyLimitTimes")) {
                            GamePlayer.Instance.AddWatchADTimes();//本機觀看次數先+1
                            CDChecker.DoneCD("WatchADCD", cd);//看廣告進CD
                            LobbyRightUI.GetInstance<LobbyRightUI>()?.SetADCDTimer();//看廣告後按鈕會顯示CD時間
                            MiniGameUI.GetInstance<MiniGameUI>()?.SetADCDTimer();//看廣告後按鈕會顯示CD時間
                            PopupUI.ShowLoading(StringData.GetUIString("Loading"));
                            FirebaseManager.DailyReward_GetWatchADReward(dataObj => {
                                PopupUI.HideLoading();
                                var returnItemDic = DataHandler.ConvertDataObjToReturnItemDic(dataObj);
                                PopupUI.ShowGetItems(returnItemDic["ReturnGainItems"], returnItemDic["ReplaceGainItems"], false);
                            });
                        } else {//廣告達每日限定次數就不給獎勵 也不用提醒玩家

                        }
                    } else {
                        GamePlayer.Instance.AddWatchADTimes();//本機觀看次數先+1
                        CDChecker.DoneCD("WatchADCD", cd);//看廣告進CD
                        LobbyRightUI.GetInstance<LobbyRightUI>()?.SetADCDTimer();//看廣告後按鈕會顯示CD時間
                        MiniGameUI.GetInstance<MiniGameUI>()?.SetADCDTimer();//看廣告後按鈕會顯示CD時間
                        PopupUI.ShowLoading(StringData.GetUIString("Loading"));
                        FirebaseManager.DailyReward_GetWatchADReward(dataObj => {
                            PopupUI.HideLoading();
                            var returnItemDic = DataHandler.ConvertDataObjToReturnItemDic(dataObj);
                            PopupUI.ShowGetItems(returnItemDic["ReturnGainItems"], returnItemDic["ReplaceGainItems"], false);
                        });
                    }
                }
                DebugLogger.Log($"isWatchSuccess={isWatchSuccess}, Watch Ad Success or Fail Result={result.ToString()}");
            });
#endif

#endif
        }


        public void TriggerCurrencyNotEnoughEvent(Currency _currency) {
            //資源不夠跳事件
            switch (_currency) {
                case Currency.Gold:
                    TriggerEventHandler.TriggerEventCheck(TriggerEventData.TriggerTiming.NotEnoughGold);
                    break;
                case Currency.Point:
                    TriggerEventHandler.TriggerEventCheck(TriggerEventData.TriggerTiming.NotEnoughPoint);
                    break;
            }
        }


    }
}