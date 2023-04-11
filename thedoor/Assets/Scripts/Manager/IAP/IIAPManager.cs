#if UNITY_IAP
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;
using System;

public interface IIAPManager : IStoreListener
{
    /// <summary>
    /// 用來初始化StandardPurchasingModule,ConfigurationBuilder, UnityPurchasing.Initialize
    /// </summary>
    void Initialize();

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
    void OnDeferred(Product item);

    /// <summary>
    /// 用來Confirm PendingPurchase
    /// </summary>
    /// <param name="id"></param>
    void ConfirmPendingPurchase(string id);

    /// <summary>
    /// This will be called after a call to IAppleExtensions.RestoreTransactions().
    /// </summary>
    /// <param name="success"></param>
    void OnTransactionsRestored(bool success);

    /// <summary>
    /// 購買商品
    /// </summary>
    /// <param name="productID">商品ID</param>
    /// <param name="shopUID">商城商品UID</param>
    /// <param name="_successCallBack">購買成功回呼</param>
    /// <param name="_failCallBack">購買失敗回呼</param>
    /// <returns></returns>
    bool PurchaseItem(string productID, string shopUID, Action<string, string, IPurchaseReceipt, string, Action<string>> _successCallBack, Action<string> _failCallBack);

    /// <summary>
    /// 恢復已購買商品 (例如移除重新安裝 非消耗性的商品必須還給玩家 所以必須提供使用者一個Restore的按鍵)
    /// </summary>
    void RestoreTransactions();

    /// <summary>
    /// 取得幣值的iso 4217 Format ex. USD
    /// </summary>
    /// <param name="productID"></param>
    /// <returns></returns>
    string GetProductCurrencyCode(string productID);

    /// <summary>
    /// 取得商品價格
    /// </summary>
    /// <param name="productID"></param>
    /// <returns></returns>
    string GetProductPrice(string productID);

    /// <summary>
    /// IAP是否已經可用 (初始化成功)
    /// </summary>
    bool IsIAPAvailable();
}
#endif