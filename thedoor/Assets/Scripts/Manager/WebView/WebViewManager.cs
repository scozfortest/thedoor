using System;
using UnityEngine;
using TheDoor.Main;
using Scoz.Func;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

/// <summary>
/// Webview Manager
/// </summary>
public class WebViewManager : MonoSingletonA<WebViewManager> {
    protected override bool destroyWhenRepeated { get { return true; } }
    protected override bool dontDestroyOnLoad { get { return true; } }

    /// <summary>
    /// 網頁載入結束的回呼 回傳網頁的StatusCode
    /// </summary>
    private event Action<int> OnWebViewPageFinished;
    /// <summary>
    /// 網頁關閉時的回呼 回傳是否被關閉
    /// </summary>
    private event Action<bool> OnWebViewShouldClose;
    /// <summary>
    /// 收到網頁傳遞的訊息
    /// </summary>
    private event Action<UniWebViewMessage> OnWebViewReceiveMessage;

    [Header("UniWebView")]
    [SerializeField]
    private UniWebView UniWebView;

    [Header("UniWebView GameObject")]
    [SerializeField]
    private GameObject UniWebViewObj;

    /// <summary>
    /// WebView是否準備載入中
    /// </summary>
    private bool IsLoadingWebView = false;

    /// <summary>
    /// 目前的UniWebView
    /// </summary>
    public UniWebView CurrentUniWebView { get { return UniWebView; } }

    private void Start() {
        
    }

    /// <summary>
    /// 顯示一般全版的說明網頁
    /// </summary>
    /// <param name="backendURLType">後台網址類型</param>
    /// <param name="param1">在backendURL中帶入的參數字串1</param>
    /// <param name="param2">在backendURL中帶入的參數字串2</param>
    /// <param name="setZoomEnabled">允許使用者對Webview網頁做大小縮放</param>
    /// <param name="setLoadWithOverviewMode">是否設定自適應網頁的模式</param>
    /// <param name="setShowSpinnerWhileLoading">是否顯示載入的系統輪子</param>
    /// <param name="setBackButtonEnabled">是否有返回按鍵可以按(用來做網頁上的瀏覽)</param>
    public void ShowWebview(BackendURLType backendURLType, string param1 = null, string param2 = null, bool setZoomEnabled = false, bool setLoadWithOverviewMode = true, bool setShowSpinnerWhileLoading = false, bool setBackButtonEnabled = false) {
        Rect rect = new Rect(0, 0, Screen.width, Screen.height);
        string backendAddress = FirestoreGameSetting.GetStrData(GameDataDocEnum.BackendURL, BackendURLType.BackendAddress.ToString());
        string backendURL = FirestoreGameSetting.GetStrData(GameDataDocEnum.BackendURL, backendURLType.ToString());
        if (!string.IsNullOrEmpty(backendURL)) {
            string showUrl = string.Empty;
            if (!string.IsNullOrEmpty(param1)) { // 參數1不為空的
                string customizedUrl = string.Empty;
                if (string.IsNullOrEmpty(param2)) { // 只有參數1
                    customizedUrl = string.Format(backendURL, param1);
                } else { // 帶入兩個參數字串
                    customizedUrl = string.Format(backendURL, param1, param2);
                }
                showUrl = string.Concat(backendAddress, customizedUrl);
            } else {
                showUrl = string.Concat(backendAddress, backendURL);
            }
            ShowWebview(showUrl, rect);
            SetZoomEnabled(setZoomEnabled);
            SetLoadWithOverviewMode(setLoadWithOverviewMode);
            SetShowSpinnerWhileLoading(setShowSpinnerWhileLoading);
            SetBackButtonEnabled(setBackButtonEnabled);
        }
    }

