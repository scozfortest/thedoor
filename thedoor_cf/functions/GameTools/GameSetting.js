module.exports = {
    //獨立資料類道具，獨立資料類代表每個道具有一筆獨立doc，不是記錄在PlayerData-Item裡的道具
    //獨立資料類道具的名稱必須要跟PlayerDataCols裡的Key對應
    UniqueItemTypes: Object.freeze({
        Role: "Role",
    }),
    //專案PackageName
    GCloudProjects: Object.freeze({
        Dev: "majampachinko-develop",
        Test: "majampachinko-test1",
        Release: "majampachinko-release",
    }),
    //專案名稱
    PackageNames: Object.freeze({
        Dev: "com.among.majampachinkodev",
        Release: "com.among.majampachinkorelease",
    }),

    //GameData Json表名稱
    //Json表字典有新增時，GameDataManager.GetData也要加
    GameJsonNames: Object.freeze({
        ItemGroup: "ItemGroup",
        Role: "Role",
        Voice: "Voice",
        Icon: "Icon",
        Hand: "Hand",
        Emoji: "Emoji",
        Stuff: "Stuff",
        Title: "Title",
        AIPlayer: "AIPlayer",
        DailyReward: "DailyReward",
        VIP: "VIP",
        PlayerLv: "PlayerLV",
        Quest: "Quest",
    }),
    //遊戲資料集合
    GameDataCols: Object.freeze({
        Setting: "GameData-Setting",
        MaJamRoom: "GameData-MaJamRoom",
        NewsTicker: "GameData-NewsTicker",
        Shop: "GameData-Shop",
        Purchase: "GameData-Purchase",
        DailyReward: "GameData-DailyReward",
        LeaderBoard: "GameData-LeaderBoard",
        ScratchCard: "GameData-ScratchCard",
        GiftCode: "GameData-GiftCode",
        GiftCodeSetting: "GameData-GiftCodeSetting",
    }),
    //玩家資料集合
    PlayerDataCols: Object.freeze({
        Player: "PlayerData-Player",
        Item: "PlayerData-Item",
        Friendship: "PlayerData-Friendship",
        History: "PlayerData-History",
        Mail: "PlayerData-Mail",
        RoleCall: "PlayerData-RoleCall",
        Role: "PlayerData-Role",
        MaJamMatchingRoom: "PlayerData-MaJamMatchingRoom",
        OnlineTimestamp: "PlayerData-OnlineTimestamp",
        MaJamMatchingInvite: "PlayerData-MaJamMatchingInvite",
        FriendRequest: "PlayerData-FriendRequest",
        PlayerTmpBag: "PlayerData-PlayerTmpBag",
    }),
    //道具類型(ItemType)
    ItemTypes: Object.freeze({
        Gold: "Gold",//遊戲幣
        Point: "Point",//點數
        Ball: "Ball",//小鋼珠
        Role: "Role",//腳色
        Voice: "Voice",//打牌中語音
        Emoji: "Emoji",//打牌中貼圖
        Icon: "Icon",//頭像
        Hand: "Hand",//自摸的瞇牌手
        Stuff: "Stuff",//消耗品
        Title: "Title",//稱號
        ItemGroup: "ItemGroup",//寶箱
    }),
    //遊戲中貨幣類型
    CurrencyTypes: Object.freeze({
        Gold: "Gold",
        Point: "Point",
        Ball: "Ball",
    }),


    //遊戲Log集合
    GameLogCols: Object.freeze({
        CFLog: "GameLog-CloudFunc",
        Signup: "GameLog-Signup",
        Signin: "GameLog-Signin",
        Signout: "GameLog-Signout",
        PlayerChangeName: "GameLog-PlayerChangeName",
        PlayerChangeIntro: "GameLog-PlayerChangeIntro",
        ShopBuy: "GameLog-ShopBuy",
        Purchase: "GameLog-Purchase",
        PayToken: "GameLog-PayToken",
        ClaimMail: "GameLog-ClaimMail",
        RemoveMail: "GameLog-RemoveMail",
        RoleCallReward: "GameLog-RoleCallReward",
        CreateRoom: "GameLog-CreateRoom",
        MaJamGameTrack: "GameLog-MaJamGameTrack",
        MaJamGameSettlement: "GameLog-MaJamGameSettlement",
        DailyReward: "GameLog-DailyReward",
        HitTheDog: "GameLog-HitTheDog",
        StreetFighter: "GameLog-StreetFighter",
        SendPoint: "GameLog-SendPoint",
        GetLVReward: "GameLog-GetLVReward",
        Quest_FinishProgress: "GameLog-Quest_FinishProgress",
        Quest_Claim: "GameLog-Quest_Claim",
        CombineRole: "GameLog-CombineRole",
        GetWatchADReward: "GameLog-WatchADReward",
        Schedule: "GameLog-Schedule",
        ScratchCardReward: "GameLog-ScratchCardReward",
        LeaderBoardReward: "GameLog-LeaderBoardReward",
        VIP_AddEXP: "GameLog-VIPAddEXP",
        GiftCode: "GameLog-GiftCode",//輸入序號的人收到禮物信件的Log
        InvitationCode: "GameLog-InvitationCode",//序號擁有人收到禮物信件的Log
    }),

    //遊戲報表集合
    GameReportCols: Object.freeze({
        VIP_Month: "Report-VIP_Month",
    }),

    //註冊類型
    SignupType: Object.freeze({
        Game: "Game",//從遊戲中註冊
        Website: "Website",//從網頁註冊
    }),
    //Log類型
    LogTypes: Object.freeze({
        Normal: "Normal",
        Warn: "Warn",
        Error: "Error",
    }),
    //玩家目前狀態類型(此值有更改需要連同Client的Enum一起改)
    PlayerStateTypes: Object.freeze({
        Online: "Online",
        Offline: "Offline",
        Adventuring: "Adventuring",
    }),
    //回傳Client結果類型(此值有更改需要連同Client的Enum一起改)
    ResultTypes: Object.freeze({
        Fail: "Fail",//Fail時Data通常是回傳給Client顯示的文字內容
        Success: "Success",//Success時Data通常是回傳給Client需要的資料
    }),

    //NewsTickerMsg類型
    NewsTickerMsgTypes: Object.freeze({
        WinTypeMsg: "WinTypeMsg",
        WinRankMsg: "WinRankMsg",
        NormalGameMsg: "NormalGameMsg",
        NormalGame1Msg: "NormalGame1Msg",
        NormalGame2Msg: "NormalGame2Msg",
        WinningStreakMsg: "WinningStreakMsg",
        TitleMsg: "TitleMsg",
        LuckyBagMsg: "LuckyBagMsg",
        ItemMsg: "ItemMsg",
    }),

    //任務目錄類別
    QuestCategoryType: Object.freeze({
        Achievement: "Achievement",
        DailyQuest: "DailyQuest",
    }),

    //任務類別
    QuestTaskType: Object.freeze({
        PlayRoomType: "PlayRoomType",
        SelfDraw: "SelfDraw",
        WinType: "WinType",
        WatchAD: "WatchAD",
        PlayMiniGame: "PlayMiniGame",
        Post: "Post",
        FinalWin: "FinalWin",
        Buy: "Buy",
        Purchase: "Purchase",
        RoleCall: "RoleCall",
        GetRole: "GetRole",
    }),

    //VIP等級類型
    VIPType: Object.freeze({
        Active: "Active",
        Purchase: "Purchase"
    }),

    //MonthlyVIPEXPBonusType
    MonthlyVIPEXPBonusType: Object.freeze({
        FirstPurchase: "FirstPurchase"
    }),

    //後臺資料
    BackstageDataCols: Object.freeze({
        Schedule: "BackstageData-Schedule"
    }),

    //交易系統資料
    ExchangesDataCols: Object.freeze({
        Setting: "ExchangesData-Setting"
    }),

    //交易系統LOG
    ExchangesLogCols: Object.freeze({
        TradeLog: "ExchangesLog-TradeLog"
    }),

};









