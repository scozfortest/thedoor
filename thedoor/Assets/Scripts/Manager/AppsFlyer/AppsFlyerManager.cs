#if APPSFLYER

using System;
using UnityEngine;
using TheDoor.Main;
using Scoz.Func;
using System.Collections.Generic;
using System.Collections;
using AppsFlyerSDK;
using System.Text;
#if !UNITY_EDITOR
#if UNITY_IOS
using Unity.Notifications.iOS;
using Unity.Advertisement.IosSupport;
#elif UNITY_ANDROID
using Firebase.Messaging;
using Firebase.Unity;
#endif
#endif

/// <summary>
/// AppsFlyer Manager
/// </summary>
public class AppsFlyerManager : MonoSingletonA<AppsFlyerManager> {
    protected override bool destroyWhenRepeated { get { return true; } }
    protected override bool dontDestroyOnLoad { get { return true; } }

    // 只有定義了AppsFlyer 且非編輯模式才會進行初始化
#if APPSFLYER && !UNITY_EDITOR
    /// <summary>
    /// AppsFlyer是否已經進行初始化
    /// </summary>
    private bool IsInit = false;
#else
    /// <summary>
    /// AppsFlyer是否已經進行初始化
    /// </summary>
    private bool IsInit = true;
#endif

    private void Start() {
        if (IsInit) {
            return;
        }

#if Release
        // Release要關閉AppsFlyer的DebugMode
        AppsFlyer.setIsDebug(false);
#else
        AppsFlyer.setIsDebug(true);
#endif
        Initialize();
    }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="uid">玩家識別碼</param>
    public void Initialize() {
        if (IsInit) {
            return;
        }

        IsInit = true;

#if !UNITY_EDITOR
#if UNITY_IOS
            StartCoroutine(RequestAuthorization());
#elif UNITY_ANDROID
            FirebaseMessaging.TokenReceived += OnTokenReceived;
#endif
#endif
    }

#if !UNITY_EDITOR
#if UNITY_IOS
        IEnumerator RequestAuthorization() {
            using (var req = new AuthorizationRequest(AuthorizationOption.Alert | AuthorizationOption.Badge, true)) {

                while (!req.IsFinished) {
                    yield return null;
                }
                if (req.Granted && req.DeviceToken != "") {
                    AppsFlyer.registerUninstall(Encoding.UTF8.GetBytes(req.DeviceToken));
                }
            }
        }

        public void IOSAskATTUserTrack() {
            // IOS 透明度追蹤
            if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus()
                == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED) {
                ATTrackingStatusBinding.RequestAuthorizationTracking();
            }
        }

#elif UNITY_ANDROID
        public void OnTokenReceived(object sender, TokenReceivedEventArgs token) {
            WriteLog.Log("Received Registration Token: " + token.Token);
            AppsFlyer.updateServerUninstallToken(token.Token);
        }
#endif

