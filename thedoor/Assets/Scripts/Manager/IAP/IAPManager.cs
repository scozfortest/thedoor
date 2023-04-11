#if UNITY_IAP

#if UNITY_ANDROID || UNITY_IPHONE || UNITY_STANDALONE_OSX || UNITY_TVOS
// You must obfuscate your secrets using Window > Unity IAP > Receipt Validation Obfuscator
// before receipt validation will compile in this sample.
//#define RECEIPT_VALIDATION
#endif

#define DELAY_CONFIRMATION // Returns PurchaseProcessingResult.Pending from ProcessPurchase, then calls ConfirmPendingPurchase after a delay
//#define USE_PAYOUTS // Enables use of PayoutDefinitions to specify what the player should receive when a product is purchased
//#define INTERCEPT_PROMOTIONAL_PURCHASES // Enables intercepting promotional purchases that come directly from the Apple App Store
//#define SUBSCRIPTION_MANAGER //Enables subscription product manager for AppleStore and GooglePlay store

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Purchasing;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine.Purchasing.Security;
using TheDoor.Main;
using Scoz.Func;


/// <summary>
/// IAP manager. From Unity IAP Initialize.
/// </summary>
public class IAPManager : MonoSingletonA<IAPManager>, IIAPManager {
    protected override bool destroyWhenRepeated { get { return true; } }
    protected override bool dontDestroyOnLoad { get { return true; } }
    /// <summary>
    /// IAP 初始化成功
    /// </summary>
    public event Action OnIntializeSuccess;
    /// <summary>
    /// 平台購買成功
    /// </summary>
    public event Action<string, string, IPurchaseReceipt, string, Action<string>> OnPurchaseItemSuccess;
    /// <summary>
    /// 平台購買失敗
    /// </summary>
    public event Action<string> OnPurchaseItemFail;
    /// <summary>
    /// 購買的回呼
    /// </summary>
    public event Action PurchaseOKCallBack;

    // Unity IAP objects
    private IStoreController StoreController;
    /// <summary>
    /// App交易紀錄額外處理
    /// </summary>
    private IAppleExtensions AppleExtensions;
    /// <summary>
    /// 交易紀錄額外處理
    /// </summary>
    private ITransactionHistoryExtensions TransactionHistoryExtensions;
    /// <summary>
    /// Google交易紀錄額外處理
    /// </summary>
    private IGooglePlayStoreExtensions GooglePlayStoreExtensions;

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
    /// 最後交易的Id
    /// </summary>
    private string LastTransationID;
    /// <summary>
    /// 最後的訂單內容
    /// </summary>
    private string LastReceipt;

    /// <summary>
    /// 是否付費中
    /// </summary>
    private bool PurchaseInProgress;

    /// <summary>
    /// IAP是否已經進行初始化
    /// </summary>
    private bool IsInit = false;

    /// <summary>
    /// 商城商品UID
    /// </summary>
    private string ShopUID = string.Empty;

    //public bool PurchaseInProgress { get { return _PurchaseInProgress; } }

#if RECEIPT_VALIDATION // 是否要Client驗證訂單
    /// <summary>
    /// Local驗證訂單的跨平台驗證器(google, apple)
    /// </summary>
    private CrossPlatformValidator Validator;
#endif

    public string environment = "production";
    private bool InitUnityGameServicesSuccess = false;

    //async void Start() {
    //    try {
    //        var options = new InitializationOptions()
    //            .SetEnvironmentName(environment);
    //        InitUnityGameServicesSuccess = false;
    //        await UnityServices.InitializeAsync(options);
    //        InitUnityGameServicesSuccess = true;
    //        // IAP還沒初始化完成
    //        if (!IsInit) {
    //            DG.Tweening.DOVirtual.DelayedCall(5.0f, () => {
    //                Initialize();
    //            });
    //        }
    //    }
    //    catch (Exception exception) {
    //        // An error occurred during initialization.
    //        DebugLogger.LogError("InitUnityGameServicesSuccess Fail!!!");
    //    }
    //}

