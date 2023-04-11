#if UNITY_ADS

using System;
using UnityEngine;
using TheDoor.Main;
using Scoz.Func;
using UnityEngine.Advertisements;

/// <summary>
/// Google AdMobs Manager
/// </summary>
public class UnityAdsManager : MonoSingletonA<UnityAdsManager>, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener {
    protected override bool destroyWhenRepeated { get { return true; } }
    protected override bool dontDestroyOnLoad { get { return true; } }

    /// <summary>
    /// Unity Ads展示成功或失敗的回呼
    /// </summary>
    public event Action<bool, AdsResultMessage> OnShowUnityAds;

    /// <summary>
    /// ads是否已經進行初始化
    /// </summary>
    private bool IsInit = false;

    /// <summary>
    /// 初始化是否完成
    /// </summary>
    private bool InitializeSuccess = false;

    // Unity AD 初始化
    [SerializeField] string AndroidGameId = "5085962";
    [SerializeField] string IOSGameId = "5085963";
    [SerializeField] bool TestMode = true;
    private string GameId;

    // 廣告單元初始化
    [SerializeField] string AndroidAdUnitId = "Rewarded_Android";
    [SerializeField] string IOsAdUnitId = "Rewarded_iOS";
    string AdUnitId;

    /// <summary>
    /// 當前能否顯示Unity Ad
    /// </summary>
    public bool UnityAdReadyToShow { get; set; }

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
        DebugLogger.Log("[Unity Ads開始初始化] InitializeAds");
        GameId = (Application.platform == RuntimePlatform.IPhonePlayer)
            ? IOSGameId
            : AndroidGameId;

#if Release
        Advertisement.Initialize(GameId, false, this);
#else
        Advertisement.Initialize(GameId, true, this);
#endif


        // Get the Ad Unit ID for the current platform:
#if UNITY_IOS
        AdUnitId = IOsAdUnitId;
#elif UNITY_ANDROID
        AdUnitId = AndroidAdUnitId;
#endif       
    }

    public void OnInitializationComplete() {
        DebugLogger.Log("[Unity Ads初始化完成] Unity Ads initialization complete.");
        InitializeSuccess = true;

        // 先預載一個AD
        LoadAd();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message) {
        DebugLogger.Log($"[Unity Ads初始化失敗] Unity Ads Initialization Failed: {error.ToString()} - {message}");
        InitializeSuccess = false;
    }

    /// <summary>
    /// 讀取廣告單元
    /// </summary>    
    public void LoadAd() { // Load content to the Ad Unit:
        // IMPORTANT! Only load content AFTER initialization (in this example, initialization is handled in a different script).
        DebugLogger.Log("[Unity Ads讀取廣告單元] Loading Ad: " + AdUnitId);
        Advertisement.Load(AdUnitId, this);
    }

    /// <summary>
    /// 顯示廣告單元
    /// </summary>    
    public void ShowUnityAd(Action<bool, AdsResultMessage> onShowUnityAdsCallBack) { // Show the loaded content in the Ad Unit:
        // Note that if the ad content wasn't previously loaded, this method will fail
        DebugLogger.Log("[Unity Ads顯示廣告單元] Showing Ad: " + AdUnitId);
        OnShowUnityAds = onShowUnityAdsCallBack;

        if (!InitializeSuccess) {
            onShowUnityAdsCallBack?.Invoke(false, AdsResultMessage.UnityAds_Not_Initialize);
            DebugLogger.Log("[Unity Ads顯示廣告單元] UnityAds_Not_Initialize Showing Ad: " + AdUnitId);
            OnShowUnityAds = null;
            return;
        }

        if (!UnityAdReadyToShow) {
            onShowUnityAdsCallBack?.Invoke(false, AdsResultMessage.UnityAds_NotReady);
            DebugLogger.Log("[Unity Ads顯示廣告單元] UnityAds_NotReady Showing Ad: " + AdUnitId);
            OnShowUnityAds = null;
            // 沒有廣告可看 重新再載一次新的廣告試試看
            if (Advertisement.isInitialized) {
                LoadAd();
            }            
            return;
        }

        if (IsShowAds) {
            onShowUnityAdsCallBack?.Invoke(false, AdsResultMessage.Ads_AlreadyShowing);
            DebugLogger.Log("[Unity Ads顯示廣告單元] Ads_AlreadyShowing Showing Ad: " + AdUnitId);
            OnShowUnityAds = null;
            return;
        }
                        
        Advertisement.Show(AdUnitId, this);
        IsShowAds = true;
        // Unity Ad 看完 要再載入新的
        UnityAdReadyToShow = false;
    }

    // Implement Load Listener and Show Listener interface methods: 
    public void OnUnityAdsAdLoaded(string adUnitId) {
        DebugLogger.Log($"[Unity Ads載入廣告單元成功] adUnitId={adUnitId}");
        // Optionally execute code if the Ad Unit successfully loads content.
        // 載入成功 表示可以看了
        if (adUnitId.Equals(AdUnitId)) {
            UnityAdReadyToShow = true;
        }        
    }

    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message) {
        DebugLogger.Log($"[Unity Ads載入廣告單元失敗] Error loading Ad Unit: {adUnitId} - {error.ToString()} - {message}");
        // Optionally execute code if the Ad Unit fails to load, such as attempting to try again.
        OnShowUnityAds?.Invoke(false, AdsResultMessage.UnityAdLoad_Fail);
        UnityAdReadyToShow = false;
    }

    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message) {
        DebugLogger.Log($"[Unity Ads顯示廣告單元失敗] Error showing Ad Unit {adUnitId}: {error.ToString()} - {message}");
        // Optionally execute code if the Ad Unit fails to show, such as loading another ad.
        OnShowUnityAds?.Invoke(false, AdsResultMessage.UnityAdShow_Fail);
        IsShowAds = false;
    }

    public void OnUnityAdsShowStart(string adUnitId) { 
    }

    public void OnUnityAdsShowClick(string adUnitId) { 
    }

    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState) {
        if (adUnitId.Equals(AdUnitId) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED)) {
            DebugLogger.Log("Unity Ads Rewarded Ad Completed");
            // 給獎勵
            OnShowUnityAds?.Invoke(true, AdsResultMessage.UnityAdsWatchSuccess);

            // Load another ad:
            Advertisement.Load(AdUnitId, this);
            IsShowAds = false;
        }
    }
}
#endif