#endif

    #region 追蹤事件
    /// <summary>
    /// AppsFlyer 自定義事件
    /// </summary>
    /// <param name="eventName">自定義事件名稱 也可採用AFInAppEvents</param>
    /// <param name="eventValue"></param>
    public void SendEvent(string eventName, Dictionary<string, string> eventValue) {
#if !UNITY_EDITOR
        WriteLog.Log($"AppsFlyer SendEvent eventName={eventName}");
        foreach (var pair in eventValue) {
            WriteLog.Log($"eventValue key={pair.Key}, value={pair.Value}");
        }
        AppsFlyer.sendEvent(eventName, eventValue);
#endif
    }

    /// <summary>
    /// 內購追蹤(price正值:購買 price負值:取消購買或退款)
    /// </summary>
    /// <param name="purchaseType">內購類型金幣或金鈔(Gold or Point)</param>
    /// <param name="productID">平台產品名稱ID</param>
    /// <param name="price">金額 依照幣值給予數字</param>
    /// <param name="quantity">獲得數量(金幣或金鈔)</param>
    /// <param name="currency">幣值縮寫 ex:TWD USD</param>
    /// <param name="transactionID">訂單編號</param>    
    public void PurchaseIAP(string purchaseType, string productID, string price, string quantity, string currency, string transactionID) {
#if !UNITY_EDITOR
        AppsFlyer.sendEvent(AFInAppEvents.PURCHASE, new Dictionary<string, string>(){
            {AFInAppEvents.CONTENT_TYPE, purchaseType},
            {AFInAppEvents.CONTENT_ID, productID},            
            {AFInAppEvents.REVENUE, price},            
            {AFInAppEvents.QUANTITY, quantity},
            {AFInAppEvents.CURRENCY, currency},
            {AFInAppEvents.RECEIPT_ID, transactionID}
        });
        WriteLog.Log($"[AppsFlyer][PurchaseIAP] purchaseType={purchaseType}, productID={productID}, price={price}, quantity={quantity}, currency={currency}, transactionID={transactionID}");
#endif
    }

    /// <summary>
    /// 玩家第一次登入完成註冊事件(國家或許可以加)
    /// </summary>
    /// <param name="uid">玩家識別碼</param>
    /// <param name="authType">註冊方式</param>
    /// <param name="language">玩家語系</param>
    public void CompleteRegistration(string uid, string authType, string language) {
#if !UNITY_EDITOR
        AppsFlyer.sendEvent(AFInAppEvents.COMPLETE_REGISTRATION, new Dictionary<string, string>(){
            {AFInAppEvents.PARAM_1, uid},
            {AFInAppEvents.PARAM_2, authType},
#if UNITY_IOS
            {AFInAppEvents.PARAM_3, "Apple"},
#elif UNITY_ANDROID
            {AFInAppEvents.PARAM_3, "Google"},
#endif
            {AFInAppEvents.PARAM_4, language}
        });

        WriteLog.Log($"[AppsFlyer][CompleteRegistration] uid={uid}, authType={authType}, language={language}");
#endif
    }
    #endregion

    /// <summary>
    /// 玩家登入
    /// </summary>
    /// <param name="uid"></param>
    public void Login(string uid) {
#if !UNITY_EDITOR
        AppsFlyer.sendEvent(AFInAppEvents.LOGIN, new Dictionary<string, string>(){
            {AFInAppEvents.PARAM_1, uid}            
        });

        WriteLog.Log($"[AppsFlyer][Login] uid={uid}");
#endif
    }

    /// <summary>
    /// 廣告點擊
    /// </summary>
    /// <param name="uid">玩家識別碼</param>
    /// <param name="fromType">廣告點擊的進入點</param>
    public void WatchAdClick(string uid, string fromType) {
#if !UNITY_EDITOR
        WriteLog.Log($"[AppsFlyerManager] Send Event WatchAdClick Done uid={uid} fromType={fromType}");
        AppsFlyer.sendEvent(AFInAppEvents.AD_CLICK, new Dictionary<string, string>(){
            {AFInAppEvents.PARAM_1, uid},
            {AFInAppEvents.PARAM_2, fromType}
        });

        WriteLog.Log($"[AppsFlyer][WatchAdClick] uid={uid}, fromType={fromType}");
#endif
    }

    /// <summary>
    /// 看廣告
    /// </summary>
    /// <param name="uid">玩家識別碼</param>
    public void WatchAdDone(string uid, string fromType) {
#if !UNITY_EDITOR
        AppsFlyer.sendEvent(AFInAppEvents.AD_VIEW, new Dictionary<string, string>(){
            {AFInAppEvents.PARAM_1, uid},
            {AFInAppEvents.PARAM_2, fromType}
        });

        WriteLog.Log($"[AppsFlyer][WatchAdDone] uid={uid}");
#endif
    }

    /// <summary>
    /// 看廣告顯示失敗
    /// </summary>
    /// <param name="uid">玩家識別碼</param>
    public void WatchAdShowFail(string uid, string fromType) {
#if !UNITY_EDITOR
        AppsFlyer.sendEvent("af_ad_show_view_fail", new Dictionary<string, string>(){
            {AFInAppEvents.PARAM_1, uid},
            {AFInAppEvents.PARAM_2, fromType},
        });

        WriteLog.Log($"[AppsFlyer][WatchAdShowFail] uid={uid}");
#endif
    }

    /// <summary>
    /// 設定玩家的Appsflyer CustomerUid
    /// </summary>
    /// <param name="uid"></param>
    public void SetCustomerUserId(string uid) {
#if !UNITY_EDITOR
        AppsFlyer.setCustomerUserId(uid);
        WriteLog.Log($"[AppsFlyer][SetCustomerUserId] uid={uid}");
#endif
    }

}
#endif