    public async void InitGameServices(Action<string, string, IPurchaseReceipt, string, Action<string>> _successCallBack, Action<string> _failCallBack) {
        try {

            if (!InitUnityGameServicesSuccess) {
                var options = new InitializationOptions()
                .SetEnvironmentName(environment);
                DebugLogger.LogWarning("Initalize UnityServices");
                await UnityServices.InitializeAsync(options);
                DebugLogger.LogWarning("Initalize End UnityServices");
                InitUnityGameServicesSuccess = true;

                // IAP還沒初始化完成
                if (!IsInit) {
                    OnPurchaseItemSuccess = _successCallBack;
                    OnPurchaseItemFail = _failCallBack;
                    Initialize();
                }
            }

        } catch (Exception exception) {
            // An error occurred during initialization.
            DebugLogger.LogError($"InitUnityGameServicesSuccess Fail!!! exception={exception}");
        }
    }

    /// <summary>
    /// This will be called when Unity IAP has finished initialising.
    /// </summary>
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions) {
        DebugLogger.LogWarning("[IAP OnInitialized] Init Shop IAP");

        StoreController = controller;
        AppleExtensions = extensions.GetExtension<IAppleExtensions>();
        TransactionHistoryExtensions = extensions.GetExtension<ITransactionHistoryExtensions>();
        GooglePlayStoreExtensions = extensions.GetExtension<IGooglePlayStoreExtensions>();

        // On Apple platforms we need to handle deferred purchases caused by Apple's Ask to Buy feature.
        // On non-Apple platforms this will have no effect; OnDeferred will never be called.
        AppleExtensions.RegisterPurchaseDeferredListener(OnDeferred); // ASK TO BUY >=IOS 8的時後 會有父母允許之後才能購買的選項 這個是用來註冊這部分的

#if SUBSCRIPTION_MANAGER
        Dictionary<string, string> introductory_info_dict = AppleExtensions.GetIntroductoryPriceDictionary();
#endif

        DebugLogger.Log("[IAP OnInitialized] IAP Available items:");
        foreach (var item in controller.products.all) {
            if (item.availableToPurchase) {
                DebugLogger.Log(string.Join(" - ",
                    new[]
                    {
                        item.metadata.localizedTitle,
                        item.metadata.localizedDescription,
                        item.metadata.isoCurrencyCode,
                        item.metadata.localizedPrice.ToString(),
                        item.metadata.localizedPriceString,
                        item.transactionID,
                        item.receipt
                    }));

#if INTERCEPT_PROMOTIONAL_PURCHASES
                // Set all these products to be visible in the user's App Store according to Apple's Promotional IAP feature
                // https://developer.apple.com/library/content/documentation/NetworkingInternet/Conceptual/StoreKitGuide/PromotingIn-AppPurchases/PromotingIn-AppPurchases.html
                AppleExtensions.SetStorePromotionVisibility(item, AppleStorePromotionVisibility.Show);
#endif

#if SUBSCRIPTION_MANAGER
                // this is the usage of SubscriptionManager class
                if (item.receipt != null) {
                    if (item.definition.type == ProductType.Subscription) {
                        if (checkIfProductIsAvailableForSubscriptionManager(item.receipt)) {
                            string intro_json = (introductory_info_dict == null || !introductory_info_dict.ContainsKey(item.definition.storeSpecificId)) ? null : introductory_info_dict[item.definition.storeSpecificId];
                            SubscriptionManager p = new SubscriptionManager(item, intro_json);
                            SubscriptionInfo info = p.getSubscriptionInfo();
                            DebugLogger.Log("product id is: " + info.getProductId());
                            DebugLogger.Log("purchase date is: " + info.getPurchaseDate());
                            DebugLogger.Log("subscription next billing date is: " + info.getExpireDate());
                            DebugLogger.Log("is subscribed? " + info.isSubscribed().ToString());
                            DebugLogger.Log("is expired? " + info.isExpired().ToString());
                            DebugLogger.Log("is cancelled? " + info.isCancelled());
                            DebugLogger.Log("product is in free trial peroid? " + info.isFreeTrial());
                            DebugLogger.Log("product is auto renewing? " + info.isAutoRenewing());
                            DebugLogger.Log("subscription remaining valid time until next billing date is: " + info.getRemainingTime());
                            DebugLogger.Log("is this product in introductory price period? " + info.isIntroductoryPricePeriod());
                            DebugLogger.Log("the product introductory localized price is: " + info.getIntroductoryPrice());
                            DebugLogger.Log("the product introductory price period is: " + info.getIntroductoryPricePeriod());
                            DebugLogger.Log("the number of product introductory price period cycles is: " + info.getIntroductoryPricePeriodCycles());
                        } else {
                            DebugLogger.Log("This product is not available for SubscriptionManager class, only products that are purchase by 1.19+ SDK can use this class.");
                        }
                    } else {
                        DebugLogger.Log("the product is not a subscription product");
                    }
                } else {
                    DebugLogger.Log("the product should have a valid receipt");
                }
#endif
            }
        }

        LogProductDefinitions();

        if (OnIntializeSuccess != null)
            OnIntializeSuccess();
    }