    /// <summary>
    /// 顯示非全版的Webview
    /// </summary>
    /// <param name="backendURLType">顯示的Webview型別</param>
    /// <param name="rect">要顯示的範圍區間</param>
    /// <param name="parentRectTransform">Webview要綁定的Parent元件</param>
    /// <param name="param1">要帶入的參數字串1</param>
    /// <param name="param2">要帶入的參數字串2</param>
    public void ShowWebview(BackendURLType backendURLType, Rect rect, RectTransform parentRectTransform, string param1 = null, string param2 = null) {
        // WebView 要在父元件Activate狀態下才能載入
        if (CurrentUniWebView == null) {
            DebugLogger.Log($"Width={Screen.width}, Height={Screen.height}, ResW={Screen.currentResolution.width}, ResH={Screen.currentResolution.height}, dpi={Screen.dpi}");
            
            string backendAddress = FirestoreGameSetting.GetStrData(GameDataDocEnum.BackendURL, BackendURLType.BackendAddress.ToString());
            string backendURL = FirestoreGameSetting.GetStrData(GameDataDocEnum.BackendURL, backendURLType.ToString());
            if (!string.IsNullOrEmpty(backendURL)) {
                string showUrl = string.Empty;
                if (!string.IsNullOrEmpty(param1)) { // 參數1不為空的
                    string customizedUrl = string.Empty;
                    if (string.IsNullOrEmpty(param2)) { // 只有參數1
                        customizedUrl = string.Format(backendURL, param1);
                    } else { // 帶入兩個參數字串
                        customizedUrl = string.Format(backendURL, param1, param2);
                    }
                    showUrl = string.Concat(backendAddress, customizedUrl);
                } else {
                    showUrl = string.Concat(backendAddress, backendURL);
                }
                ShowWebview(showUrl, rect, false, false, parentRectTransform);
                SetBackButtonEnabled(false);    // 關閉返回按鍵
                SetZoomEnabled(true);           // 設定可以縮放
                SetLoadWithOverviewMode(true);  // 設定自適應模式

                // 依據寬度跟DPI的不同比例 以1080(width)/450(dpi) = 2.4f當成基準值
                if (Screen.dpi != 0) {
                    int textZoomSize = (int)(((Screen.width / Screen.dpi) / 2.4f) * 100);
                    DebugLogger.Log($"textZoomSize={textZoomSize}");
                    if (textZoomSize > 100) {
                        textZoomSize = 100;
                    }
                    SetTextZoom(textZoomSize);
                } else {
                    SetTextZoom(100);
                }
            }
        }
    }

    /// <summary>
    /// 顯示Webview
    /// </summary>
    /// <param name="url">要顯示的網址</param>
    /// <param name="rect">顯示視窗大小(x, y, width, height) width and height = 0 不設定視窗大小</param>
    /// <param name="fullscreen">是否為全螢幕</param>
    /// <param name="useToolbar">是否使用工具列(有關閉按鈕)</param>
    /// <param name="parentRectTransform">綁定的ParentTrans</param>
    /// <param name="onWebViewPageFinished">網頁載入後呼叫</param>
    /// <param name="onWebViewShouldClose">網頁關閉後呼叫</param>
    /// <param name="onWebViewReceiveMessage">收到網頁訊息後呼叫</param>
    public void ShowWebview(string url, Rect rect, bool fullscreen = true, bool useToolbar = true, RectTransform parentRectTransform = null, Action<int> onWebViewPageFinished = null, Action<bool> onWebViewShouldClose = null, Action<UniWebViewMessage> onWebViewReceiveMessage = null) {
        if (IsLoadingWebView) {
            DebugLogger.Log($"[WebViewManager][ShowWebview] IsLoadingWebView={IsLoadingWebView}");
            return;
        }

        if (UniWebView != null) {
            RemoveWebView();
        }        

        DebugLogger.Log($"[WebViewManager][ShowWebview] url={url}");
                  
        UniWebViewObj = new GameObject("UniWebView");
        if (parentRectTransform != null) {
            UniWebViewObj.transform.SetParent(parentRectTransform.gameObject.transform);
            UniWebViewObj.transform.localPosition = Vector3.zero;
            UniWebViewObj.transform.localRotation = Quaternion.identity;
        }

        UniWebView = UniWebViewObj.AddComponent<UniWebView>();

#if !UNITY_EDITOR
        // 把上面Done的按鍵的文字改為關閉
        if (useToolbar) {
            UniWebView.SetDoneButtonText(StringData.GetUIString("Close"));
        }
#if !UNITY_IOS
        UniWebView.SetShowToolbar(useToolbar);
        UniWebView.SetToolbarDoneButtonText(StringData.GetUIString("Close"));
#endif
        
#endif
        if (rect.width != 0 && rect.height != 0) {
            UniWebView.Frame = rect;
        }
        IsLoadingWebView = true;
        UniWebView.Load(url);
        UniWebView.Show(fullscreen, useToolbar, UniWebViewToolbarPosition.Top, parentRectTransform);

        UniWebView.OnPageFinished += OnPageFinished;
        OnWebViewPageFinished = onWebViewPageFinished;

        UniWebView.OnShouldClose += OnShouldClose;
        OnWebViewShouldClose = onWebViewShouldClose;

        UniWebView.OnMessageReceived += OnMessageReceived;
        OnWebViewReceiveMessage = onWebViewReceiveMessage;
    }

