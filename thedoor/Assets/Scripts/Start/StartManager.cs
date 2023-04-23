using Scoz.Func;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using Firebase.Analytics;

namespace TheDoor.Main {
    public class StartManager : MonoBehaviour {
        public Canvas MyCanvas;
        public StartUI MyStartUI;
        [SerializeField] Text VersionText;
        [SerializeField] AudioClip StartMusic;

        public static StartManager Instance;
        List<AsyncOperationHandle> HandleList = new List<AsyncOperationHandle>();
        /// <summary>
        /// 是否第一次執行遊戲，第一次執行遊戲後會自動進大廳，之後透過從大廳的設定中點回到主介面就不會又自動進大廳了
        /// </summary>
        public static bool FirstTimeLaunchGame { get; private set; } = true;

        private void Start() {
            SetVersionText();//顯示下方文字
            MyStartUI.Init();
            Instance = this;
            //建立遊戲管理者
            GameManager.CreateNewInstance();
            //播放背景音樂主題曲，要在GameManager.CreateNewInstance()之後，因為AudioPlayer是在這之後才被初始化
            PlayMainMusic();
            //檢查網路
            PopupUI_Local.ShowLoading("Checking Internet");
            InternetChecker.SetOnConnectedAction(OnConnected);
            InternetChecker.StartCheckInternet();
        }
        public void SetVersionText() {
            //顯示下方文字
            if (GamePlayer.Instance != null && GamePlayer.Instance.Data != null) {
                VersionText.text = "版本: " + Application.version;
            } else {
                VersionText.text = "版本: " + Application.version;
            }

        }
        public void PlayMainMusic() {
            AudioPlayer.StopAllMusic_static();
            AudioPlayer.PlayAudioByAudioClip(MyAudioType.Music, StartMusic, true);
        }

        //確認有網路後才會執行後續
        void OnConnected() {
            PopupUI_Local.ShowLoading("Init Data");
            //初始化Firebase
            FirebaseManager.Init(success => {
                if (success)
                    OnFirebaseInitFInished();
            });
        }
        /// <summary>
        /// Firebase初始化後觸發
        /// </summary>
        void OnFirebaseInitFInished() {
            PopupUI_Local.HideLoading();
            // 詢問IOS玩家是否要開啟透明追蹤
#if APPSFLYER && UNITY_IOS && !UNITY_EDITOR
             AppsFlyerManager.Inst.IOSAskATTUserTrack();
#endif

            //離線模式
            if (GameManager.OfflineMode) {
                GameManager.Instance.SetTime(DateTime.Now);
                StartDownloadingAssetAndGoNextScene();
                return;
            }


            if (FirebaseManager.MyUser == null) {//玩家尚未登入
                WriteLog.Log("玩家尚未登入");
                MyStartUI.ShowUI(StartUI.Condietion.NotLogin);
            } else {//玩家已經登入，就開始載入Firestore上的資料(LoadDatas)

                //是否第一次執行遊戲，第一次執行遊戲後會自動進大廳，之後透過從大廳的設定中點回到主介面就不會又自動進大廳了
                if (FirstTimeLaunchGame) {
                    FirebaseManager.LoadDatas(() => {
                        WriteLog.LogFormat("玩家 {0} 已經登入", FirebaseManager.MyUser.UserId);
                        StartManager.Instance.SetVersionText();//顯示下方文字
#if APPSFLYER
                        // 設定玩家UID
                        AppsFlyerManager.Inst.SetCustomerUserId(FirebaseManager.MyUser.UserId);
                        // AppsFlyer紀錄玩家登入
                        AppsFlyerManager.Inst.Login(FirebaseManager.MyUser.UserId);
#endif

#if !UNITY_EDITOR && FIREBASE_ANALYTICS
                    FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                    // 設定Firebase UserId
                    FirebaseAnalytics.SetUserId(FirebaseManager.MyUser.UserId);
                    // 記錄登入事件
                    Parameter[] loginParameters = {
                          new Parameter(FirebaseAnalytics.ParameterMethod, FirebaseManager.MyUser.UserId)
                    };
                    FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventLogin, loginParameters);
#endif

                        //如果是編輯器不直接轉場景(正式機才會直接進Lobby)
#if UNITY_EDITOR
                        MyStartUI.ShowUI(StartUI.Condietion.BackFromLobby_ShowLogoutBtn);
#else
                        StartDownloadingAssetAndGoNextScene();
#endif
                    });


                } else {//如果是從大廳點設定回到主介面跑這裡，顯示登出按鈕與返回大廳按鈕
                    MyStartUI.ShowUI(StartUI.Condietion.BackFromLobby_ShowLogoutBtn);
                }



            }
        }


        private void OnDestroy() {
            for (int i = 0; i < HandleList.Count; i++) {
                if (HandleList[i].IsValid())
                    Addressables.Release(HandleList[i]);
            }
            Instance = null;
        }


        /// <summary>
        /// 1. 開始下載資源包
        /// 2. 載完後顯示準備開始遊戲文字
        /// 3. 切至下一個場景
        /// </summary>
        public void StartDownloadingAssetAndGoNextScene() {
            StartUI.GetInstance<StartUI>()?.SetMiddleText(StringData.GetUIString("Login_DownloadAsset"));
            GameManager.StartDownloadAddressable(() => {//下載完資源包後執行

                if (!GameManager.OfflineMode) {
                    /// 根據是否能進行遊戲來執行各種狀態
                    /// 1. 判斷玩家版本，若版本低於最低遊戲版本則會跳強制更新
                    /// 2. 判斷玩家版本，若版本低於目前遊戲版本則會跳更新建議
                    /// 3. 判斷Maintain是否為true，若為true則不在MaintainExemptPlayerUIDs中的玩家都會跳維護中
                    /// 4. 判斷該玩家是否被Ban，不是才能進遊戲
                    GameStateManager.Instance.StartCheckCanPlayGame(() => {
                        FirstTimeLaunchGame = false;
                        PopupUI.InitSceneTransitionProgress(3, "LobbyUILoaded");
                        PopupUI.CallTransition(MyScene.LobbyScene);
                    });
                } else {//離線模式
                    FirstTimeLaunchGame = false;
                    PopupUI.InitSceneTransitionProgress(3, "LobbyUILoaded");
                    PopupUI.CallTransition(MyScene.LobbyScene);
                }

            });
        }


    }
}