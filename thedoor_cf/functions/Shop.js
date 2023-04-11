//基本設定
const functions = require('firebase-functions');
const admin = require('firebase-admin');
//自訂方法
const GameSetting = require('./GameTools/GameSetting.js');
const PlayerItemManager = require('./GameTools/PlayerItemManager.js');
const GameDataManager = require('./GameTools/GameDataManager.js');
const FirestoreManager = require('./FirebaseTools/FirestoreManager.js');
const ArrayTool = require('./Scoz/ArrayTool.js');
const Logger = require('./GameTools/Logger.js');
//專案ID
const GCloudProject = process.env.GCLOUD_PROJECT

//購買商品
exports.Shop_Buy = functions.region('asia-east1').https.onCall(async (data, context) => {
    if (!context.auth) {
        console.warn("非Firebase註冊用戶");
        throw new functions.https.HttpsError(
            'unauthenticated',
            'only authenticated users can add requests'
        );
    }

    if (!("ShopUID" in data) || !("BuyCount" in data)) {
        console.log("格式錯誤");
        return { Result: "格式錯誤" };
    }

    let playerDocData = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.Player, context.auth.uid);
    let historyDocData = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.History, context.auth.uid);
    let shopDocData = await FirestoreManager.GetDocData(GameSetting.GameDataCols.Shop, data["ShopUID"]);

    //寫ShopBuy開始LOG
    let shopBuyLogDocName = context.auth.uid + "-" + Date.now();
    let beforeCurrencies = {
        Gold: playerDocData["Gold"],
        Ball: playerDocData["Ball"],
        Point: playerDocData["Point"],
    }
    await Logger.ShopBuy_Start(shopBuyLogDocName, context.auth.uid, data["ShopUID"], data["BuyCount"], shopDocData["Name"], shopDocData["ItemType"], shopDocData["ItemValue"], beforeCurrencies);//寫LOG

    //確認是否為有效商品
    if (shopDocData["SaleState"] == "OffSale") {
        console.log("非上架商品:" + data["ShopUID"]);
        return { Result: "非上架商品:" + data["ShopUID"] };
    } else if (shopDocData["SaleState"] == "ForTest") {
        if (GCloudProject == GameSetting.GCloudProjects.Release) {
            console.log("此為測試商品 不可用於正式版:" + data["ShopUID"]);
            return { Result: "非上架商品" };
        }
    }
    let nowTimestamp = admin.firestore.Timestamp.now();
    if ("StartTime" in shopDocData && nowTimestamp < shopDocData["StartTime"]) {
        console.log("尚未上架的商品:" + data["ShopUID"]);
        return { Result: "尚未上架的商品:" + data["ShopUID"] };
    }
    if ("EndTime" in shopDocData && nowTimestamp > shopDocData["EndTime"]) {
        console.log("已下架的商品:" + data["ShopUID"]);
        return { Result: "已下架的商品:" + data["ShopUID"] };
    }
    //設定價錢
    let PriceData = {};
    if (data["BuyCount"] == "One" && "Price_One" in shopDocData) {
        for (let key in shopDocData["Price_One"]) {
            if (key in GameSetting.CurrencyTypes)
                PriceData[key] = Number(shopDocData["Price_One"][key]);
        }
    } else if (data["BuyCount"] == "Ten" && "Price_Ten" in shopDocData) {
        for (let key in shopDocData["Price_Ten"]) {
            if (key in GameSetting.CurrencyTypes)
                PriceData[key] = Number(shopDocData["Price_Ten"][key]);
        }
    } else {
        console.log("購買類型錯誤:" + data["ShopUID"]);
        return { Result: "購買類型錯誤:" + data["ShopUID"] };
    }
    //確認玩家資源足夠
    for (let key in PriceData) {
        //console.log("需求" + key + ": " + PriceData[key]);
        //console.log("玩家擁有" + key + ": " + playerDocData[key]);
        if (Number(playerDocData[key]) < Number(PriceData[key])) {
            console.log("貨幣不足:" + key);
            return { Result: "貨幣不足:" + key };
        }
    }
    //確認商品在限購次數內
    let buyLimitType = "None";

    if (("BuyLimitType" in shopDocData) && ("BuyLimit" in shopDocData)) {//判斷此商品是不是限購商品

        buyLimitType = shopDocData["BuyLimitType"];
        let buyLimitTimes = Number(shopDocData["BuyLimit"]);
        if (buyLimitTimes <= 0) {//限購次數<=0就當沒有限購
            buyLimitType = "None";
        } else {

            switch (buyLimitType) {
                case "Daily"://每日限購
                    if (historyDocData != null && ("DailyLimitShopItems" in historyDocData)) {
                        let alreadyBuyTimes = ArrayTool.Count(historyDocData["DailyLimitShopItems"], data["ShopUID"]);//取得已經購買此商品次數
                        if (alreadyBuyTimes >= buyLimitTimes) {
                            console.log("已達每日限購次數");
                            return { Result: "已達每日限購次數" };
                        }
                    }
                    break;
                case "Permanence"://終身限購
                    if (historyDocData != null && ("LimitShopItems" in historyDocData)) {
                        let alreadyBuyTimes = ArrayTool.Count(historyDocData["LimitShopItems"], data["ShopUID"]);//取得已經購買此商品次數
                        if (alreadyBuyTimes >= buyLimitTimes) {
                            console.log("已達限購次數");
                            return { Result: "已達限購次數" };
                        }
                    }
                    break;
            }
        }
    }


    //檢查道具類型
    if (!(shopDocData["ItemType"] in GameSetting.ItemTypes)) {
        console.log("錯誤的道具類型: " + data["ShopUID"]);
        return { Result: "錯誤的道具類型: " + data["ShopUID"] };
    }


    //設定消耗貨幣
    let updatePlayerData = {};
    for (let key in PriceData) {
        let value = Number(PriceData[key]);
        if (value > 0) {
            //console.log(key + "消耗 " + value);
            updatePlayerData[key] = admin.firestore.FieldValue.increment(-value);
        }
    }

    //設定限購道具的已購買次數並寫入玩家PlayerData-History資料
    let updateHistoryData = {};
    if (buyLimitType != "None") {
        switch (buyLimitType) {
            case "Daily"://每日限購
                if (historyDocData != null) {
                    if ("DailyLimitShopItems" in historyDocData) {
                        let uids = historyDocData["DailyLimitShopItems"];
                        uids.push(data["ShopUID"]);
                        updateHistoryData["DailyLimitShopItems"] = uids;
                    } else {
                        updateHistoryData["DailyLimitShopItems"] = admin.firestore.FieldValue.arrayUnion(data["ShopUID"]);
                    }
                    await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.History, context.auth.uid, updateHistoryData);
                } else {
                    updateHistoryData["DailyLimitShopItems"] = [data["ShopUID"]];
                    await FirestoreManager.AddDoc_DesignatedDocName(GameSetting.PlayerDataCols.History, context.auth.uid, updateHistoryData);
                }
                break;
            case "Permanence"://終身限購
                if (historyDocData != null) {
                    if ("LimitShopItems" in historyDocData) {
                        let uids = historyDocData["LimitShopItems"];
                        uids.push(data["ShopUID"]);
                        updateHistoryData["LimitShopItems"] = uids;
                    } else {
                        updateHistoryData["LimitShopItems"] = admin.firestore.FieldValue.arrayUnion(data["ShopUID"]);
                    }
                    await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.History, context.auth.uid, updateHistoryData);
                } else {
                    updateHistoryData["LimitShopItems"] = [data["ShopUID"]];
                    await FirestoreManager.AddDoc_DesignatedDocName(GameSetting.PlayerDataCols.History, context.auth.uid, updateHistoryData);
                }
                break;
        }
    }

    //寫入玩家PlayerData-Player資料
    if (Object.keys(updatePlayerData).length != 0)
        await FirestoreManager.UpdateDoc(GameSetting.PlayerDataCols.Player, context.auth.uid, updatePlayerData);



    //寫ShopBuy進度LOG
    let costLogData = {};
    for (let key in PriceData) {
        costLogData["Cost" + key] = PriceData[key];
    }
    Logger.ShopBuy_AddProgress(shopBuyLogDocName, "消耗貨幣", costLogData);//寫LOG

    //給予玩家道具
    let returnGainItems = [];
    let replaceGainItems = [];//被取代的獎勵清單(此獎勵清單是本來有抽到 但因為重複道具而被取代的道具清單 回傳給cleint顯示用)
    //確認是否為十抽固定品項獎勵類型(十抽抽出來固定獲得這10個道具)
    let tenDrawSet = false;
    if (shopDocData["ItemType"] == GameSetting.ItemTypes.ItemGroup) {
        let itemGroupData = GameDataManager.GetData(GameSetting.GameJsonNames.ItemGroup, shopDocData["ItemValue"]);
        if (itemGroupData["Type"] == "TenDrawSet")
            tenDrawSet = true;
    }

    if (tenDrawSet == false) {
        if (data["BuyCount"] == "One") {
            returnGainItems = await PlayerItemManager.GiveItem(shopDocData["ItemType"], shopDocData["ItemValue"], 1, context.auth.uid, replaceGainItems);//給予玩家道具
        } else if (data["BuyCount"] == "Ten") {
            returnGainItems = await PlayerItemManager.GiveItem(shopDocData["ItemType"], shopDocData["ItemValue"], 10, context.auth.uid, replaceGainItems);//給予玩家道具10次
        }
    } else {//tenDrawSet為true時只會獲得一個寶箱且內容固定是該寶箱ID的那10個道具
        returnGainItems = await PlayerItemManager.GiveItem(shopDocData["ItemType"], shopDocData["ItemValue"], 1, context.auth.uid, replaceGainItems);//給予玩家道具
    }



    //寫ShopBuy進度LOG
    let logData = {
        GainItems: returnGainItems,
    }
    let newPlayerDocData = await FirestoreManager.GetDocData(GameSetting.PlayerDataCols.Player, context.auth.uid);
    logData["AfterCurrencies"] = {
        Gold: newPlayerDocData["Gold"],
        Ball: newPlayerDocData["Ball"],
        Point: newPlayerDocData["Point"],
    }
    Logger.ShopBuy_AddProgress(shopBuyLogDocName, "完成", logData);//寫LOG



    return {
        Result: GameSetting.ResultTypes.Success,
        Data: {
            ReturnGainItems: returnGainItems,
            ReplaceGainItems: replaceGainItems,
        }
    };

});