#if FACEBOOK_ADS

using System;
using UnityEngine;
using TheDoor.Main;
using Scoz.Func;
using AudienceNetwork;

/// <summary>
/// Facebook Ads Manager
/// </summary>
public class FacebookAdsManager : MonoSingletonA<FacebookAdsManager> {
    protected override bool destroyWhenRepeated { get { return true; } }
    protected override bool dontDestroyOnLoad { get { return true; } }

    /// <summary>
    /// Unity Ads展示成功或失敗的回呼
    /// </summary>
    public event Action<bool, AdsResultMessage> OnShowFacebookAds;

    /// <summary>
    /// ads是否已經進行初始化
    /// </summary>
    private bool IsInit = false;

    /// <summary>
    /// 初始化是否完成
    /// </summary>
    public bool InitializeSuccess {
        get { return AudienceNetworkAds.IsInitialized(); }
    }

    /// <summary>
    /// 廣告Ad單元
    /// </summary>
    private RewardedVideoAd RewardedVideoAd;

    // 廣告單元初始化
    [SerializeField] string AndroidPlacementID = "420215393467947_455398239949662";
    [SerializeField] string IOSPlacementID = "420215393467947_455399426616210";

    /// <summary>
    /// 當前能否顯示Facebook Ad
    /// </summary>
    public bool FacebookAdReadyToShow { get; set; }

    /// <summary>
    /// 是否正要顯示Unity Ads
    /// </summary>
    public bool IsShowAds { get; set; }

    /// <summary>
    /// 初始化UnityAds
    /// </summary>
    public void InitializeAds() {
        if (IsInit) {
            return;
        }

        IsInit = true;
        DebugLogger.Log("[Facebook Ads開始初始化] Initialize");
#if !UNITY_EDITOR
        AudienceNetworkAds.Initialize();
        LoadRewardedVideo();
#endif
    }

    public void LoadRewardedVideo() {
        RewardedVideoAd = null;
        // Create the rewarded video unit with a placement ID (generate your own on the Facebook app settings).
        // Use different ID for each ad placement in your app.
#if UNITY_IOS
        RewardedVideoAd = new RewardedVideoAd(IOSPlacementID);
#elif UNITY_ANDROID
        RewardedVideoAd = new RewardedVideoAd(AndroidPlacementID);
#else
        RewardedVideoAd = new RewardedVideoAd("YOUR_PLACEMENT_ID");
#endif

        FacebookAdReadyToShow = false;

        RewardedVideoAd.Register(this.gameObject);
        // Set delegates to get notified on changes or when the user interacts with the ad.
        RewardedVideoAd.RewardedVideoAdDidLoad += RewardedVideoAdDidLoad;
        RewardedVideoAd.RewardedVideoAdDidFailWithError += RewardedVideoAdDidFailWithError;
        RewardedVideoAd.RewardedVideoAdWillLogImpression += RewardedVideoAdWillLogImpression;
        RewardedVideoAd.RewardedVideoAdDidClick += RewardedVideoAdDidClick;
        RewardedVideoAd.RewardedVideoAdDidClose += RewardedVideoAdDidClose;

        // Initiate the request to load the ad.
        RewardedVideoAd.LoadAd();
    }

    public void RewardedVideoAdDidLoad() {
        DebugLogger.Log("[Facebook Ads讀取廣告單元] RewardedVideo ad loaded.");
        FacebookAdReadyToShow = true;
    }


    public void RewardedVideoAdDidFailWithError(string error) {
        DebugLogger.Log("[Facebook Ads讀取廣告失敗] RewardedVideo ad failed to load with error: " + error);
        OnShowFacebookAds?.Invoke(false, AdsResultMessage.FacebookAdLoad_Fail);
        FacebookAdReadyToShow = false;
    }

    public void RewardedVideoAdWillLogImpression() {
        DebugLogger.Log("[Facebook Ads顯示廣告單元] RewardedVideo ad logged impression.");
    }

    public void RewardedVideoAdDidClick() {
        DebugLogger.Log("[Facebook Ads廣告單元被點擊] RewardedVideo ad clicked.");
    }

    public void RewardedVideoAdDidClose() {
        DebugLogger.Log("[Facebook Ads廣告單元關閉] Rewarded video ad did close.");
        if (RewardedVideoAd != null) {
            RewardedVideoAd.Dispose();
        }

        // 給獎勵
        OnShowFacebookAds?.Invoke(true, AdsResultMessage.FacebookAdsWatchSuccess);
        IsShowAds = false;

        // Load another ad:
        LoadRewardedVideo();
    }

    /// <summary>
    /// 顯示廣告單元
    /// </summary>    
    public void ShowFacebookAd(Action<bool, AdsResultMessage> onShowFacebookAdsCallBack) { // Show the loaded content in the Ad Unit:
        // Note that if the ad content wasn't previously loaded, this method will fail
        DebugLogger.Log("[Facebook Ads顯示廣告單元] Showing Ad: ");
        OnShowFacebookAds = onShowFacebookAdsCallBack;

        if (!InitializeSuccess) {
            onShowFacebookAdsCallBack?.Invoke(false, AdsResultMessage.FacebookAds_Not_Initialize);
            DebugLogger.Log("[Facebook Ads顯示廣告單元] FacebookAds_Not_Initialize");
            OnShowFacebookAds = null;
            return;
        }

        if (!FacebookAdReadyToShow) {
            onShowFacebookAdsCallBack?.Invoke(false, AdsResultMessage.FacebookAds_NotReady);
            DebugLogger.Log("[Facebook Ads顯示廣告單元] FacebookAds_NotReady");
            OnShowFacebookAds = null;
            // 沒有廣告可看 重新再載一次新的廣告試試看
            if (RewardedVideoAd == null || !(RewardedVideoAd != null && RewardedVideoAd.IsValid())) {
                LoadRewardedVideo();
            }
            return;
        }

        if (IsShowAds) {
            onShowFacebookAdsCallBack?.Invoke(false, AdsResultMessage.Ads_AlreadyShowing);
            DebugLogger.Log("[Facebook Ads顯示廣告單元] Ads_AlreadyShowing");
            OnShowFacebookAds = null;
            return;
        }

        RewardedVideoAd.Show();
        IsShowAds = true;
        // Facebook Ad 看完 要再載入新的
        FacebookAdReadyToShow = false;
    }
}
#endif