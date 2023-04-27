using System;

namespace TheDoor.Main {
    public enum Target {
        Myself,
        Enemy,
    }
    public enum Currency {
        Gold,
        Point,
    }
    public enum SaleState {
        OnSale,//上架中
        OffSale,//下架中
        ForTest,//測試商品，不會出現在Release版
    }
    public enum BuyLimitType {
        None,//無限定
        Permanence,//永久
        Daily,//每日
    }

    public enum LobbyUIs {
        Default,//預設介面
        CreateRole,//創腳介面
    }
    public enum AdventureUIs {
        Default,//預設介面
        Script,
        Battle,
        Rest,
    }
    public enum LinkUIType {
        None,//無
        Lobby,//大廳
        Lobby_MaJamPachinko,//大廳-麻將
        Lobby_Shop,//大廳-商城
        Lobby_Rest,//大廳-休息站
        Lobby_Achievement,//大廳-成就
        Lobby_Social,//大廳-社群
        Activity_Active,// 活動-活動頁籤
        Activity_Rank,//   活動-排行榜頁籤
        Activity_Rank_Gold,//   活動-排行榜-金幣排行榜頁籤
        Mail,//    信箱
        MiniGame,//    簽到 & 小遊戲
        Collection_Role,//  蒐藏-腳色
        Collection_Item,//  蒐藏-道具
        Collection_Title,//  蒐藏-稱號
        MaJamPachinko,//   麻將選房間介面
        Shop_LuckyBag,//   商城-福袋介面
        Shop_BuyGold,//    商城-購買金幣介面
        Shop_BuyPoint,//   商城-購買金鈔介面
        Social_Friend,//   社群_好友介面
        Social_Link,//   社群_外部連結介面
        Achievement_Career,//  生涯_生涯介面
        Achievement_Quest,//   生涯_每日任務介面
        Achievement_Achievement,// 生涯_成就介面
        Rest,//    休息站
        DailyReward,//簽到簿
        ScratchCard,//刮刮卡
        InvitationCode,//邀請碼
    }

    /// <summary>
    /// 顯示Google Ad廣告成功或失敗的回傳Enum訊息
    /// </summary>
    public enum AdsResultMessage {
        /// <summary>
        /// GoogleAd尚未初始化        
        /// </summary>
        GoogleAds_Not_Initialize,
        /// <summary>
        /// UnityAd尚未初始化        
        /// </summary>
        UnityAds_Not_Initialize,
        /// <summary>
        /// FacebookAd尚未初始化        
        /// </summary>
        FacebookAds_Not_Initialize,
        /// <summary>
        /// AppodealAd尚未初始化
        /// </summary>
        StartIoAds_Not_Initialize,
        /// <summary>
        /// GoogleAd已經有影片播放中
        /// </summary>
        Ads_AlreadyShowing,
        /// <summary>
        /// GoogleAds載入廣告失敗
        /// </summary>
        GoogleAdLoad_Fail,
        /// <summary>
        /// Unity載入廣告失敗
        /// </summary>
        UnityAdLoad_Fail,
        /// <summary>
        /// FacebookAd載入廣告失敗
        /// </summary>
        FacebookAdLoad_Fail,
        /// <summary>
        /// StartIoAd載入廣告失敗
        /// </summary>
        StartIoAdLoad_Fail,
        /// <summary>
        /// Google顯示廣告失敗
        /// </summary>
        GoogleAdShow_Fail,
        /// <summary>
        /// Unity顯示廣告失敗
        /// </summary>
        UnityAdShow_Fail,
        /// <summary>
        /// StartIo顯示廣告失敗
        /// </summary>
        StartIoAdShow_Fail,
        /// <summary>
        /// Google觀看廣告成功
        /// </summary>
        GoogleAdsWatchSuccess,
        /// <summary>
        /// Unity觀看廣告成功
        /// </summary>
        UnityAdsWatchSuccess,
        /// <summary>
        /// Facebook觀看廣告成功
        /// </summary>
        FacebookAdsWatchSuccess,
        /// <summary>
        /// AStartIo觀看廣告成功
        /// </summary>
        StartIoAdsWatchSuccess,
        /// <summary>
        /// 不觀看廣告直接給獎成功
        /// </summary>
        DontShowAdSuccess,
        /// <summary>
        /// 不觀看廣告直接給獎失敗
        /// </summary>
        DontShowAdFail,
        /// <summary>
        /// GoogleAds廣告還沒準備好
        /// </summary>
        GoogleAds_NotReady,
        /// <summary>
        /// UnityAds廣告還沒準備好
        /// </summary>
        UnityAds_NotReady,
        /// <summary>
        /// FacebookAds廣告還沒準備好
        /// </summary>
        FacebookAds_NotReady,
        /// <summary>
        /// StartIoAds廣告還沒準備好
        /// </summary>
        StartIoAds_NotReady,
    }

    /// <summary>
    /// 後臺網址類別
    /// </summary>
    public enum BackendURLType {
        BackendAddress,         // 後台網頁Address
        CustomerServiceURL,     // 客服網頁
        NewsURL,                // 公告
        RulesURL,               // 遊戲玩法
        SpecificationURL,       // 遊戲規範
        ProtectionPolicyURL,    // 隱私權政策
        UserContractURL,        // 使用者合約
        VIPInfoURL,             // 玩家當前VIP資訊說明
        ShopURL,                // 商城福袋機率等說明
        RuleInfoURL,            // 麻將遊戲規則說明
        LifeInfoURL,            // 生涯說明
        DailyURL,               // 簽到
    }
}