using System;
using System.Collections;
using System.Collections.Generic;
namespace TheDoor.Main {



    /// <summary>
    /// Firestore上的Collection
    /// </summary>
    public enum ColEnum {
        GameSetting,//遊戲設定
        NewsTicker,//跑馬燈資料
        MaJamRoom,//麻將房間設定
        Player,//玩家資料
        Role,//腳色
        Item,//道具
        Mail,//信件
        Shop,//商城
        Purchase,//儲值商城
        RoleCall,//腳色來電
        Friendship,//玩家關係
        History,//玩家紀錄
        MaJamMatchingRoom,//配對中的麻將房間
        MaJamMatchingInvite,//麻將配對邀請
        FriendRequest,//好友邀請
        Activity,//活動
        LeaderBoard,//排行榜
        PlayingRoom,//進行中的麻將房資料
        ScratchCard,//刮刮卡
        GiftCodeSetting,//邀請碼設定
        GiftCode,//邀請碼
        ADImg,//全版大圖廣告
    }
    /// <summary>
    /// 玩家擁有個人資料共用欄位
    /// </summary>
    public enum OwnedEnum {
        UID,
        OwnerUID,
        CreateTime,
    }
    /// <summary>
    /// GameData-Setting中的Doc名稱
    /// 有加到這個Enum中，遊戲開始時會自動去取Firestore上GameData-Setting中對應的文件
    /// </summary>
    public enum GameDataDocEnum {
        BTGamepad,//藍芽手把設定
        DailyReward,//簽到簿
        DefaultPlayer,//玩家初始設定
        HitTheDog,//小遊戲-擊娃娃
        StreetFighter,//小遊戲-巷戰古惑仔
        Room,//房間設定
        MaJamGame,//麻將
        NewsTicker,//跑馬燈設定
        RoleCall,//腳色來電
        SocialSetting,//社群設定
        Timer,//計時器相關設定
        Version,//版本
        BackendURL,// 後臺網址
        ADReward,//主動觀看廣告設定
        Guide,//教學
        TriggerEvent,//觸發事件
        ScheduledInGameNotification,//遊戲內推播通知
        Quest,//任務設定
        LeaderBoard,//排行榜
        InvitationCode,//邀請碼設定
        VIPRoom,//交易站設定
    }




}
