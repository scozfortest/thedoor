#if UNITY_IAP
#if UNITY_ANDROID || UNITY_IPHONE || UNITY_STANDALONE_OSX || UNITY_TVOS
// You must obfuscate your secrets using Window > Unity IAP > Receipt Validation Obfuscator
// before receipt validation will compile in this sample.
#define RECEIPT_VALIDATION
#endif
#define DELAY_CONFIRMATION // Returns PurchaseProcessingResult.Pending from ProcessPurchase, then calls ConfirmPendingPurchase after a delay
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Purchasing;
#if RECEIPT_VALIDATION
using UnityEngine.Purchasing.Security;
#endif
using TheDoor.Main;

public class IAPStoreController : IStoreController
{
   public  ProductCollection products { get; }
    public void ConfirmPendingPurchase(Product product) {

    }
    public void FetchAdditionalProducts(HashSet<ProductDefinition> products, Action successCallback, Action<InitializationFailureReason> failCallback) { }
    public void InitiatePurchase(Product product, string payload) { }
    public void InitiatePurchase(string productId, string payload) { }
    public  void InitiatePurchase(Product product) { }
    public  void InitiatePurchase(string productId) { }
}


public class IAPAppleStoreController : IAppleExtensions {
    public bool simulateAskToBuy { get; set; }             

    Dictionary<string, string> StorData;
    public void ContinuePromotionalPurchases() { }
    public void FetchStorePromotionOrder(Action<List<Product>> successCallback, Action errorCallback) { }
    public void FetchStorePromotionVisibility(Product product, Action<string, AppleStorePromotionVisibility> successCallback, Action errorCallback) { }

    public Dictionary<string, string> GetIntroductoryPriceDictionary() { return StorData; }
    public Dictionary<string, string> GetProductDetails() { return StorData; }
    public string GetTransactionReceiptForProduct(Product product) { return ""; }
    public void PresentCodeRedemptionSheet() { }
    public void RefreshAppReceipt(Action<string> successCallback, Action errorCallback) { }
    public void RegisterPurchaseDeferredListener(Action<Product> callback) { }
    public void RestoreTransactions(Action<bool> callback) { }
    public void SetApplicationUsername(string applicationUsername) { }
    public void SetStorePromotionOrder(List<Product> products) { }
    public void SetStorePromotionVisibility(Product product, AppleStorePromotionVisibility visible) { }
}

#endif
