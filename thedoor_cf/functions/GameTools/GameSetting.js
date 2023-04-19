module.exports = {
    //獨立資料類道具，獨立資料類代表每個道具有一筆獨立doc，不是記錄在PlayerData-Item裡的道具
    //獨立資料類道具的名稱必須要跟PlayerDataCols裡的Key對應
    UniqueItemTypes: Object.freeze({
        Role: "Role",
        Supply: "Supply",
    }),
    //專案PackageName
    GCloudProjects: Object.freeze({
        Dev: "thedoor-develop",
        Test: "thedoor-test",
        Release: "thedoor-release",
    }),
    //專案名稱
    PackageNames: Object.freeze({
        Dev: "com.among.thedoordev",
        Release: "com.among.thedoorrelease",
    }),

    //GameData Json表名稱
    //Json表字典有新增時，GameDataManager.GetData也要加
    GameJsonNames: Object.freeze({
        ItemGroup: "ItemGroup",
        Role: "Role",
        Supply: "Supply",
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
        History: "PlayerData-History",
        Role: "PlayerData-Role",
        Supply: "PlayerData-Supply",
        OnlineTimestamp: "PlayerData-OnlineTimestamp",
    }),
    //道具類型(ItemType)
    ItemTypes: Object.freeze({
        Gold: "Gold",//遊戲幣
        Point: "Point",//點數
        ItemGroup: "ItemGroup",//寶箱
    }),
    //遊戲中貨幣類型
    CurrencyTypes: Object.freeze({
        Gold: "Gold",
        Point: "Point",
    }),


    //遊戲Log集合
    GameLogCols: Object.freeze({
        CFLog: "GameLog-CloudFunc",
        Signup: "GameLog-Signup",
        Signin: "GameLog-Signin",
        Signout: "GameLog-Signout",
        CreateRole: "GameLog-CreateRole",
        ShopBuy: "GameLog-ShopBuy",
        Purchase: "GameLog-Purchase",
        PayToken: "GameLog-PayToken",
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
    }),
    //回傳Client結果類型(此值有更改需要連同Client的Enum一起改)
    ResultTypes: Object.freeze({
        Fail: "Fail",//Fail時Data通常是回傳給Client顯示的文字內容
        Success: "Success",//Success時Data通常是回傳給Client需要的資料
    }),

};