#if SUBSCRIPTION_MANAGER
    private bool checkIfProductIsAvailableForSubscriptionManager(string receipt) {
        var receipt_wrapper = (Dictionary<string, object>)MiniJson.JsonDecode(receipt);
        if (!receipt_wrapper.ContainsKey("Store") || !receipt_wrapper.ContainsKey("Payload")) {
            Debug.Log("The product receipt does not contain enough information");
            return false;
        }
        var store = (string)receipt_wrapper ["Store"];
        var payload = (string)receipt_wrapper ["Payload"];

        if (payload != null ) {
            switch (store) {
            case GooglePlay.Name:
                {
                    var payload_wrapper = (Dictionary<string, object>)MiniJson.JsonDecode(payload);
                    if (!payload_wrapper.ContainsKey("json")) {
                        Debug.Log("The product receipt does not contain enough information, the 'json' field is missing");
                        return false;
                    }
                    var original_json_payload_wrapper = (Dictionary<string, object>)MiniJson.JsonDecode((string)payload_wrapper["json"]);
                    if (original_json_payload_wrapper == null || !original_json_payload_wrapper.ContainsKey("developerPayload")) {
                        Debug.Log("The product receipt does not contain enough information, the 'developerPayload' field is missing");
                        return false;
                    }
                    var developerPayloadJSON = (string)original_json_payload_wrapper["developerPayload"];
                    var developerPayload_wrapper = (Dictionary<string, object>)MiniJson.JsonDecode(developerPayloadJSON);
                    if (developerPayload_wrapper == null || !developerPayload_wrapper.ContainsKey("is_free_trial") || !developerPayload_wrapper.ContainsKey("has_introductory_price_trial")) {
                        Debug.Log("The product receipt does not contain enough information, the product is not purchased using 1.19 or later");
                        return false;
                    }
                    return true;
                }
            case AppleAppStore.Name:
            case AmazonApps.Name:
            case MacAppStore.Name:
                {
                    return true;
                }
            default:
                {
                    return false;
                }
            }
        }
        return false;
    }
