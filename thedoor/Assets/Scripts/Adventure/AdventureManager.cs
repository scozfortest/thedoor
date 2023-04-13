using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Scoz.Func;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using UnityEngine.Purchasing.Security;

namespace TheDoor.Main {
    public class AdventureManager : MonoBehaviour {
        [HeaderAttribute("==============Addressable Assets==============")]
        public Canvas MyCanvas;
        public AssetReference AdventureUIAsset;

        [HeaderAttribute("==============設定==============")]
        LoadingProgress MyLoadingProgress;//讀取進度，讀取完會跑FinishInitAdventure()
        List<AsyncOperationHandle> HandleList = new List<AsyncOperationHandle>();

        public static AdventureManager Instance { get; private set; }

        void Start() {
            Instance = this;
            if (GameManager.IsInit) {
                InitAdventure();
            } else {
                //建立遊戲管理者
                GameManager.CreateNewInstance();
                FirebaseManager.Init(success => {
                    if (success) {
                        if (FirebaseManager.MyUser == null)//還沒在StartScene註冊帳戶就直接從其他Scene登入會報錯誤(通常還沒註冊帳戶就不會有玩家資料直接進遊戲會有問題)
                            DebugLogger.LogError("尚未註冊Firebase帳戶");

                        //讀取Firestore資料
                        FirebaseManager.LoadDatas(() => {
                            //載資源包
                            GameManager.StartDownloadAddressable(() => {
                                InitAdventure();
                            });
                        });
                    }
                });
            }
        }


        /// <summary>
        /// 冒險初始化
        /// </summary>
        public void InitAdventure() {
            MyLoadingProgress = new LoadingProgress(AdventureUILoaded);
            SpawnAddressableAssets();
        }
        /// <summary>
        /// 冒險初始化完成時執行
        /// </summary>
        public void AdventureUILoaded() {
            PopupUI.FinishTransitionProgress("AdventureUILoaded");

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
            MyLoadingProgress.AddLoadingProgress("AdventureUI");//新增讀取中項目

            DateTime now = DateTime.Now;
            //初始化UI
            Addressables.LoadAssetAsync<GameObject>(AdventureUIAsset).Completed += handle => {
                DebugLogger.LogFormat("載入AdventureUI花費: {0}秒", (DateTime.Now - now).TotalSeconds);
                HandleList.Add(handle);
                GameObject go = Instantiate(handle.Result);
                go.transform.SetParent(MyCanvas.transform);
                go.transform.localPosition = handle.Result.transform.localPosition;
                go.transform.localScale = handle.Result.transform.localScale;
                RectTransform rect = go.GetComponent<RectTransform>();
                rect.offsetMin = Vector2.zero;//Left、Bottom
                rect.offsetMax = Vector2.zero;//Right、Top
                go.GetComponent<AdventureUI>().Init();
                MyLoadingProgress.FinishProgress("AdventureUI");//完成讀取UI
            };
        }
    }
}