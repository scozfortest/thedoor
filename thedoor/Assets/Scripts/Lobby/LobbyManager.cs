using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Scoz.Func;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using UnityEngine.Purchasing.Security;

namespace TheDoor.Main {
    public class LobbyManager : MonoBehaviour {
        [HeaderAttribute("==============Addressable Assets==============")]
        public Canvas MyCanvas;
        public AssetReference LobbyUIAsset;

        [HeaderAttribute("==============設定==============")]
        LoadingProgress MyLoadingProgress;//讀取進度，讀取完會跑FinishInitLobby()
        List<AsyncOperationHandle> HandleList = new List<AsyncOperationHandle>();

        public static LobbyManager Instance { get; private set; }

        void Start() {
            Instance = this;
            if (GameManager.IsInit) {
                InitLobby();
            } else {
                //建立遊戲管理者
                GameManager.CreateNewInstance();
                FirebaseManager.Init(success => {
                    if (success) {
                        if (FirebaseManager.MyUser == null)//還沒在StartScene註冊帳戶就直接從其他Scene登入會報錯誤(通常還沒註冊帳戶就不會有玩家資料直接進遊戲會有問題)
                            WriteLog.LogError("尚未註冊Firebase帳戶");

                        //讀取Firestore資料
                        FirebaseManager.LoadDatas(() => {
                            //載資源包
                            GameManager.StartDownloadAddressable(() => {
                                InitLobby();
                            });
                        });
                    }
                });
            }
        }


        /// <summary>
        /// 大廳初始化
        /// </summary>
        public void InitLobby() {
            MyLoadingProgress = new LoadingProgress(LobbyUILoaded);
            SpawnAddressableAssets();
        }
        /// <summary>
        /// 大廳初始化完成時執行
        /// </summary>
        public void LobbyUILoaded() {
            PopupUI.FinishSceneTransitionProgress("LobbyUILoaded");
            //建立冒險用腳色資料
            if (GamePlayer.Instance.Data.CurRole != null)
                AdventureManager.CreatePlayerRole();

#if GOOGLE_ADS
            // 初始化Google Ads
            GoogleAdsManager.Inst.Initialize();
#endif

        }
        private void OnDestroy() {
            Instance = null;
            for (int i = 0; i < HandleList.Count; i++) {
                if (HandleList[i].IsValid())
                    Addressables.Release(HandleList[i]);
            }
        }
        void SpawnAddressableAssets() {
            MyLoadingProgress.AddLoadingProgress("LobbyUI");//新增讀取中項目

            DateTime now = DateTime.Now;
            //初始化UI
            Addressables.LoadAssetAsync<GameObject>(LobbyUIAsset).Completed += handle => {
                WriteLog.LogFormat("載入LobbyUI花費: {0}秒", (DateTime.Now - now).TotalSeconds);
                HandleList.Add(handle);
                GameObject go = Instantiate(handle.Result);
                go.transform.SetParent(MyCanvas.transform);
                go.transform.localPosition = handle.Result.transform.localPosition;
                go.transform.localScale = handle.Result.transform.localScale;
                RectTransform rect = go.GetComponent<RectTransform>();
                rect.offsetMin = Vector2.zero;//Left、Bottom
                rect.offsetMax = Vector2.zero;//Right、Top
                go.GetComponent<LobbyUI>().Init();
                MyLoadingProgress.FinishProgress("LobbyUI");//完成讀取UI
            };
        }

        #region IAP驗證 
        // 在初始化IAP時 先前沒有驗證完成的訂單會需要再度驗證 所以會在初始化加入驗證的CallBack
        /// <summary>
        /// IAP購買成功
        /// </summary>
        /// <param name="productUID">平台商品ID</param>
        /// <param name="shopUID">商城商品ID</param>
        /// <param name="receipt">商品訂單資訊</param>
        /// <param name="receiptString">訂單內容</param>
        /// <param name="successCallBack">完成驗證後的回呼要通知IAPManager商品已經驗證成功可以完成購買這個項目</param>
        private void OnPurchaseSuccess(string productUID, string shopUID, IPurchaseReceipt receipt, string receiptString, Action<string> successCallBack) {
            WriteLog.Log($"購買商品訂單成立 商城商品Id={shopUID}, 平台商品ID={productUID} 準備驗證");
            FirebaseManager.Purchase(shopUID, receiptString, dataObj => {
                if (dataObj.ToString() == "used token") {
                    successCallBack?.Invoke(productUID);
                } else {
                    var returnItemDic = DataHandler.ConvertDataObjToReturnItemDic(dataObj);
                    //PopupUI.ShowGetItems(returnItemDic["ReturnGainItems"], returnItemDic["ReplaceGainItems"], false);
                    successCallBack?.Invoke(productUID);
                }
            });

        }

        /// <summary>
        /// IAP購買失敗
        /// </summary>
        /// <param name="productID">失敗的訂單Id</param>
        private void OnPurchaseFail(string productID) {
            WriteLog.Log("購買商品訂單失敗 productID=" + productID);
        }
        #endregion
    }
}