#endif

    /// <summary>
    /// This will be called when a purchase completes.
    /// </summary>
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e) {
        //if (!e.purchasedProduct.hasReceipt) {
        //    DebugLogger.Log("沒有請求");
        //    return PurchaseProcessingResult.Complete;
        //}
        DebugLogger.Log("請求 = " + e.purchasedProduct.hasReceipt);
        DebugLogger.Log("支付過 呼叫驗證");
        DebugLogger.Log("Purchase OK: " + e.purchasedProduct.definition.id);
        DebugLogger.Log("Receipt: " + e.purchasedProduct.receipt);

        LastTransationID = e.purchasedProduct.transactionID;
        LastReceipt = e.purchasedProduct.receipt;
        PurchaseInProgress = false;

        if (PurchaseOKCallBack != null) {
            PurchaseOKCallBack();
        }

        if (ShopUID.Equals(string.Empty)) {
            ShopUID = GamePlayer.Instance.MyHistoryData.BougthShopUID;
        }

#if RECEIPT_VALIDATION // Local validation is available for GooglePlay, and Apple stores
        if (IsCurrentStoreSupportedByValidator())
        {
            try
            {
                var result = Validator.Validate(e.purchasedProduct.receipt);
                DebugLogger.Log("Receipt is valid. Contents:");
                foreach (IPurchaseReceipt productReceipt in result)
                {
                    DebugLogger.Log($"productID={productReceipt.productID}");
                    DebugLogger.Log($"purchaseDate_Short={productReceipt.purchaseDate.ToShortDateString()}");
                    DebugLogger.Log($"transactionID={productReceipt.transactionID}");

                    string receipt = "";                                             
                    if (productReceipt is GooglePlayReceipt google)
                    {
                        DebugLogger.Log($"purchaseState={google.purchaseState.ToString()}");
                        DebugLogger.Log($"purchaseToken={google.purchaseToken}");
                        receipt = e.purchasedProduct.receipt;
                        DebugLogger.Log($"receipt={receipt}");
                    }

                    if (productReceipt is AppleInAppPurchaseReceipt apple)
                    {
                        DebugLogger.Log($"originalTransactionIdentifier={apple.originalTransactionIdentifier}");
                        DebugLogger.Log($"subscriptionExpirationDate_Short={apple.subscriptionExpirationDate.ToShortDateString()}");
                        DebugLogger.Log($"cancellationDate_Short={apple.cancellationDate.ToShortDateString()}");
                        DebugLogger.Log($"quantity={apple.quantity.ToString()}");
                        
                        AppleReceipt appleReceipt = JsonUtility.FromJson<AppleReceipt>(e.purchasedProduct.receipt);
                        receipt = appleReceipt.Payload;
                        DebugLogger.Log($"Payload={receipt}");
                    }

                    // For improved security, consider comparing the signed
                    // IPurchaseReceipt.productId, IPurchaseReceipt.transactionID, and other data
                    // embedded in the signed receipt objects to the data which the game is using
                    // to make this purchase.
                    DebugLogger.Log("開始驗證訂單 發送物品");
                    OnPurchaseItemSuccess?.Invoke(productReceipt.productID, ShopUID, productReceipt, receipt, ConfirmPendingPurchase);
                    //DebugLogger.Log("Purchase Success!");
                    //DebugLogger.Log("LastTransationID=" + LastTransationID);
                    OnPurchaseItemSuccess = null;
                }
            }
            catch (IAPSecurityException ex)
            {
                DebugLogger.LogError("Invalid receipt, not unlocking content. " + ex);
                OnPurchaseItemFail?.Invoke(ShopUID);
                OnPurchaseItemFail = null;
                return PurchaseProcessingResult.Complete;
            }
        }
#if UNITY_EDITOR
        else
        {
            if (OnPurchaseItemSuccess != null)
            {
                DebugLogger.Log("訂單完成???");
                OnPurchaseItemSuccess(e.purchasedProduct.definition.id, ShopUID, null, string.Empty, null);
                DebugLogger.Log("Purchase Success!");
                DebugLogger.Log("LastTransationID=" + LastTransationID);
                OnPurchaseItemSuccess = null;
            }
        }
#endif
#else
        // Local不驗證訂單 直接取訂單資訊Pass給Server驗證
        if (IsCurrentStoreSupportedByValidator()) {
            try {
                if (e.purchasedProduct != null && e.purchasedProduct.definition != null) {
                    DebugLogger.Log($"productID={e.purchasedProduct.definition.id}");
                    DebugLogger.Log($"transactionID={e.purchasedProduct.transactionID}");
                }

                string receipt = string.Empty;
                if (IsGooglePlayStoreSelected) {
                    receipt = e.purchasedProduct.receipt;
                    DebugLogger.Log($"Google receipt={e.purchasedProduct.receipt}");
                } else if (IsAppleAppStoreSelected) {
                    AppleReceipt appleReceipt = JsonUtility.FromJson<AppleReceipt>(e.purchasedProduct.receipt);
                    DebugLogger.Log($"Apple Payload={appleReceipt.Payload}");
                    //receipt = appleReceipt.Payload;
                    // 給Server驗整張訂單
                    receipt = e.purchasedProduct.receipt;
                    DebugLogger.Log($"Apple Receipt={receipt}");
                }

                // For improved security, consider comparing the signed
                // IPurchaseReceipt.productId, IPurchaseReceipt.transactionID, and other data
                // embedded in the signed receipt objects to the data which the game is using
                // to make this purchase.
                DebugLogger.Log("開始驗證訂單 發送物品");
                OnPurchaseItemSuccess?.Invoke(e.purchasedProduct.definition.id, ShopUID, null, receipt, ConfirmPendingPurchase);
                OnPurchaseItemSuccess = null;

            } catch (IAPSecurityException ex) {
                DebugLogger.LogError("Invalid receipt, not unlocking content. " + ex);
                OnPurchaseItemFail?.Invoke(ShopUID);
                OnPurchaseItemFail = null;
                return PurchaseProcessingResult.Complete;
            }
        }
#if UNITY_EDITOR
        else {
            if (OnPurchaseItemSuccess != null) {
                DebugLogger.Log("EDITOR 訂單完成");
                OnPurchaseItemSuccess(e.purchasedProduct.definition.id, ShopUID, null, string.Empty, null);
                DebugLogger.Log("Purchase Success!");
                DebugLogger.Log("LastTransationID=" + LastTransationID);
                OnPurchaseItemSuccess = null;
            }
        }
#endif
#endif
        // Unlock content from purchases here.
#if USE_PAYOUTS
        if (e.purchasedProduct.definition.payouts != null) {
            Debug.Log("Purchase complete, paying out based on defined payouts");
            foreach (var payout in e.purchasedProduct.definition.payouts) {
                Debug.Log(string.Format("Granting {0} {1} {2} {3}", payout.quantity, payout.typeString, payout.subtype, payout.data));
            }
        }
#endif
        // Indicate if we have handled this purchase.
        //   PurchaseProcessingResult.Complete: ProcessPurchase will not be called
        //     with this product again, until next purchase.
        //   PurchaseProcessingResult.Pending: ProcessPurchase will be called
        //     again with this product at next app launch. Later, call
        //     StoreController.ConfirmPendingPurchase(Product) to complete handling
        //     this purchase. Use to transactionally save purchases to a cloud
        //     game service.

#if DELAY_CONFIRMATION
        //等Serve清單驗證
        DebugLogger.Log(string.Format("IAP Pending {0}", e.purchasedProduct.definition.id));
        DebugLogger.Log("Receipt: " + e.purchasedProduct.receipt);
        AddPendingProducts(e.purchasedProduct);
        GamePlayer.Instance.MyHistoryData.SetBougthShopUID(ShopUID);
        return PurchaseProcessingResult.Pending;
#else
        DebugLogger.Log("IAP Complete Add m_PendingProductsList");
        return PurchaseProcessingResult.Complete;
#endif
    }

