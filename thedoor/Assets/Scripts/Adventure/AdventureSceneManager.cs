using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Scoz.Func;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using UnityEngine.Purchasing.Security;
using Cinemachine;

namespace TheDoor.Main {
    public class AdventureSceneManager : MonoBehaviour {


        [SerializeField] AdventureManager AdvManager;
        [HeaderAttribute("==============Addressable Assets==============")]
        public Canvas HeightBaseCanvas;
        public Canvas WidthBaseCanvas;
        public AssetReference AdventureUIAsset;
        public AssetReference DoorNodeUIAsset;


        [HeaderAttribute("==============設定==============")]
        [SerializeField] Camera MainCam;
        [SerializeField] CinemachineVirtualCamera MyCam;


        LoadingProgress MyLoadingProgress;//讀取進度，讀取完會跑FinishInitAdventure()
        List<AsyncOperationHandle> HandleList = new List<AsyncOperationHandle>();


        public static AdventureSceneManager Instance { get; private set; }

        void Start() {
            Instance = this;
            CameraManager.SetCam(MainCam.GetComponent<CinemachineBrain>());
            CameraManager.AddVirtualCam(CameraManager.CamNames.Adventure, MyCam);
            if (GameManager.IsInit) {
                InitAdventure();
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
            PopupUI.FinishSceneTransitionProgress("AdventureUILoaded");
            AdventureUI.Instance.MyDoorNodeUI = DoorNodeUI.Instance;
            AdventureUI.Instance.SwitchUI(AdventureUIs.Default);
            AdventureManager.GoNextDoor();

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

            DateTime now = DateTime.Now;

            //AdventureUI
            MyLoadingProgress.AddLoadingProgress("AdventureUI");//新增讀取中項目
            Addressables.LoadAssetAsync<GameObject>(AdventureUIAsset).Completed += handle => {
                WriteLog.LogFormat("載入AdventureUI花費: {0}秒", (DateTime.Now - now).TotalSeconds);
                HandleList.Add(handle);
                GameObject go = Instantiate(handle.Result);
                go.transform.SetParent(HeightBaseCanvas.transform);
                go.transform.localPosition = handle.Result.transform.localPosition;
                go.transform.localScale = handle.Result.transform.localScale;
                RectTransform rect = go.GetComponent<RectTransform>();
                rect.offsetMin = Vector2.zero;//Left、Bottom
                rect.offsetMax = Vector2.zero;//Right、Top
                go.GetComponent<AdventureUI>().Init();
                MyLoadingProgress.FinishProgress("AdventureUI");//完成讀取UI
            };

            //DoorNodeUI
            MyLoadingProgress.AddLoadingProgress("DoorNodeUI");//新增讀取中項目
            Addressables.LoadAssetAsync<GameObject>(DoorNodeUIAsset).Completed += handle => {
                WriteLog.LogFormat("載入AdventureUI花費: {0}秒", (DateTime.Now - now).TotalSeconds);
                HandleList.Add(handle);
                GameObject go = Instantiate(handle.Result);
                go.transform.SetParent(WidthBaseCanvas.transform);
                go.transform.localPosition = handle.Result.transform.localPosition;
                go.transform.localScale = handle.Result.transform.localScale;
                RectTransform rect = go.GetComponent<RectTransform>();
                rect.offsetMin = Vector2.zero;//Left、Bottom
                rect.offsetMax = Vector2.zero;//Right、Top
                go.GetComponent<DoorNodeUI>().Init();
                go.GetComponent<DoorNodeUI>().LoadItemAsset(() => {
                    MyLoadingProgress.FinishProgress("DoorNodeUI");//完成讀取UI
                });
            };
        }
    }
}