    /// <summary>
    /// 開啟網頁結束
    /// </summary>
    /// <param name="webView"></param>
    /// <param name="statusCode"></param>
    /// <param name="url"></param>
    private void OnPageFinished (UniWebView webView, int statusCode, string url) {
        OnWebViewPageFinished?.Invoke(statusCode);
        IsLoadingWebView = false;
    }

    /// <summary>
    /// 關閉舊的Webview
    /// </summary>
    /// <param name="webView"></param>
    private bool OnShouldClose(UniWebView webView) {
        OnWebViewShouldClose?.Invoke(true);
        RemoveWebView();
        return true;
    }

    /// <summary>
    /// 移除WebView
    /// </summary>
    private void RemoveWebView() {
        if (UniWebView != null) {
            UniWebView.OnPageFinished -= OnPageFinished;
            UniWebView.OnShouldClose -= OnShouldClose;
            UniWebView.OnMessageReceived -= OnMessageReceived;
            Destroy(UniWebView.gameObject);
        }
        OnWebViewPageFinished = null;
        OnWebViewShouldClose = null;
        OnWebViewReceiveMessage = null;        
        UniWebView = null;
        IsLoadingWebView = false;
    }

    /// <summary>
    /// 收到網頁的訊息
    /// </summary>
    /// <param name="webView"></param>
    /// <param name="message"></param>
    private void OnMessageReceived(UniWebView webView, UniWebViewMessage message) {

        //if (message.Path.Equals("close")) {
        //    Destroy(UniWebView.gameObject);
        //    UniWebView = null;
        //}

        OnWebViewReceiveMessage?.Invoke(message);
    }

    /// <summary>
    /// 設定toolbar的標題
    /// </summary>
    /// <param name="titleText"></param>
    public void SetTitleText(string titleText) {
#if !UNITY_EDITOR      
        if (UniWebView != null) {
            UniWebView.SetDoneButtonText(titleText);
        }
#endif
    }

    /// <summary>
    /// 手機返回鍵是否有作用
    /// </summary>
    /// <param name="backEnable">返回是否有作用</param>
    public void SetBackButtonEnabled(bool backEnable) {
#if !UNITY_EDITOR
        if (UniWebView != null) {
            UniWebView.SetBackButtonEnabled(backEnable);
        }
#endif
    }

    /// <summary>
    /// 手機Webview上縮放是否有作用
    /// </summary>
    /// <param name="zoomEnable">是否能縮放</param>
    public void SetZoomEnabled(bool zoomEnable) {
#if !UNITY_EDITOR
        if (UniWebView != null) {
            UniWebView.SetZoomEnabled(zoomEnable);
        }        
#endif
    }

    /// <summary>
    /// 手機自適應模式
    /// </summary>
    /// <param name="overviewMode">自適應模式</param>
    public void SetLoadWithOverviewMode(bool overviewMode) {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (UniWebView != null) {
            UniWebView.SetLoadWithOverviewMode(overviewMode);
        }         
#endif
    }

    /// <summary>
    /// Zoom時的字體顯示比例
    /// </summary>
    /// <param name="textSizePercent">zoom時的字體顯示比例</param>
    public void SetTextZoom(int textSizePercent) {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (UniWebView != null) {
            UniWebView.SetTextZoom(textSizePercent);
        }         
#endif
    }


    /// <summary>
    /// 設定是否顯示載入的Loading輪子
    /// </summary>
    /// <param name="">設定是否顯示載入的Loading輪子</param>
    public void SetShowSpinnerWhileLoading(bool showSpinner) {
#if !UNITY_EDITOR
        if (UniWebView != null) {
            UniWebView.SetShowSpinnerWhileLoading(showSpinner);
        }         
#endif
    }

    /// <summary>
    /// 關閉WebView
    /// </summary>
    public void CloseWebView() {
        RemoveWebView();
    }

}