#if DELAY_CONFIRMATION
    private Dictionary<string, Product> PendingProducts = new Dictionary<string, Product>();

    /// <summary>
    /// 增加等待購買成功回應的商品
    /// </summary>
    /// <param name="p">商品</param>
    private void AddPendingProducts(Product p) {
        if (PendingProducts.ContainsKey(p.definition.id)) {
            DebugLogger.Log("AddPendingProducts Same IAP ID :" + p.definition.id);
            return;
        }
        PendingProducts.Add(p.definition.id, p);
    }

    /// <summary>
    /// 寫等待Server回應購買成功的商品Log
    /// </summary>
    private void Log_PendingProducts() {
        if (PendingProducts == null || PendingProducts.Count < 1)
            return;
        DebugLogger.Log("Log Pending Products");
        foreach (var item in PendingProducts) {
            DebugLogger.Log($"id: {item.Value.definition.id}\nstore-specific id: {item.Value.definition.storeSpecificId}\ntype: {item.Value.definition.type.ToString()}\n");
        }
    }

    // 確定完成購買物品 通知StoreController回報完成訂單
    public void ConfirmPendingPurchase(string id) {
        Product product = StoreController.products.WithID(id);
        if (product != null && product.availableToPurchase) {
            DebugLogger.Log("購買物品發送成功 完成訂單 ID=" + id);
            StoreController.ConfirmPendingPurchase(product);
            PurchaseInProgress = false;
            ShopUID = string.Empty;
            GamePlayer.Instance.MyHistoryData.SetBougthShopUID(string.Empty);
            PendingProducts.Remove(id);

            //#if APPSFLYER && !UNITY_EDITOR
            //            PurchaseData purchaseData = GameData.GetPurchaseData(id);
            //            if (purchaseData != null && purchaseData.MyItemData != null) {
            //                string currency = GetProductCurrencyCode(id);
            //                AppsFlyerManager.Inst.PurchaseIAP(purchaseData.MyItemData.Type.ToString(), id, purchaseData.Price.ToString(), purchaseData.MyItemData.Value.ToString(), currency, product.transactionID);
            //            } 
            //            else {
            //                DebugLogger.LogWarning($"[ConfirmPendingPurchase] purchaseData=Null id={id}");
            //            }           
            //#endif
        }
    }

