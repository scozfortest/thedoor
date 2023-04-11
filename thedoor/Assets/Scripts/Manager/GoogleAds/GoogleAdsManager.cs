#if GOOGLE_ADS

using System;
using UnityEngine;
using TheDoor.Main;
using Scoz.Func;
using GoogleMobileAds.Api;

/// <summary>
/// Google AdMobs Manager
/// </summary>
public class GoogleAdsManager : MonoSingletonA<GoogleAdsManager> {
    protected override bool destroyWhenRepeated { get { return true; } }
    protected override bool dontDestroyOnLoad { get { return true; } }

    /// <summary>
    /// Google Ads展示成功或失敗的回呼
    /// </summary>
    public event Action<bool, AdsResultMessage> OnShowGoogleAds;
#pragma warning disable 0414
    /// <summary>
    /// 是否為GoogleStore
    /// </summary>
    private bool IsGooglePlayStoreSelected;
    /// <summary>
    /// 是否為蘋果的平台    
    /// </summary>
    private bool IsAppleAppStoreSelected;
#pragma warning restore 0414

    /// <summary>
    /// ads是否已經進行初始化
    /// </summary>
    private bool IsInit = false;

    /// <summary>
    /// 初始化是否完成
    /// </summary>
    private bool InitializeSuccess = false;

    /// <summary>
    /// ads是否已經載入下一個廣告
    /// </summary>
    private bool IsLoadNext = false;

    /// <summary>
    /// 是否正要顯示Google Ads
    /// </summary>
    public bool IsShowAds { get; set; }

    /// <summary>
    /// 當前能否顯示Google Ad
    /// </summary>
    public bool GoogleAdReadyToShow { get; set; }

    /// <summary>
    /// 看獎勵影片的廣告獎勵
    /// </summary>
    private RewardedAd RewardedAd;

    /// <summary>
    /// google或Apple的廣告Unique Id
    /// </summary>
    private string AdUnitId;

    /// <summary>
    /// AppsFly值，看廣告時會送，看完廣告後會寫的log也是用這個key
    /// </summary>
    string AppsFlyKey = "Not Defined";

