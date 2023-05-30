using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Storage;
using Firebase.Firestore;
using Firebase.Extensions;
using Scoz.Func;
using System;
using System.IO;
using System.Globalization;

namespace TheDoor.Main {
    public partial class FirebaseManager : MonoBehaviour {

        public static FirebaseApp MyFirebaseApp {
            get {
#if UNITY_EDITOR
                return FirebaseApp.GetInstance(GameManager.CurVersion.ToString());
#else
                return FirebaseApp.DefaultInstance;
#endif
            }
        }
        static FirebaseAuth MyAuth { get { return FirebaseAuth.GetAuth(MyFirebaseApp); } }
        public static FirebaseUser MyUser { get { return MyAuth.CurrentUser; } }
        public static string LanguageCode { get { return MyAuth.LanguageCode; } }
        public static FirebaseStorage Storage { get { return FirebaseStorage.GetInstance(MyFirebaseApp); } }
        static bool IsInit = false;
        private const string Region = "asia-east1";
        static FirebaseFirestore Store { get { return FirebaseFirestore.GetInstance(MyFirebaseApp); } }

        private void Update() {
            AppleRun();
        }

        //三方登入，若有綁定三方平台會加到這個清單，若此清單為空代表沒有綁定任何三方平台
        static HashSet<ThirdPartLink> ThirdPartLinks = new HashSet<ThirdPartLink>();
        /// <summary>
        /// 是否已經綁定任何三方登入
        /// </summary>
        public static bool IsLinkingAnyThirdPart {
            get {
                UpdateAccountType();
                return ThirdPartLinks.Count > 0;
            }
        }
        /// <summary>
        /// 是否綁定該平台中
        /// </summary>
        public static bool IsLinkingThrdPart(ThirdPartLink _type) {
            UpdateAccountType();
            return ThirdPartLinks.Contains(_type);
        }
        /// <summary>
        /// 更新玩家正使用三方登入的清單
        /// </summary>
        static void UpdateAccountType() {
            ThirdPartLinks.Clear();
            List<Firebase.Auth.IUserInfo> providerData = FirebaseManager.MyUser.ProviderData as List<Firebase.Auth.IUserInfo>;
            for (int i = 0; i < providerData.Count; i++) {
                if (providerData[i].ProviderId.Contains("facebook")) {
                    if (!ThirdPartLinks.Contains(ThirdPartLink.Facebook))
                        ThirdPartLinks.Add(ThirdPartLink.Facebook);
                }
                if (providerData[i].ProviderId.Contains("google")) {
                    if (!ThirdPartLinks.Contains(ThirdPartLink.Google))
                        ThirdPartLinks.Add(ThirdPartLink.Google);
                }
                if (providerData[i].ProviderId.Contains("apple")) {
                    if (!ThirdPartLinks.Contains(ThirdPartLink.Apple))
                        ThirdPartLinks.Add(ThirdPartLink.Apple);
                }
                /*
                Debug.Log(providerData[i].ProviderId);
                Debug.Log(providerData[i].UserId);
                Debug.Log(providerData[i].PhotoUrl);
                */
            }
            //Debug.LogError(test);
        }

        /// <summary>
        /// 集合字典，有新增Firestore集合名稱要放這裡
        /// </summary>
        public static Dictionary<ColEnum, string> ColNames { get; private set; } = new Dictionary<ColEnum, string>() {
            { ColEnum.GameSetting , "GameData-Setting"},
            { ColEnum.Player , "PlayerData-Player"},
            { ColEnum.Role , "PlayerData-Role"},
            { ColEnum.Supply , "PlayerData-Supply"},
            { ColEnum.Adventure , "PlayerData-Adventure"},
            { ColEnum.Item , "PlayerData-Item"},
            { ColEnum.Shop , "GameData-Shop"},
            { ColEnum.Purchase , "GameData-Purchase"},
            { ColEnum.History , "PlayerData-History"},


            //※有新增類型也要考慮PerPlayerOwnedOneDocCols是否要新增
        };
        /// <summary>
        /// 定義那些資料庫集合是一個玩家只會擁有一個doc類型的偵聽跑這裡(例如PlayerData-Player，PlayerData-Player集合中玩家只會擁有一個 Doc)
        /// </summary>
        public static HashSet<string> PerPlayerOwnedOneDocCols = new HashSet<string>{
        "Player",//玩家資料
        "Item",//道具
        "History",//紀錄
        };
        /// <summary>
        /// 如果玩家之前已經註冊過就會初始化Firestore讀取進度，讀取完會執行傳入的Callback Action
        /// 如果玩家還沒註冊過就會直接執行傳入的Callack Action
        /// </summary>
        static LoadingProgress MyLoadingProgress;