#endif

    /// <summary>
    /// 購買失敗時呼叫 Called when a purchase fails.
    /// </summary>
    /// <param name="item">商品</param>
    /// <param name="r">失敗原因</param>
    public void OnPurchaseFailed(Product item, PurchaseFailureReason r) {
        DebugLogger.Log("Purchase failed: " + item.definition.id);
        DebugLogger.Log(r.ToString());

        // Detailed debugging information
        DebugLogger.Log($"Store specific error code: {TransactionHistoryExtensions.GetLastStoreSpecificPurchaseErrorCode()}");
        if (TransactionHistoryExtensions.GetLastPurchaseFailureDescription() != null) {
            DebugLogger.Log($"Purchase failure description message: {TransactionHistoryExtensions.GetLastPurchaseFailureDescription().message}");
        }

        PurchaseInProgress = false;

        DebugLogger.Log($"購買商品失敗 未完成訂單 商城商品UID={ShopUID}, 商品ID:{item.definition.id} 訂單編號:{item.transactionID}");
        if (OnPurchaseItemFail != null) {
            OnPurchaseItemFail(ShopUID);
            OnPurchaseItemFail = null;
        }
    }

    /// <summary>
    /// Called when Unity IAP encounters an unrecoverable initialization error.
    ///
    /// Note that this will not be called if Internet is unavailable; Unity IAP
    /// will attempt initialization until it becomes available.
    /// </summary>
    public void OnInitializeFailed(InitializationFailureReason error) {
        DebugLogger.Log("IAPManager failed to initialize!");
        switch (error) {
            case InitializationFailureReason.AppNotKnown:
                DebugLogger.LogError("您的應用是否正確上傳到相關的發布商控制台?");
                break;
            case InitializationFailureReason.PurchasingUnavailable:
                // Ask the user if billing is disabled in device settings.
                DebugLogger.Log("手機關閉內購功能!");
                break;
            case InitializationFailureReason.NoProductsAvailable:
                // Developer configuration error; check product metadata.
                DebugLogger.Log("沒有內購商品可購買!");
                break;
        }
    }

    public void Initialize() {
        if (IsInit) {
            DebugLogger.LogWarning("[IAP Initialize] Is Arleady Initalize");
            return;
        }

        // 等待GameServices初始化完成先
        if (!InitUnityGameServicesSuccess) {
            DebugLogger.LogWarning("[IAP Initialize] InitUnityGameServicesSuccess = false");
            return;
        }

        IsInit = true;

        DebugLogger.LogWarning("[IAP Initialize] Start Initialize()");

        var module = StandardPurchasingModule.Instance();

        // The FakeStore supports: no-ui (always succeeding), basic ui (purchase pass/fail), and
        // developer ui (initialization, purchase, failure code setting). These correspond to
        // the FakeStoreUIMode Enum values passed into StandardPurchasingModule.useFakeStoreUIMode.
        module.useFakeStoreUIMode = FakeStoreUIMode.StandardUser;

        var builder = ConfigurationBuilder.Instance(module);

        IsGooglePlayStoreSelected = StandardPurchasingModule.Instance().appStore == AppStore.GooglePlay;
        IsAppleAppStoreSelected = (StandardPurchasingModule.Instance().appStore == AppStore.AppleAppStore) ||
                                  (StandardPurchasingModule.Instance().appStore == AppStore.MacAppStore);

        DebugLogger.Log($"[IAP Initialize] IsGooglePlayStoreSelected={IsGooglePlayStoreSelected}, IsAppleAppStoreSelected={IsAppleAppStoreSelected}");

        // Define our products.
        // Either use the Unity IAP Catalog, or manually use the ConfigurationBuilder.AddProduct API.
        // Use IDs from both the Unity IAP Catalog and hardcoded IDs via the ConfigurationBuilder.AddProduct API.

        // In this case our products have the same identifier across all the App stores,
        // except on the Mac App store where product IDs cannot be reused across both Mac and
        // iOS stores.
        // So on the Mac App store our products have different identifiers,
        // and we tell Unity IAP this by using the IDs class.
        DebugLogger.LogWarning("[IAP Initialize] IAP Create Purchase Products");
        List<PurchaseData> purchaseDatas = GameData.GetPurchaseDatas(PurchaseData.Tag.All);
        List<string> productIds = new List<string>();
        //建商品表單
        if (purchaseDatas != null) {
            for (int i = 0; i < purchaseDatas.Count; i++) {
                if (!productIds.Contains(purchaseDatas[i].ProductUID)) {
                    builder.AddProduct(purchaseDatas[i].ProductUID, ProductType.Consumable);
                    productIds.Add(purchaseDatas[i].ProductUID);
                }
            }
        }

#if INTERCEPT_PROMOTIONAL_PURCHASES
        // On iOS and tvOS we can intercept promotional purchases that come directly from the App Store.
        // On other platforms this will have no effect; OnPromotionalPurchase will never be called.
        builder.Configure<IAppleConfiguration>().SetApplePromotionalPurchaseInterceptorCallback(OnPromotionalPurchase);
        DebugLogger.Log("Setting Apple promotional purchase interceptor callback");
#endif

#if RECEIPT_VALIDATION
        // 只有雙平台才可以Local驗證
        if (IsCurrentStoreSupportedByValidator()) {
            string appIdentifier;
#if UNITY_5_6_OR_NEWER
            appIdentifier = Application.identifier;
#else
            appIdentifier = Application.bundleIdentifier;
#endif
            Validator = new CrossPlatformValidator(GooglePlayTangle.Data(), AppleTangle.Data(), appIdentifier);
            DebugLogger.LogWarning($"[IAP Initialize] Validator={Validator}");
        }
#endif

        // Now we're ready to initialize Unity IAP.
        //Start init
        DebugLogger.LogWarning($"[IAP Initialize] UnityPurchasing Initialize Start");
        UnityPurchasing.Initialize(this, builder);
        DebugLogger.LogWarning($"[IAP Initialize] UnityPurchasing Initialize Done");

    }

    /// <summary>
    /// This will be called after a call to IAppleExtensions.RestoreTransactions().
    /// </summary>
    public void OnTransactionsRestored(bool success) {
        DebugLogger.Log("物品恢復");
        DebugLogger.Log("Transactions restored.");
    }

    /// <summary>
    /// iOS Specific.
    /// This is called as part of Apple's 'Ask to buy' functionality,
    /// when a purchase is requested by a minor and referred to a parent
    /// for approval.
    ///
    /// When the purchase is approved or rejected, the normal purchase events
    /// will fire.
    /// </summary>
    /// <param name="item">Item.</param>
    public void OnDeferred(Product item) {
        DebugLogger.Log("Purchase deferred: " + item.definition.id);
    }

