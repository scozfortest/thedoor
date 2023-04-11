#if STARTIO_ADS

using System;
using UnityEngine;
using MaJamPachinko.Main;
using Scoz.Func;
using StartApp;
using System.Collections;

/// <summary>
/// StartIo Ads Manager
/// </summary>
public class StartIoAdsManager : MonoSingletonA<StartIoAdsManager> {
    protected override bool destroyWhenRepeated { get { return true; } }
    protected override bool dontDestroyOnLoad { get { return true; } }

    /// <summary>
    /// Unity Ads展示成功或失敗的回呼
    /// </summary>
    public event Action<bool, AdsResultMessage> OnShowStartIoAds;

    /// <summary>
    /// ads是否已經進行初始化
    /// </summary>
    private bool IsInit = false;

    /// <summary>
    /// 初始化是否完成
    /// </summary>
    public bool InitializeSuccess { get ; set; }

    // 廣告單元Key 記錄在Resoureces的StartAppDataAndroid以及StartAppDataiOS中
    [SerializeField] string AndroidAppID = "211679122";
    [SerializeField] string IOSAppKeyID = "211321604";

    /// <summary>
    /// 是否正要顯示Unity Ads
    /// </summary>
    public bool IsShowAds { get; set; }

    /// <summary>
    /// 當前能否顯示StartIo Ad
    /// </summary>
    public bool StartIoAdReadyToShow { get; set; }

    private InterstitialAd InterstitialAd;

    /// <summary>
    /// 初始化UnityAds
    /// </summary>
    public void InitializeAds() {
        if (IsInit) {
            return;
        }

        IsInit = true;
        DebugLogger.Log("[StartIo Ads開始初始化] Initialize");

        AdSdk.Instance.DisableReturnAds();
#if Release
        AdSdk.Instance.SetTestAdsEnabled(false);
#else
        AdSdk.Instance.SetTestAdsEnabled(true);
#endif
        //var config = new SplashConfig {
        //    TemplateTheme = SplashConfig.Theme.Blaze
        //};
        //AdSdk.Instance.ShowSplash(config);


        InitializeSuccess = false;
        InterstitialAd = AdSdk.Instance.CreateInterstitial("myTagForFullscreen");
        InterstitialAd.RaiseAdLoaded += OnRaiseAdLoaded;
        InterstitialAd.RaiseAdShown += OnRaiseAdShown;
        InterstitialAd.RaiseAdImpressionSent += OnRaiseAdImpressionSent;
        InterstitialAd.RaiseAdLoadingFailed += OnRaiseAdLoadingFailed;
        InterstitialAd.RaiseAdClosed += OnRaiseAdClosed;
        InterstitialAd.RaiseAdClicked += OnRaiseAdClicked;
        InterstitialAd.RaiseAdVideoCompleted += OnRaiseAdVideoCompleted;
        StartCoroutine(LoadAdUntilInitializeSuccess());
        
    }

    IEnumerator LoadAdUntilInitializeSuccess() {
        // 初始化要10秒= =
        yield return new WaitForSeconds(10.0f);
        InitializeSuccess = true;
        LoadInterstitial();
    }

    void LoadInterstitial() {
        InterstitialAd.LoadAd(InterstitialAd.AdType.Rewarded);
    }

    #region InterstitialAd Callbacks
    //Called when rewarded video was loaded (precache flag shows if the loaded ad is precache).
    private void OnRaiseAdLoaded(object sender, EventArgs e) {
        DebugLogger.Log("[StartIo Ads讀取廣告單元] RewardedVideo ad loaded.");
        StartIoAdReadyToShow = true;
    }

    // Called when rewarded video is shown
    private void OnRaiseAdShown(object sender, EventArgs e) {
        DebugLogger.Log("[StartIo Ads顯示廣告成功] RewardedVideo shown");
        StartIoAdReadyToShow = false;
    }

    private void OnRaiseAdImpressionSent(object sender, EventArgs e) {
        DebugLogger.Log("[StartIo Ads開始展示廣告] OnRaiseAdImpressionSent");
    }