    private void Start() {
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public void Initialize() {

        if (IsInit) {
            return;
        }

        IsInit = true;
        // 設定兩大平台的廣告唯一Id (目前是使用Google的廣告測試Id) 再上架前不能使用自己的 會被鎖帳
        // 平台帳號Id放在Assets\GoogleMobileAds\Resources\GoogleMobileAdsSettings.asset
#if UNITY_ANDROID
#if Test
        AdUnitId = "ca-app-pub-3940256099942544/5224354917";     // Google ads 測試用獎勵影片 google AD   
#elif Release
        AdUnitId = "ca-app-pub-3750846340821495/9644870436";     // MaJampachinko Release版 google Ads Id            
#else
        //AdUnitId = "ca-app-pub-3750846340821495/9644870436";     // MaJampachinko Release版 google Ads Id            
        AdUnitId = "ca-app-pub-3940256099942544/5224354917";    // Google ads 測試用獎勵影片 google AD
#endif

#elif UNITY_IPHONE || UNITY_IOS

#if Test
        AdUnitId = "ca-app-pub-3940256099942544/1712485313";      // Google Ads 測試用獎勵影片 Apple AD
#elif Release
        AdUnitId = "ca-app-pub-3750846340821495/7172449489";      // MaJampachinko Release版 apple Ads Id
#else
        //AdUnitId = "ca-app-pub-3750846340821495/7172449489";      // MaJampachinko Release版 apple Ads Id
        AdUnitId = "ca-app-pub-3940256099942544/1712485313";      // Google Ads 測試用獎勵影片 Apple AD
#endif
        
#else
        AdUnitId = "unexpected_platform";
#endif

        //List<string> deviceIds = new List<string>();
        //deviceIds.Add("B5A5B4898C6A045EF742FB82AB82A43E"); // Hazily 測試機的Id        
        //deviceIds.Add("861ABAA0EC6FA52CC82A14A878FC061B"); // Android 測試機的Id        
        RequestConfiguration requestConfiguration = new RequestConfiguration
            .Builder()
            .SetSameAppKeyEnabled(true) // 利用使用者正在使用的應用程式收集到的資料，放送更相關的個人化廣告。
                                        //.SetTestDeviceIds(deviceIds)       // TODO: 設定測試機的Id(上架前要移除這個)
            .build();

        MobileAds.SetRequestConfiguration(requestConfiguration);

        MobileAds.SetiOSAppPauseOnBackground(true);

        InitializeSuccess = false;
        MobileAds.Initialize(initStatus => {
            IsShowAds = false;
            DebugLogger.Log("[Google AdMob] Initialize Success!");
            InitializeSuccess = true;
            // 先準備一個影片
            CreateAndLoadRewardedAd();
        });
    }

    /// <summary>
    /// 看廣告
    /// </summary>
    /// <param name="_cb">看完廣告的Callback</param>
    public void ShowGoogleAds(string _appsFlyKey, Action<bool, AdsResultMessage> _cb) {
        AppsFlyKey = _appsFlyKey;
        // 尚未初始化
        if (!InitializeSuccess) {
            _cb?.Invoke(false, AdsResultMessage.GoogleAds_Not_Initialize);
            return;
        }

        if (IsShowAds) {
            _cb?.Invoke(false, AdsResultMessage.Ads_AlreadyShowing);
            return;
        }

        OnShowGoogleAds = _cb;
        IsShowAds = true;

        // 沒有預載下一個廣告 重新載一個
        if (!IsLoadNext) {
            CreateAndLoadRewardedAd();
        }
        // 直接顯示可以看的廣告
        else {
            UserChoseToWatchAd();
        }

#if APPSFLYER
        AppsFlyerManager.Inst.WatchAdClick(FirebaseManager.MyUser.UserId, AppsFlyKey);
#endif
    }

    void HandleRewardedAdLoaded(RewardedAd ad, AdFailedToLoadEventArgs error) {
        // Create and pass the SSV options to the rewarded ad.
        var options = new ServerSideVerificationOptions
                              .Builder()
                              .SetCustomData("SAMPLE_CUSTOM_DATA_STRING")
                              .Build();
        ad.SetServerSideVerificationOptions(options);
    }

    /// <summary>
    /// 廣告載入完成時呼叫
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public void HandleRewardedAdLoaded(object sender, EventArgs args) {
        DebugLogger.Log("HandleRewardedAdLoaded event received");

        GoogleAdReadyToShow = true;

        // 廣告載入完成 通知顯示
        if (IsShowAds) {
            UserChoseToWatchAd();
        }
    }


    /// <summary>
    /// 無法載入廣告時呼叫
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public void HandleRewardedAdFailedToLoad(object sender, AdFailedToLoadEventArgs args) {
        DebugLogger.Log($"HandleRewardedAdFailedToLoad event received with message: {args.LoadAdError.GetMessage()}");
        if (IsShowAds) {
            AfterFailToWatchAD();
        }

        OnShowGoogleAds?.Invoke(false, AdsResultMessage.GoogleAdLoad_Fail);
        IsLoadNext = false;
        IsShowAds = false;
        GoogleAdReadyToShow = false;
    }

    /// <summary>
    /// 廣告顯示出來時呼叫 可以視需求關閉程式音訊或是遊戲迴圈
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public void HandleRewardedAdOpening(object sender, EventArgs args) {
        DebugLogger.Log("HandleRewardedAdOpening event received");
    }

    /// <summary>
    /// 廣告無法顯示出來時
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public void HandleRewardedAdFailedToShow(object sender, AdErrorEventArgs args) {
        DebugLogger.Log($"HandleRewardedAdFailedToShow event received with message: { args.AdError.GetMessage()}");
        if (IsShowAds) {
            AfterFailToWatchAD();
        }

        OnShowGoogleAds?.Invoke(false, AdsResultMessage.GoogleAdShow_Fail);
        IsShowAds = false;
        IsLoadNext = false;
        GoogleAdReadyToShow = false;
    }

    /// <summary>
    /// 使用者看完廣告 關閉廣告時呼叫
    /// 應用程式暫停音訊輸出或遊戲迴圈，這裡是恢復運作的絕佳位置
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public void HandleRewardedAdClosed(object sender, EventArgs args) {
        DebugLogger.Log("HandleRewardedAdClosed event received");
        OnShowGoogleAds?.Invoke(true, AdsResultMessage.GoogleAdsWatchSuccess);

        OnShowGoogleAds = null;
        // 預先載入下一個廣告
        CreateAndLoadRewardedAd();
        IsShowAds = false;
        GoogleAdReadyToShow = false;
        ActionAfterUserWatchAdSuccess();
    }

    /// <summary>
    /// 使用者應該獲得獎勵
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public void HandleUserEarnedReward(object sender, Reward args) {
        string type = args.Type;
        double amount = args.Amount;
        DebugLogger.Log($"HandleRewardedAdRewarded event received for {amount.ToString()} {type}");
    }

    /// <summary>
    /// 玩家選擇看廣告 在廣告載入完成後 可以呼叫 用來顯示出廣告
    /// </summary>
    private void UserChoseToWatchAd() {
        DebugLogger.Log($"[GoogleAdsManager] UserChoseToWatchAd");
        if (RewardedAd.IsLoaded()) {
            DebugLogger.Log($"[GoogleAdsManager] UserChoseToWatchAd RewardedAd IsLoaded And Will Show!");
            RewardedAd.Show();
            //ActionAfterUserWatchAdSuccess();
        } else {
            AfterFailToWatchAD();
        }
    }

    /// <summary>
    /// 觀看Google AD失敗
    /// </summary>
    private void AfterFailToWatchAD() {
        // Google看廣告失敗 改嘗試看Unity Ads  
        bool unityAdsEnable = FirestoreGameSetting.GetBoolData(GameDataDocEnum.ADReward, "UnityAdsEnable");
        DebugLogger.Log($"[GoogleAdsManager] unityAdsEnable={unityAdsEnable}");
        if (unityAdsEnable) {
#if UNITY_ADS
            UnityAdsManager.Inst.ShowUnityAd((isWatchAdSucces, adsResultMessage) => {
                // 看廣告成功
                if (isWatchAdSucces) {
                    DebugLogger.Log($"[GoogleAdsManager] 看UnityAds影片成功");
                    OnShowGoogleAds?.Invoke(true, AdsResultMessage.UnityAdsWatchSuccess);
                    ActionAfterUserWatchAdSuccess();
                    OnShowGoogleAds = null;
                } else {
                    CheckFacebookAdCanShow();
                }

            });
#else
            CheckFacebookAdCanShow();
#endif
        } else {
            CheckFacebookAdCanShow();
        }
    }

    private void CheckFacebookAdCanShow() {
        // Google看廣告失敗 改嘗試看Unity Ads
        bool facebookAdsEnable = FirestoreGameSetting.GetBoolData(GameDataDocEnum.ADReward, "FacebookAdsEnable");
        DebugLogger.Log($"[GoogleAdsManager] CheckFacebookAdCanShow facebookAdsEnable={facebookAdsEnable}");
        if (facebookAdsEnable) {
#if FACEBOOK_ADS
            FacebookAdsManager.Inst.ShowFacebookAd((isWatchAdSucces, adsResultMessage) => {
                // 看廣告成功
                if (isWatchAdSucces) {
                    DebugLogger.Log($"[FacebookAdsManager] 看FacebookAds影片成功");
                    OnShowGoogleAds?.Invoke(true, AdsResultMessage.FacebookAdsWatchSuccess);
                    ActionAfterUserWatchAdSuccess();
                    OnShowGoogleAds = null;
                } else {
                    CheckStartIoAdCanShow();
                }

            });
#else
            CheckStartIoAdCanShow();
#endif
        } else {
            CheckStartIoAdCanShow();
        }
    }

    private void CheckStartIoAdCanShow() {
        // Facebook看廣告失敗 改嘗試看StartIo Ads
        bool startIoAdsEnable = FirestoreGameSetting.GetBoolData(GameDataDocEnum.ADReward, "StartIoAdsEnable");
        DebugLogger.Log($"[GoogleAdsManager] CheckStartIoAdCanShow startIoAdsEnable={startIoAdsEnable}");
        if (startIoAdsEnable) {
#if STARTIO_ADS
            StartIoAdsManager.Inst.ShowStartIoAd((isWatchAdSucces, adsResultMessage) => {
                // 看廣告成功
                if (isWatchAdSucces) {
                    DebugLogger.Log($"[StartIoAdsManager] 看StartIoAds影片成功");
                    OnShowGoogleAds?.Invoke(true, AdsResultMessage.StartIoAdsWatchSuccess);
                    ActionAfterUserWatchAdSuccess();
                    OnShowGoogleAds = null;
                } else {
                    CheckDontShowAd();
                }                
            });
#else
            CheckDontShowAd();
#endif
        } else {
            CheckDontShowAd();
        }
    }

    /// <summary>
    /// 在上一個廣告關閉後 可以先載入下一個廣告
    /// </summary>
    public void CreateAndLoadRewardedAd() {
        DebugLogger.Log("CreateAndLoadRewardedAd Start");
        // RewardedAd是屬於一次性的物件 看完一次就沒有作用了 必須重建
        RewardedAd = new RewardedAd(AdUnitId);

        // Called when an ad request has successfully loaded.
        RewardedAd.OnAdLoaded += HandleRewardedAdLoaded;
        // Called when an ad request failed to load.
        RewardedAd.OnAdFailedToLoad += HandleRewardedAdFailedToLoad;
        // Called when an ad is shown.
        RewardedAd.OnAdOpening += HandleRewardedAdOpening;
        // Called when an ad request failed to show.
        RewardedAd.OnAdFailedToShow += HandleRewardedAdFailedToShow;
        // Called when the user should be rewarded for interacting with the ad.
        RewardedAd.OnUserEarnedReward += HandleUserEarnedReward;
        // Called when the ad is closed.
        RewardedAd.OnAdClosed += HandleRewardedAdClosed;

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the rewarded ad with the request.
        RewardedAd.LoadAd(request);

        IsLoadNext = true;
        DebugLogger.Log("CreateAndLoadRewardedAd Done");
    }

    private void ActionAfterUserWatchAdSuccess() {

#if APPSFLYER
        AppsFlyerManager.Inst.WatchAdDone(FirebaseManager.MyUser.UserId, AppsFlyKey);
#endif
    }

    /// <summary>
    /// 檢查不秀廣告直接給獎
    /// </summary>
    private void CheckDontShowAd() {
        // 直接給獎
        bool dontShowAD = FirestoreGameSetting.GetBoolData(GameDataDocEnum.ADReward, "DontShowAD");
        DebugLogger.Log($"[GoogleAdsManager] dontShowAD={dontShowAD}");
        if (dontShowAD) {
            DebugLogger.Log($"[GoogleAdsManager] 直接給獎");
            OnShowGoogleAds?.Invoke(true, AdsResultMessage.DontShowAdSuccess);
        } else { // 最後真的失敗了 
            DebugLogger.Log($"[GoogleAdsManager] 直接給獎失敗");
            OnShowGoogleAds?.Invoke(false, AdsResultMessage.DontShowAdFail);
        }
        OnShowGoogleAds = null;
        GoogleAdReadyToShow = false;
        IsLoadNext = false;
        IsShowAds = false;

#if APPSFLYER
        AppsFlyerManager.Inst.WatchAdShowFail(FirebaseManager.MyUser.UserId, AppsFlyKey);
#endif
    }
}
#endif