        public static void Init(Action<bool> _action) {
            if (IsInit) {
                _action?.Invoke(true);
                return;
            }
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
                var dependencyStatus = task.Result;
                WriteLog.Log("<color=#9b791d>[Firebase]DependencyStatus: " + dependencyStatus + "</color>");
                if (dependencyStatus == DependencyStatus.Available) {
#if UNITY_EDITOR
                    InitFirebaseAppByJson();//讀取google-services.json來設定Firebase專案
#endif
                    if (Debugger.Instance != null)//將目前版本顯示在除錯UI上
                        Debugger.Instance.UpdateFirebaseEnvVersionText();
                    WriteLog.LogFormat("<color=#ff833f>[Firebase] <<<<<<<<<<<<<<<<專案ID: {0}>>>>>>>>>>>>>>>> </color>", MyFirebaseApp.Options.ProjectId);
                    DeviceManager.AddOnApplicationQuitAction(StopAllListener);
                    _action?.Invoke(true);
                    IsInit = true;
                } else {
                    _action?.Invoke(false);
                    PopupUI_Local.ShowClickCancel("Could not resolve all Firebase dependencies", () => { Application.Quit(); });
                    WriteLog.LogErrorFormat("<color=#9b791d>[Firebase]Could not resolve all Firebase dependencies: {0}", dependencyStatus + "</color>");
                    // Firebase Unity SDK is not safe to use here.
                }
            });

#if UNITY_EDITOR || UNITY_STANDALONE
            FirebaseFirestore.DefaultInstance.Settings.PersistenceEnabled = false;
#endif
#if UNITY_IPHONE
            InitFacebook();//初始化Facebook
#elif UNITY_ANDROID
            InitFacebook();//初始化Facebook
#endif

        }

        static void InitFirebaseAppByJson() {
            //讀取google-services.json來設定Firebase專案
            var path = Application.streamingAssetsPath + "/google-services.json";
            var jsonText = File.ReadAllText(path);
            AppOptions appOptions = AppOptions.LoadFromJsonConfig(jsonText);
            var app = FirebaseApp.Create(appOptions, GameManager.CurVersion.ToString());
        }

        /// <summary>
        /// 玩家進遊戲要載入的Firestore資料要寫在這裡，有新加要載的東西也要記的加MyLoadingProgress清單
        /// </summary>
        public static void LoadDatas(Action _ac) {
            GameData.OnFirebaseInit();
            MyLoadingProgress = new LoadingProgress(_ac);

            //進遊戲載入完才能進大廳的資料放這裡
            MyLoadingProgress.AddLoadingProgress(
                ColEnum.Player.ToString()
                );
            WriteLog.Log("<color=#9b791d>[Firebase] 登入方式為[" + MyUser.ProviderId + "] 開始載入Firebase資料</color>");
            //更新玩家正使用三方登入的清單
            UpdateAccountType();
            //取Server時間
            GetNewServerTime();//跟FirebaseServer要時間(遊戲開始時會要一次以防玩家本機時區不同，時間會錯亂)
            //取消偵聽玩家個人資料
            StopListenPlayerOwnedDatas();
            //玩家個資(單筆)
            //GetDataByDocID(ColEnum.Player, MyUser.UserId, SetFinishedLoadData);//取得玩家資料

            //玩家個資(多筆)
            //GetPersonalDatas(ColEnum.Mail, SetFinishedLoadDatas);//取得信件


            //註冊偵聽
            //RegisterListener_Shop();//註冊偵聽-商城
            //RegisterListener_Purchase();//註冊偵聽-儲值商城
            //RegisterListener_GameSetting();//註冊偵聽-遊戲設定
        }
        public static void GetNewServerTime()//跟FirebaseServer要時間(遊戲開始時會要一次以防玩家本機時區不同，時間會錯亂)
        {
            GetServerTime(time => {
                GameManager.Instance.SetTime(time);
            });
        }

        /// <summary>
        /// 取Firebase單筆資料回傳
        /// </summary>
        static void SetFinishedLoadData(ColEnum _colName, Dictionary<string, object> _data) {
            WriteLog.LogFormat("<color=#9b791d>[Firebase] {0} 讀取完成</color>", _colName);
            try {
                switch (_colName) {
                    case ColEnum.Player:
                        GamePlayer.Instance.SetMainPlayerData(_data);
                        //第一次從Firebase上取得玩家資料後執行以下Funcs
                        FirebaseManager.PlayerSign_SignIn();//送玩家登入LOG
                        GameTimer.Instance.StartDoFirstAction();//執行計時器第一次要執行的東西
                        DeviceManager.AddOnFocusAction(PlayerSign_UpdateOnlineTimestamp);//App背景作業回來後也會更新上線時間戳

                        //登入後會先存裝置UID到DB，存好後AlreadSetDeviceUID會設為true，所以之後從DB取到的裝置的UID應該都跟目前的裝置一致，若不一致代表是有其他裝置登入同個帳號
                        FirebaseManager.PlayerSign_SetDevice(() => { GamePlayer.Instance.AlreadSetDeviceUID = true; });//設定裝置UID

                        //註冊偵聽
                        RegisterListener_OwnedData(ColEnum.Player, GamePlayer.Instance.Data.UID);//註冊偵聽-玩家資料
                        RegisterListener_OwnedData(ColEnum.Item, GamePlayer.Instance.Data.UID);//註冊偵聽-玩家非獨立資料類道具
                        RegisterListener_OwnedData(ColEnum.History, GamePlayer.Instance.Data.UID);//註冊偵聽-玩家紀錄
                        RegisterListener_OwnedData(ColEnum.Role, GamePlayer.Instance.Data.UID);//註冊偵聽-玩家腳色
                        RegisterListener_OwnedData(ColEnum.Supply, GamePlayer.Instance.Data.UID);//註冊偵聽-腳色道具
                        RegisterListener_OwnedData(ColEnum.Adventure, GamePlayer.Instance.Data.UID);//註冊偵聽-腳色冒險
                        InitNotification();//初始化Firebase推播

                        break;
                }
                MyLoadingProgress.FinishProgress(_colName.ToString());
            } catch (Exception _e) {
                WriteLog.LogError(_e);
            }
        }

        /// <summary>
        /// 取Firebase多筆資料回傳
        /// </summary>
        static void SetFinishedLoadDatas(ColEnum _colName, List<Dictionary<string, object>> _datas) {
            WriteLog.LogFormat("<color=#9b791d>[Firebase] {0} 讀取完成</color>", _colName);
            try {

                MyLoadingProgress.FinishProgress(_colName.ToString());
            } catch (Exception _e) {
                WriteLog.LogError(_e);
            }
        }
        /// <summary>
        /// 傳入Firebase Timestamp返回UTC+8 DateTime
        /// </summary>
        public static DateTime GetDateTimeFromFirebaseTimestamp(object _obj, double addHour = 8) {
            try {
                if (_obj is DateTime)
                    return (DateTime)_obj;
                else
                    return ((Timestamp)_obj).ToDateTime().AddHours(addHour);
            } catch {
                try {
                    DateTime dateTime = DateTime.Parse(_obj.ToString(), null, DateTimeStyles.RoundtripKind);
                    return dateTime;
                } catch {
                    WriteLog.LogError("Firebase時間戳轉換失敗");
                    return new DateTime();
                }
            }
        }

        public static IEnumerator Logout(Action _cb) {
            if (MyUser == null) {
                WriteLog.Log("尚未登入所以不需要登出");
            } else {
                new GamePlayer();//產生一個新玩家
                //PlayerPrefs.DeleteAll();//刪除Firebase也一併清除本機資料
                MyAuth.SignOut();
            }
            yield return null;
            _cb?.Invoke();
        }
    }
}