#if INTERCEPT_PROMOTIONAL_PURCHASES
    private void OnPromotionalPurchase(Product item) {
        DebugLogger.Log("Attempted promotional purchase: " + item.definition.id);

        // Promotional purchase has been detected. Handle this event by, e.g. presenting a parental gate.
        // Here, for demonstration purposes only, we will wait five seconds before continuing the purchase.
        StartCoroutine(ContinuePromotionalPurchases());
    }

    private IEnumerator ContinuePromotionalPurchases()
    {
        DebugLogger.Log("Continuing promotional purchases in 5 seconds");
        yield return new WaitForSeconds(5);
        DebugLogger.Log("Continuing promotional purchases now");
        AppleExtensions.ContinuePromotionalPurchases (); // iOS and tvOS only; does nothing on Mac
    }
#endif

    /// <summary>
    /// 購買商品
    /// </summary>
    /// <param name="productID">平台商品ID</param>
    /// <param name="shopUID">商城商品UID</param>
    /// <param name="_successCallBack">購買成功回呼</param>
    /// <param name="_failCallBack">購買失敗回呼</param>
    /// <returns></returns>
    public bool PurchaseItem(string productID, string shopUID, Action<string, string, IPurchaseReceipt, string, Action<string>> _successCallBack, Action<string> _failCallBack) {

        if (PurchaseInProgress == true) {
            DebugLogger.Log("Please wait, purchase in progress");
            return true;
        }

        if (StoreController == null) {
            DebugLogger.LogError("Purchasing is not initialized");
            return false;
        }

        if (StoreController.products.WithID(productID) == null) {
            DebugLogger.LogError("No product has id " + productID);
            return false;
        }

        OnPurchaseItemSuccess = _successCallBack;
        OnPurchaseItemFail = _failCallBack;
        ShopUID = shopUID;
        DebugLogger.Log("Buy produce :" + productID);
        // Don't need to draw our UI whilst a purchase is in progress.
        // This is not a requirement for IAP Applications but makes the demo
        // scene tidier whilst the fake purchase dialog is showing.
        PurchaseInProgress = true;

        //Sample code how to add accountId in developerPayload to pass it to getBuyIntentExtraParams
        //Dictionary<string, string> payload_dictionary = new Dictionary<string, string>();
        //payload_dictionary["accountId"] = "Faked account id";
        //payload_dictionary["developerPayload"] = "Faked developer payload";
        //StoreController.InitiatePurchase(StoreController.products.WithID(productID), MiniJson.JsonEncode(payload_dictionary));
        StoreController.InitiatePurchase(StoreController.products.WithID(productID));
        return true;
    }

    /// <summary>
    /// 恢復已購買商品 (例如移除重新安裝 非消耗性的商品必須還給玩家 所以必須提供使用者一個Restore的按鍵)
    /// </summary>
    public void RestoreTransactions() {
        DebugLogger.Log("數據恢復");
        if (IsGooglePlayStoreSelected) {
            GooglePlayStoreExtensions.RestoreTransactions(OnTransactionsRestored);
        } else if (IsAppleAppStoreSelected) {
            AppleExtensions.RestoreTransactions(OnTransactionsRestored);
        }
    }

    /// <summary>
    /// 紀錄購買的商品
    /// </summary>
    private void LogProductDefinitions() {
        var products = StoreController.products.all;
        foreach (var product in products) {
            DebugLogger.Log(string.Format("id: {0}\nstore-specific id: {1}\ntype: {2}\nenabled: {3}\n", product.definition.id, product.definition.storeSpecificId, product.definition.type.ToString(), product.definition.enabled ? "enabled" : "disabled"));
        }
    }

    /// <summary>
    /// 取得幣值的iso 4217 Format ex. USD
    /// </summary>
    public string GetProductCurrencyCode(string productID) {
        if (StoreController != null) {
            Product product = StoreController.products.WithID(productID);
            if (product != null) {
                return product.metadata.isoCurrencyCode;
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// 取得商品是否有請求
    /// </summary>
    public bool GetProductHasReceipt(string productID) {
        if (StoreController != null) {
            Product product = StoreController.products.WithID(productID);
            if (product != null) {
                return product.hasReceipt;
            }
        }
        return false;
    }

    /// <summary>
    /// 取得商品價格
    /// </summary>
    public decimal GetProductPriceDecimal(string productID) {
        if (StoreController != null) {
            Product product = StoreController.products.WithID(productID);
            if (product != null) {
                return product.metadata.localizedPrice;
            }
        }

        return 1;
    }
    /// <summary>
    /// 取得商品價格
    /// </summary>
    public string GetProductPrice(string productID) {
        if (StoreController != null) {
            Product product = StoreController.products.WithID(productID);
            if (product != null) {
                return product.metadata.localizedPriceString;
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// IAP是否已經可用 (初始化成功)
    /// </summary>
    public bool IsIAPAvailable() {
        return StoreController != null;
    }

    public class AppleReceipt {
        public string Store;
        public string TransactionID;
        public string Payload;
    }

    private bool IsCurrentStoreSupportedByValidator() {
        //The CrossPlatform validator only supports the GooglePlayStore and Apple's App Stores.
        return IsGooglePlayStoreSelected || IsAppleAppStoreSelected;
    }
}
#endif