    // Called when rewarded video failed to load
    private void OnRaiseAdLoadingFailed(object sender, MessageArgs e) {
        DebugLogger.Log("[StartIo Ads讀取廣告失敗] RaiseAdLoadingFailed failed to load: " + e.Message);
        OnShowStartIoAds?.Invoke(false, AdsResultMessage.StartIoAdLoad_Fail);
        StartIoAdReadyToShow = false;        
    }

    // Called when rewarded video was loaded, but cannot be shown (internal network errors, placement settings, or incorrect creative)
    private void OnRewardedVideoShowFailed(object sender, EventArgs e) {
        DebugLogger.Log("[StartIo Ads顯示廣告失敗] RewardedVideo show failed");
        OnShowStartIoAds?.Invoke(false, AdsResultMessage.StartIoAdShow_Fail);
        IsShowAds = false;
        StartIoAdReadyToShow = false;
        // 顯示失敗後 重新載入一個
        LoadInterstitial();
    }
    
    // Called when rewarded video is closed
    private void OnRaiseAdClosed(object sender, EventArgs e) {
        DebugLogger.Log("[StartIo Ads廣告關閉] RewardedVideo closed");
        // 給獎勵
        OnShowStartIoAds?.Invoke(true, AdsResultMessage.StartIoAdsWatchSuccess);
        IsShowAds = false;
        StartIoAdReadyToShow = false;
        // Load another ad:        
        LoadInterstitial();
    }

    // Called when rewarded video is viewed until the end
    private void OnRaiseAdVideoCompleted(object sender, EventArgs e) {
        DebugLogger.Log("[StartIo Ads廣告展示到最後成功] RewardedVideo finished");
    }
    // Called when reward video is clicked
    private void OnRaiseAdClicked(object sender, EventArgs e) {
        DebugLogger.Log("[StartIo Ads廣告被點擊成功] RewardedVideo clicked");
    }
#endregion

    /// <summary>
    /// 顯示廣告單元
    /// </summary>    
    public void ShowStartIoAd(Action<bool, AdsResultMessage> onShowStartIoAdsCallBack) { // Show the loaded content in the Ad Unit:
        // Note that if the ad content wasn't previously loaded, this method will fail
        DebugLogger.Log("[StartIo Ads顯示廣告單元] Showing Ad: ");
        OnShowStartIoAds = onShowStartIoAdsCallBack;

        if (!InitializeSuccess) {
            onShowStartIoAdsCallBack?.Invoke(false, AdsResultMessage.StartIoAds_Not_Initialize);
            DebugLogger.Log("[StartIo Ads顯示廣告單元] StartIoAds_Not_Initialize");
            OnShowStartIoAds = null;
            return;
        }

        if (!StartIoAdReadyToShow) {
            onShowStartIoAdsCallBack?.Invoke(false, AdsResultMessage.StartIoAds_NotReady);
            DebugLogger.Log("[StartIo Ads顯示廣告單元] StartIoAdReadyToShow = false ShowStartIoAd StartIoAds_NotReady");
            OnShowStartIoAds = null;
            // 重新載入一個
            LoadInterstitial();
            return;
        }

        if (IsShowAds) {
            onShowStartIoAdsCallBack?.Invoke(false, AdsResultMessage.Ads_AlreadyShowing);
            DebugLogger.Log("[StartIo Ads顯示廣告單元] Ads_AlreadyShowing");
            OnShowStartIoAds = null;
            return;
        }

        if (InterstitialAd.IsReady()) {
            InterstitialAd.ShowAd();
            IsShowAds = true;
            DebugLogger.Log("[StartIo Ads顯示廣告單元] 開始 ShowAd");
        } else { // 沒準備好
            onShowStartIoAdsCallBack?.Invoke(false, AdsResultMessage.StartIoAds_NotReady);
            DebugLogger.Log("[StartIo Ads顯示廣告單元] ShowStartIoAd StartIoAds_NotReady");
            OnShowStartIoAds = null;
            IsShowAds = false;
            // 重新載入一個
            LoadInterstitial();
        }
    }
}
#endif